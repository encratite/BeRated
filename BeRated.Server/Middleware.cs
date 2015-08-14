using System;
using System.Threading.Tasks;
using Microsoft.Owin;

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
            var task = context.Response.WriteAsync("Success");
            return task;
        }
    }
}
