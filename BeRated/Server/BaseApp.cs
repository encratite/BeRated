using System;
using Microsoft.Owin;

namespace BeRated.Server
{
    public abstract class BaseApp : IDisposable
    {
        private TemplateManager _TemplateManager = null;

		public virtual string GetCachedResponse(IOwinContext context)
		{
			return null;
		}

		public virtual void OnResponse(IOwinContext context, string markup, TimeSpan invokeDuration, TimeSpan renderDuration)
		{
		}

        public virtual void Dispose()
        {
            if (_TemplateManager != null)
            {
                _TemplateManager.Dispose();
                _TemplateManager = null;
            }
        }

        public string Render(string path, Type modelType, object model)
        {
            return _TemplateManager.Render(path, modelType, model);
        }

        protected void Initialize(string templatePath)
        {
            _TemplateManager = new TemplateManager(templatePath);
            _TemplateManager.LoadTemplates();
        }
    }
}
