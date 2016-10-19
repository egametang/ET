using System;
using System.Collections.Generic;
using Base;
using Model;

namespace Model
{
	public class CommandLine: ICloneable
	{
		public string IP = "";
		public Options Options = new Options();

		public object Clone()
		{
			return MongoHelper.FromBson<CommandLine>(MongoHelper.ToBson(this));
		}
	}

	public class CommandLines
	{
		public List<CommandLine> Commands = new List<CommandLine>();
	}
}
