using System;

namespace Model
{
	public class HttpHandlerAttribute : Attribute
	{
		public AppType AppType { get; }

		public string Path { get; }

		public HttpHandlerAttribute(AppType appType, string path)
		{
			this.AppType = appType;
			this.Path = path;
		}
	}
}