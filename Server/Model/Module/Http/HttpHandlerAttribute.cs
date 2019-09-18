using System;

namespace ETModel
{
	public class HttpHandlerAttribute : BaseAttribute
	{
		public AppType AppType { get; }

		public string Path { get; }

		public HttpHandlerAttribute(AppType appType, string path)
		{
			this.AppType = appType;
			this.Path = path;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
	public class GetAttribute : Attribute
	{
		public string Path { get; }

		public GetAttribute()
		{
		}

		public GetAttribute(string path)
		{
			this.Path = path;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
	public class PostAttribute : Attribute
	{
		public string Path { get; }

		public PostAttribute()
		{
		}

		public PostAttribute(string path)
		{
			this.Path = path;
		}
	}
}