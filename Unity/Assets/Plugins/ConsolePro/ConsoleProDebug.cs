using UnityEngine;
using System;
#if UNITY_EDITOR
using System.Reflection;
#endif

public static class ConsoleProDebug
{
	// Clear the console and the native console
	public static void Clear()
	{
		#if UNITY_EDITOR
		if(ConsoleClearMethod != null)
		{
			ConsoleClearMethod.Invoke(null, null);
		}
		#endif
	}

	// Send a log to a specific filter regardless of contents
	// Ex: ConsoleProDebug.LogToFilter("Hi", "CustomFilter");
	public static void LogToFilter(string inLog, string inFilterName, UnityEngine.Object inContext = null)
	{
		Debug.Log(inLog + "\nCPAPI:{\"cmd\":\"Filter\", \"name\":\"" + inFilterName + "\"}", inContext);
	}

	// Send a log as a regular log but change its type in ConsolePro
	// Ex: ConsoleProDebug.LogAsType("Hi", "Error");
	public static void LogAsType(string inLog, string inTypeName, UnityEngine.Object inContext = null)
	{
		Debug.Log(inLog + "\nCPAPI:{\"cmd\":\"LogType\", \"name\":\"" + inTypeName + "\"}", inContext);
	}

	// Watch a variable. This will only produce one log entry regardless of how many times it is logged, allowing you to track variables without spam.
	// Ex:
	// void Update() {
	// ConsoleProDebug.Watch("Player X Position", transform.position.x);
	// }
	public static void Watch(string inName, string inValue)
	{
		Debug.Log(inName + " : " + inValue + "\nCPAPI:{\"cmd\":\"Watch\", \"name\":\"" + inName + "\"}");
	}

	public static void Search(string inText)
	{
		Debug.Log("\nCPAPI:{\"cmd\":\"Search\", \"text\":\"" + inText + "\"}");
	}

	#if UNITY_EDITOR
	// Reflection calls to access Console Pro from runtime
	private static bool _checkedConsoleClearMethod = false;
	private static MethodInfo _consoleClearMethod = null;
	private static MethodInfo ConsoleClearMethod
	{
		get
		{
			if(_consoleClearMethod == null || !_checkedConsoleClearMethod)
			{
				_checkedConsoleClearMethod = true;
				if(ConsoleWindowType == null)
				{
					return null;
				}

				_consoleClearMethod = ConsoleWindowType.GetMethod("ClearEntries", BindingFlags.Static | BindingFlags.Public);
			}

			return _consoleClearMethod;
		}
	}

	private static bool _checkedConsoleWindowType = false;
	private static Type _consoleWindowType = null;
	private static Type ConsoleWindowType
	{
		get
		{
			if(_consoleWindowType == null || !_checkedConsoleWindowType)
			{
				_checkedConsoleWindowType = true;
				Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
				for(int iAssembly = 0; iAssembly < assemblies.Length; iAssembly++)
				{
					Type[] types = assemblies[iAssembly].GetTypes();
					for(int iType = 0; iType < types.Length; iType++)
					{
						if(types[iType].Name == "ConsolePro3Window")
						{
							_consoleWindowType = types[iType];
						}
					}
				}
			}

			return _consoleWindowType;
		}
	}
	#endif
}