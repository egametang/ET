using UnityEngine;
using Base;

public class LSharpLogger : CLRSharp.ICLRSharp_Logger//实现L#的LOG接口
{
	public void Log(string str)
	{
		Base.Log.Debug(str);
	}

	public void Log_Error(string str)
	{
		Base.Log.Error(str);
	}

	public void Log_Warning(string str)
	{
		Base.Log.Warning(str);
	}
}

