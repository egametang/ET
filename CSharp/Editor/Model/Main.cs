using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Egametang
{
	public class Main
	{
		private string loginResult = "";
		private string username = "";
		private string password = "";

		public Main()
		{
		}

		public string LoginInfo
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

		public string Username
		{
			get
			{
				return username;
			}
			set
			{
				username = value;
			}
		}

		public string Password
		{
			get
			{
				return password;
			}
			set
			{
				password = value;
			}
		}
	}
}
