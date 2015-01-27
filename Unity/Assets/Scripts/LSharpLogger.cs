using UnityEngine;

public class LSharpLogger : CLRSharp.ICLRSharp_Logger//实现L#的LOG接口
{
	public void Log(string str)
	{
		Debug.Log(str);
	}

	public void Log_Error(string str)
	{
		Debug.LogError(str);
	}

	public void Log_Warning(string str)
	{
		Debug.LogWarning(str);
	}
}

