using Microsoft.Owin.Hosting;
using System;

namespace BeRated.Server
{
    class WebAppLauncher : IDisposable
    {
        private BaseApp _Instance;
        private string _Url;

        private IDisposable _WebApp = null;

        public WebAppLauncher(BaseApp instance, string url)
        {
            _Instance = instance;
            _Url = url;
        }

        void IDisposable.Dispose()
        {
            if (_Instance != null)
            {
                _Instance.Dispose();
                _Instance = null;
            }
            if (_WebApp != null)
            {
                _WebApp.Dispose();
                _WebApp = null;
            }
        }

        public void Start()
        {
            var options = new StartOptions(_Url);
            WebApp.Start(_Url, (app) =>
            {
                var webServerApp = new MiddlewareApp(_Instance);
                webServerApp.Configuration(app);
            });
        }
    }
}
