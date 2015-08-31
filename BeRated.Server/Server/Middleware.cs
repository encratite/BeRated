using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ashod;
using Microsoft.Owin;
using System.Diagnostics;

namespace BeRated.Server
{
    public class Middleware : OwinMiddleware
    {
        private BaseApp _Instance;

        public Middleware(OwinMiddleware next, BaseApp instance)
            : base(next)
        {
            _Instance = instance;
        }

        public override Task Invoke(IOwinContext context)
        {
            var response = context.Response;
            try
            {
                var uri = context.Request.Uri;
                string path = uri.PathAndQuery;
                if (path.Length == 0)
                    throw new MiddlewareException("Path is empty.");
                if (path == "/favicon.ico")
                {
                    response.StatusCode = 404;
                    return response.WriteAsync("Not found.");
                }
                var requestPattern = new Regex(@"^/(?<method>\w+?)(?:\?(?:(?<firstArgument>\w+?)=(?<firstValue>[^?=&]*))(?:&(?<arguments>\w+?)=(?<values>[^?=&]*))*)?$", RegexOptions.ECMAScript);
                var match = requestPattern.Match(path);
                if (!match.Success)
                    throw new MiddlewareException("Malformed request.");
                var groups = match.Groups;
                var methodGroup = groups["method"];
                var firstArgumentGroup = groups["firstArgument"];
                var firstValueGroup = groups["firstValue"];
                var argumentGroup = groups["arguments"];
                var valueGroup = groups["values"];
                string method = methodGroup.Value;
                var arguments = new Dictionary<string, string>();
                if (firstArgumentGroup.Success)
                {
                    arguments[firstArgumentGroup.Value] = firstValueGroup.Value;
                    var valueEnumerator = valueGroup.Captures.GetEnumerator();
                    valueEnumerator.MoveNext();
                    foreach (Capture argument in argumentGroup.Captures)
                    {
                        var value = (Capture)valueEnumerator.Current;
                        arguments[argument.Value] = value.Value;
                        valueEnumerator.MoveNext();
                    }
                }
                Type modelType;
                var watch = new PerformanceWatch();
                object model = Invoke(method, arguments, out modelType);
                watch.Print("Controller");
                string markup = _Instance.Render(uri.AbsolutePath, modelType, model);
                watch.Print("Render");
				markup = markup.Replace("\r", "");
				var whitespacePattern = new Regex(@"^\s+|\n{2,}", RegexOptions.ECMAScript | RegexOptions.Multiline);
				markup = whitespacePattern.Replace(markup, "");
                response.ContentType = "text/html";
                var task = context.Response.WriteAsync(markup);
                watch.Print("Post-processing");
                return task;
            }
            catch (Exception exception)
            {
				var targetInvocationException = exception as TargetInvocationException;
				if (targetInvocationException != null)
					exception = targetInvocationException.InnerException;
                Logger.Error("Request error: {0} ({1})", exception.Message, exception.GetType());
                response.StatusCode = 500;
                response.ContentType = "text/plain";
                string message;
				string remoteAddress = context.Request.RemoteIpAddress;
				bool isLocal = remoteAddress == "127.0.0.1" || remoteAddress == "::1";
                if (exception.GetType() == typeof(MiddlewareException) || isLocal)
                    message = string.Format("{0}\n{1}", exception.Message, exception.StackTrace);
                else
                    message = "An error occurred.";

                return response.WriteAsync(message);
            }
        }

        private object Invoke(string method, Dictionary<string, string> arguments, out Type modelType)
        {
            var notFoundException = new MiddlewareException("No such method.");
            var watch = new PerformanceWatch();
            var methodInfo = _Instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public);
            watch.Print("GetMethod");
            if (methodInfo == null)
                throw notFoundException;
            var attribute = methodInfo.GetCustomAttribute(typeof(ControllerAttribute));
            if (attribute == null)
                throw notFoundException;
            var parameters = methodInfo.GetParameters();
            var invokeParameters = new List<object>();
            foreach (var parameter in parameters)
            {
                string argument;
                object convertedParameter;
                var type = parameter.ParameterType;
                bool isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
                if (arguments.TryGetValue(parameter.Name, out argument))
                {
                    if (isNullable)
                        type = type.GenericTypeArguments.First();
                    convertedParameter = Convert.ChangeType(argument, type);
                }
                else
                {
                    if (type == typeof(string) || isNullable)
                    {
                        convertedParameter = null;
                    }
                    else
                    {
                        string message = string.Format("Parameter \"{0}\" has not been specified.", parameter.Name);
                        throw new MiddlewareException(message);
                    }
                }
                invokeParameters.Add(convertedParameter);
            }
            var output = methodInfo.Invoke(_Instance, invokeParameters.ToArray());
            watch.Print("Invoke");
            modelType = methodInfo.ReturnType;
            return output;
        }
    }
}
