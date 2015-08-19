using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BeRated.Server
{
    class TemplateManager : IDisposable
    {
        private const string PathSeparator = "/";

        private string _Path;

        private Dictionary<string, DateTime> _TemplateTimestamps = new Dictionary<string, DateTime>();

        private IRazorEngineService _Engine;

        public TemplateManager(string path)
        {
            _Path = path;
            var configuration = new TemplateServiceConfiguration();
            _Engine = RazorEngineService.Create(configuration);
        }

        public void Dispose()
        {
            if (_Engine != null)
            {
                _Engine.Dispose();
                _Engine = null;
            }
        }

        public void LoadTemplates()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            LoadTemplates(string.Empty);
            stopwatch.Stop();
            Console.WriteLine("Compiled templates in {0} ms", stopwatch.ElapsedMilliseconds);
        }

        public string Render(string path, Type modelType, object model)
        {
            string markup = _Engine.Run(path, model.GetType(), model);
            return markup;
        }

        private void LoadTemplates(string virtualPath)
        {
            string relativePath = virtualPath.Replace(PathSeparator[0], Path.PathSeparator);
            string physicalPath = Path.Combine(_Path, relativePath);
            var templateDirectory = new DirectoryInfo(physicalPath);
            var directories = templateDirectory.GetDirectories();
            foreach (var directory in directories)
            {
                string directoryVirtualPath = Combine(virtualPath, directory.Name);
                LoadTemplates(directoryVirtualPath);
            }
            var files = templateDirectory.GetFiles("*.cshtml");
            foreach (var file in files)
            {
                string key = Combine(virtualPath, Path.GetFileNameWithoutExtension(file.Name));
                string source = File.ReadAllText(file.FullName);
                Console.WriteLine("Compiling {0} for virtual path {1}", file.FullName, key);
                _Engine.AddTemplate(key, source);
                _Engine.Compile(key);
                _TemplateTimestamps[key] = file.LastWriteTimeUtc;
            }
        }

        private string Combine(string left, string right)
        {
            return left + PathSeparator + right;
        }
    }
}
