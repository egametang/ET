using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// Stage Camera is an orthographic camera for UI rendering.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("FairyGUI/UI Camera")]
	public class StageCamera : MonoBehaviour
	{
		/// <summary>
		/// 
		/// </summary>
		public bool constantSize = true;

		[System.NonSerialized]
		public Transform cachedTransform;
		[System.NonSerialized]
		public Camera cachedCamera;

		[System.NonSerialized]
		int screenWidth;
		[System.NonSerialized]
		int screenHeight;
		[System.NonSerialized]
		bool isMain;

		/// <summary>
		/// 
		/// </summary>
		[System.NonSerialized]
		public static Camera main;

		/// <summary>
		/// 
		/// </summary>
		[System.NonSerialized]
		public static int screenSizeVer = 1;

		public const string Name = "Stage Camera";
		public const string LayerName = "UI";

		public static float DefaultCameraSize = 5;
		public static float UnitsPerPixel = 0.02f;

		void OnEnable()
		{
			cachedTransform = this.transform;
			cachedCamera = this.GetComponent<Camera>();
			if (this.gameObject.name == Name)
			{
				main = cachedCamera;
				isMain = true;
			}
			OnScreenSizeChanged();
		}

		void Update()
		{
			if (screenWidth != Screen.width || screenHeight != Screen.height)
				OnScreenSizeChanged();
		}

		void OnScreenSizeChanged()
		{
			screenWidth = Screen.width;
			screenHeight = Screen.height;
			if (screenWidth == 0 || screenHeight == 0)
				return;

			float upp;
			if (constantSize)
			{
				cachedCamera.orthographicSize = DefaultCameraSize;
				upp = cachedCamera.orthographicSize * 2 / screenHeight;
			}
			else
			{
				upp = 0.02f;
				cachedCamera.orthographicSize = screenHeight / 2 * UnitsPerPixel;
			}
			cachedTransform.localPosition = new Vector3(cachedCamera.orthographicSize * screenWidth / screenHeight, -cachedCamera.orthographicSize);

			if (isMain)
			{
				UnitsPerPixel = upp;
				screenSizeVer++;
				if (Application.isPlaying)
					Stage.inst.HandleScreenSizeChanged();
				else
				{
					UIContentScaler scaler = GameObject.FindObjectOfType<UIContentScaler>();
					if (scaler != null)
						scaler.ApplyChange();
					else
						UIContentScaler.scaleFactor = 1;
				}
			}
		}

		void OnRenderObject()
		{
			//Update和OnGUI在EditMode的调用都不那么及时，OnRenderObject则比较频繁，可以保证界面及时刷新。所以使用OnRenderObject
			if (isMain && !Application.isPlaying)
			{
				EMRenderSupport.Update();
			}
		}

		public void ApplyModifiedProperties()
		{
			screenWidth = 0; //force OnScreenSizeChanged called in next update
		}

		/// <summary>
		/// Check if there is a stage camera in the scene. If none, create one.
		/// </summary>
		public static void CheckMainCamera()
		{
			if (GameObject.Find(Name) == null)
			{
				int layer = LayerMask.NameToLayer(LayerName);
				CreateCamera(Name, 1 << layer);
			}

			HitTestContext.cachedMainCamera = Camera.main;
		}

		/// <summary>
		/// 
		/// </summary>
		public static void CheckCaptureCamera()
		{
			if (GameObject.Find(Name) == null)
			{
				int layer = LayerMask.NameToLayer(LayerName);
				CreateCamera(Name, 1 << layer);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="cullingMask"></param>
		/// <returns></returns>
		public static Camera CreateCamera(string name, int cullingMask)
		{
			GameObject cameraObject = new GameObject(name);
			Camera camera = cameraObject.AddComponent<Camera>();
			camera.depth = 1;
			camera.cullingMask = cullingMask;
			camera.clearFlags = CameraClearFlags.Depth;
			camera.orthographic = true;
			camera.orthographicSize = DefaultCameraSize;
			camera.nearClipPlane = -30;
			camera.farClipPlane = 30;

#if UNITY_5_4_OR_NEWER
			camera.stereoTargetEye = StereoTargetEyeMask.None;
#endif

#if UNITY_5_6_OR_NEWER
			camera.allowHDR = false;
			camera.allowMSAA = false;
#endif
			cameraObject.AddComponent<StageCamera>();

			return camera;
		}
	}
}
