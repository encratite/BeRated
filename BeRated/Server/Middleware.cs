using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ashod;
using Microsoft.Owin;

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
			var cookies = new Dictionary<string, string>();
			foreach (var pair in context.Request.Cookies)
				cookies.Add(pair.Key, pair.Value);
			Context.Current = new Context
			{
				Cookies = cookies,
			};
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
				string markup = _Instance.GetCachedResponse(context);
				if (markup == null)
				{
					TimeSpan invokeDuration;
					TimeSpan renderDuration;
					markup = ProcessRequest(uri, path, out invokeDuration, out renderDuration);
					_Instance.OnResponse(context, markup, invokeDuration, renderDuration);
				}
				response.ContentType = "text/html";
                var task = context.Response.WriteAsync(markup);
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
			finally
			{
				Context.Current = null;
			}
        }

		private string ProcessRequest(Uri uri, string path, out TimeSpan invokeDuration, out TimeSpan renderDuration)
		{
			string markup;
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
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			object model = Invoke(method, arguments, out modelType);
			stopwatch.Stop();
			invokeDuration = stopwatch.Elapsed;
			stopwatch.Start();
			markup = _Instance.Render(uri.AbsolutePath, modelType, model);
			stopwatch.Stop();
			renderDuration = stopwatch.Elapsed;
			markup = markup.Replace("\r", "");
			var whitespacePattern = new Regex(@"^\s+|\n{2,}", RegexOptions.ECMAScript | RegexOptions.Multiline);
			markup = whitespacePattern.Replace(markup, "");
			return markup;
		}

		private object Invoke(string method, Dictionary<string, string> arguments, out Type modelType)
        {
            var notFoundException = new MiddlewareException("No such method.");
            var methodInfo = _Instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public);
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
            modelType = methodInfo.ReturnType;
            return output;
        }
    }
}
