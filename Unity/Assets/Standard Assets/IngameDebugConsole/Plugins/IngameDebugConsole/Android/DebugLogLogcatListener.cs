#if !UNITY_EDITOR && UNITY_ANDROID
using System.Collections.Generic;
using UnityEngine;

// Credit: https://stackoverflow.com/a/41018028/2373034
namespace IngameDebugConsole
{
	public class DebugLogLogcatListener : AndroidJavaProxy
	{
		private Queue<string> queuedLogs;
		private AndroidJavaObject nativeObject;

		public DebugLogLogcatListener() : base( "com.yasirkula.unity.DebugConsoleLogcatLogReceiver" )
		{
			queuedLogs = new Queue<string>( 16 );
		}

		~DebugLogLogcatListener()
		{
			Stop();

			if( nativeObject != null )
				nativeObject.Dispose();
		}

		public void Start( string arguments )
		{
			if( nativeObject == null )
				nativeObject = new AndroidJavaObject( "com.yasirkula.unity.DebugConsoleLogcatLogger" );

			nativeObject.Call( "Start", this, arguments );
		}

		public void Stop()
		{
			if( nativeObject != null )
				nativeObject.Call( "Stop" );
		}

		public void OnLogReceived( string log )
		{
			queuedLogs.Enqueue( log );
		}

		public string GetLog()
		{
			if( queuedLogs.Count > 0 )
				return queuedLogs.Dequeue();

			return null;
		}
	}
}
#endif