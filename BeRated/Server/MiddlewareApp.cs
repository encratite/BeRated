using Owin;

namespace BeRated.Server
{
    class MiddlewareApp
    {
        private BaseApp _Instance;

        public MiddlewareApp(BaseApp instance)
        {
            _Instance = instance;
        }

        public void Configuration(IAppBuilder app)
        {
            app.Use(typeof(Middleware), _Instance);
        }
    }
}
