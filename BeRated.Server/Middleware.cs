using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                var path = context.Request.Path;
                if (!path.HasValue)
                    throw new MiddlewareException("Path not set.");
                string pathString = path.Value;
                if (pathString.Length == 0)
                    throw new MiddlewareException("Path is empty.");
                var pathTokens = pathString.Substring(1).Split('/');
                if (pathTokens.Length == 0)
                    throw new MiddlewareException("Invalid number of path tokens.");
                object output = InvokeServerInstance(pathTokens);
                string json = JsonConvert.SerializeObject(output);
                string markup = _Instance.GetMarkup(json);
                response.ContentType = "text/html";
                var task = context.Response.WriteAsync(markup);
                return task;
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

        private object InvokeServerInstance(string[] pathTokens)
        {
            string methodName = pathTokens.First();
            var arguments = pathTokens.Skip(1).ToArray();
            var notFoundException = new MiddlewareException("No such method.");
            var method = _Instance.GetType().GetMethod(methodName, BindingFlags.Public);
            if (method == null)
                throw notFoundException;
            var attribute = method.GetCustomAttribute(typeof(ServerMethodAttribute));
            if (attribute == null)
                throw notFoundException;
            var parameters = method.GetParameters();
            if (arguments.Length != parameters.Length)
                throw new MiddlewareException("Invalid argument count.");
            var convertedParameters = new List<object>();
            for (int i = 0; i < arguments.Length; i++)
            {
                string argument = arguments[i];
                var parameter = parameters[i];
                var convertedParameter = ConvertParameter(argument, parameter.ParameterType);
                convertedParameters.Add(convertedParameter);
            }
            var output = method.Invoke(_Instance, convertedParameters.ToArray());
            return output;
        }

        private object ConvertParameter(string input, Type parameterType)
        {
            // To do: support integers
            return input;
        }
    }
}
