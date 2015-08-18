using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace BeRated.Server
{
    public class Middleware : OwinMiddleware
    {
        private IServerInstance _Instance;

        public Middleware(OwinMiddleware next, IServerInstance instance)
            : base(next)
        {
            _Instance = instance;
        }

        public override Task Invoke(IOwinContext context)
        {
            var response = context.Response;
            try
            {
                string path = context.Request.Uri.PathAndQuery;
                if (path.Length == 0)
                    throw new MiddlewareException("Path is empty.");
                if (path == "/favicon.ico")
                {
                    response.StatusCode = 404;
                    return response.WriteAsync("Not found.");
                }
                var pattern = new Regex(@"^/(?<method>\w+?)(?:\?(?:(?<firstArgument>\w+?)=(?<firstValue>[^?=&]*))(?:&(?<arguments>\w+?)=(?<values>[^?=&]*))*)?$", RegexOptions.ECMAScript);
                var match = pattern.Match(path);
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
                object model = InvokeServerInstance(method, arguments);
                throw new NotImplementedException();
                /*
                response.ContentType = "text/html";
                var task = context.Response.WriteAsync(markup);
                return task;
                */
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error: {0} ({1})", exception.Message, exception.GetType());
                response.StatusCode = 500;
                response.ContentType = "text/plain";
                string message;
                if (exception.GetType() == typeof(MiddlewareException))
                    message = exception.Message;
                else
                    message = "An error occurred.";

                return response.WriteAsync(message);
            }
        }

        private object InvokeServerInstance(string method, Dictionary<string, string> arguments)
        {
            var notFoundException = new MiddlewareException("No such method.");
            var methodInfo = _Instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public);
            if (methodInfo == null)
                throw notFoundException;
            var attribute = methodInfo.GetCustomAttribute(typeof(ServerMethodAttribute));
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
            return output;
        }
    }
}
