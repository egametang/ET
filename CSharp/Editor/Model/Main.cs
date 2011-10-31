using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Egametang
{
	public class Main
	{
		private string loginResult = "";

		public Main()
		{
		}
		public string LoginResult
		{
			get
			{
				return loginResult;
			}
			set
			{
				loginResult = value;
			}
		}
	}
}
