using System;
using System.Collections.Generic;
using System.IO;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace BeRated.Server
{
    class TemplateManager : IDisposable
    {
        private const char PathSeparator = '/';

        private string _Path;

        private Dictionary<string, DateTime> _TemplateTimestamps = new Dictionary<string, DateTime>();

        private IRazorEngineService _Engine;

        public TemplateManager(string path)
        {
            _Path = path;
            var configuration = new TemplateServiceConfiguration();
            _Engine = RazorEngineService.Create(configuration);
        }

        void IDisposable.Dispose()
        {
            if (_Engine != null)
            {
                _Engine.Dispose();
                _Engine = null;
            }
        }

        public void LoadTemplates(string virtualPath = null)
        {
            string physicalPath = _Path;
            if (virtualPath != null)
            {
                string relativePath = virtualPath.Replace(PathSeparator, Path.PathSeparator);
                physicalPath = Path.Combine(physicalPath, relativePath);
            }
            var directory = new DirectoryInfo(physicalPath);
            foreach (var file in directory.GetFiles("*.cshtml"))
            {
                string key = file.Name;
                if (virtualPath != null)
                    key = Combine(virtualPath, key);
                string source = File.ReadAllText(file.FullName);
                _Engine.AddTemplate(key, source);
                _Engine.Compile(key);
                _TemplateTimestamps[key] = file.LastWriteTimeUtc;
            }
        }

        private string Combine(string left, string right)
        {
            return string.Format("{0}{1}{2}", left, PathSeparator, right);
        }
    }
}
