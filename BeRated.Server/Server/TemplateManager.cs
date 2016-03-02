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
                catch (Exception exception)
                {
					templateState.Compiled = false;
					var errors = GetTemplateErrors(exception, templatePath);
					string message = string.Join("\n", errors);
					throw new ApplicationException(message);
                }
            }
            string markup = _Engine.Run(key, modelType, model);
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
                catch (Exception exception)
                {
                    var errors = GetTemplateErrors(exception, file.FullName);
					foreach (string error in errors)
						Logger.Error(error);
                }
            }
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

		private List<string> GetTemplateErrors(Exception exception, string path)
		{
			var output = new List<string>();
			var parsingException = exception as TemplateParsingException;
			var compilationException = exception as TemplateCompilationException;
			if (compilationException != null)
			{
				output.Add(string.Format("Failed to compile {0}:", path));
				foreach (var error in compilationException.CompilerErrors)
					output.Add(string.Format("Line {0}: {1}", error.Line, error.ErrorText));
			}
			else if (parsingException != null)
			{
				output.Add(string.Format("Failed to parse {0}:", path));
				output.Add(string.Format("Line {0}: {1}", parsingException.Line, parsingException.Message));
			}
			else
			{
				output.Add(string.Format("Unknown error in {0}:", path));
				output.Add(string.Format("{0} ({1}", exception.Message, exception.GetType()));
			}
			return output;
		}
	}
}
