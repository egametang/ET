using System;

namespace IngameDebugConsole
{
	[AttributeUsage( AttributeTargets.Method, Inherited = false, AllowMultiple = true )]
	public class ConsoleMethodAttribute : Attribute
	{
		private string m_command;
		private string m_description;

		public string Command { get { return m_command; } }
		public string Description { get { return m_description; } }

		public ConsoleMethodAttribute( string command, string description )
		{
			m_command = command;
			m_description = description;
		}
	}
}