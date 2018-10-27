using System;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class StageEngine : MonoBehaviour
	{
		public int ObjectsOnStage;
		public int GraphicsOnStage;

		public static bool beingQuit;

		void Start()
		{
			useGUILayout = false;
		}

		void LateUpdate()
		{
			Stage.inst.InternalUpdate();

			ObjectsOnStage = Stats.ObjectCount;
			GraphicsOnStage = Stats.GraphicsCount;
		}

		void OnGUI()
		{
			Stage.inst.HandleGUIEvents(Event.current);
		}

#if !UNITY_5_4_OR_NEWER
		void OnLevelWasLoaded()
		{
			StageCamera.CheckMainCamera();
		}
#endif

		void OnApplicationQuit()
		{
			beingQuit = true;

			if (Application.isEditor)
				UIPackage.RemoveAllPackages();
		}
	}
}