using System.IO;

namespace BeRated.Server
{
	class TemplateState
	{
		public FileInfo FileInfo { get; set; }

		public bool Compiled { get; set; }

		public TemplateState(FileInfo fileInfo)
		{
			FileInfo = fileInfo;
			Compiled = false;
		}
	}
}
