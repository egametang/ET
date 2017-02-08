using System;

namespace Model
{
	[Serializable]
	public class GameException: Exception
	{
		public GameException(string message): base(message)
		{
		}

		public GameException(string message, Exception e): base(message, e)
		{
		}
	}

	/// <summary>
	/// 输入异常,比如没有输入密码账号
	/// </summary>
	[Serializable]
	public class InputErrorException: Exception
	{
		public InputErrorException(string message): base(message)
		{
		}

		public InputErrorException(string message, Exception e): base(message, e)
		{
		}
	}

	/// <summary>
	/// 配置异常
	/// </summary>
	[Serializable]
	public class ConfigException: Exception
	{
		public ConfigException(string message): base(message)
		{
		}

		public ConfigException(string message, Exception e): base(message, e)
		{
		}
	}
}