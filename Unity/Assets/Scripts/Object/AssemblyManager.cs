using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Model
{
	public class AssemblyManager
	{
		private static readonly AssemblyManager instance = new AssemblyManager();

		public static AssemblyManager Instance
		{
			get
			{
				return instance;
			}
		}

		private readonly Dictionary<string, Assembly> dictionary = new Dictionary<string, Assembly>();

		public void Add(string name, Assembly assembly)
		{
			this.dictionary[name] = assembly;
		}

		public void Remove(string name)
		{
			this.dictionary.Remove(name);
		}

		public Assembly[] GetAll()
		{
			return this.dictionary.Values.ToArray();
		}

		public Assembly Get(string name)
		{
			return this.dictionary[name];
		}
	}
}
