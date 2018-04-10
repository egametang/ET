using System.Collections.Generic;

namespace ETEditor
{
	public class RsyncConfig
	{
		public string Host = "";
		public string Account = "";
		public string Password = "";
		public string RelativePath = "";
		public List<string> Exclude = new List<string>();
	}
}