using System;

namespace BeRated.Server
{
    public abstract class BaseApp : IDisposable
    {
        private TemplateManager _TemplateManager = null;

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
