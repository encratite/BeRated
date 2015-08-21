using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Ashod;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace BeRated.Server
{
    class TemplateManager : IDisposable
    {
        private const string PathSeparator = "/";

        private string _Path;

        private IRazorEngineService _Engine;

        private Dictionary<string, TemplateState> _TemplateStates = new Dictionary<string, TemplateState>();

        private DelegateTemplateManager _TemplateManager = new DelegateTemplateManager();
        private InvalidatingCachingProvider _CachingProvider = new InvalidatingCachingProvider();

        public TemplateManager(string path)
        {
            _Path = path;
            var configuration = new TemplateServiceConfiguration();
            configuration.TemplateManager = _TemplateManager;
            configuration.CachingProvider = _CachingProvider;
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
            Logger.Log("Compiled templates in {0} ms", stopwatch.ElapsedMilliseconds);
        }

        public string Render(string key, Type modelType, object model)
        {
			TemplateState templateState;
            if (!_TemplateStates.TryGetValue(key, out templateState))
                throw new ApplicationException("Unable to find template in cache");
			var cacheFileInfo = templateState.FileInfo;
            string templatePath = cacheFileInfo.FullName;
            var fileInfo = new FileInfo(templatePath);
            if (!templateState.Compiled || fileInfo.LastWriteTimeUtc > cacheFileInfo.LastWriteTimeUtc)
            {
                try
                {
                    Console.WriteLine("Recompiling {0}", templatePath);
                    string source = File.ReadAllText(templatePath);
                    var templateKey = _Engine.GetKey(key);
                    _TemplateManager.RemoveDynamic(templateKey);
                    _CachingProvider.InvalidateCache(templateKey);
                    _Engine.Compile(source, key);
                    templateState.FileInfo = fileInfo;
					templateState.Compiled = true;
                }
                catch (TemplateCompilationException exception)
                {
					templateState.Compiled = false;
					PrintCompilationError(templatePath, exception);
                    throw new ApplicationException("Failed to serve request due to compilation error");
                }
            }
            string markup = _Engine.Run(key, model.GetType(), model);
            return markup;
        }

        private void LoadTemplates(string virtualPath)
        {
            string physicalPath = ConvertPath(virtualPath);
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
				var templateState = new TemplateState(file);
				_TemplateStates[key] = templateState;
				try
                {
                    string source = File.ReadAllText(file.FullName);
                    Logger.Log("Compiling {0} for virtual path {1}", file.FullName, key);
                    _Engine.Compile(source, key);
					templateState.Compiled = true;
                }
                catch (TemplateCompilationException exception)
                {
                    PrintCompilationError(file.FullName, exception);
                }
            }
        }

        private static void PrintCompilationError(string path, TemplateCompilationException exception)
        {
            Logger.Error("Failed to compile {0}:", path);
            foreach (var error in exception.CompilerErrors)
                Logger.Error("Line {0}: {1}", error.Line, error.ErrorText);
        }

        private string ConvertPath(string virtualPath)
        {
            string relativePath = virtualPath.Replace(PathSeparator, Path.DirectorySeparatorChar.ToString());
            string physicalPath = Path.Combine(_Path, relativePath);
            return physicalPath;
        }

        private string Combine(string left, string right)
        {
            return left + PathSeparator + right;
        }
    }
}
