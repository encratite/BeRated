using Owin;

namespace BeRated.Server
{
    class WebServerApp
    {
        private IServerInstance _Instance;

        public WebServerApp(IServerInstance instance)
        {
            _Instance = instance;
        }

        public void Configuration(IAppBuilder app)
        {
            app.Use(typeof(Middleware), _Instance);
        }
    }
}
