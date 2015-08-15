using System;
using Microsoft.Owin.Hosting;
using Owin;

namespace BeRated.Server
{
    class WebServer : IDisposable
    {
        private IServerInstance _Instance;
        private string _Url;

        private IDisposable _WebApp = null;

        public WebServer(IServerInstance instance, string url)
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
                var webServerApp = new WebServerApp(_Instance);
                webServerApp.Configuration(app);
            });
        }
    }
}
