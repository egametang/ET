using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class CaptureCamera : MonoBehaviour
	{
		/// <summary>
		/// 
		/// </summary>
		[System.NonSerialized]
		public Transform cachedTransform;
		/// <summary>
		/// 
		/// </summary>
		[System.NonSerialized]
		public Camera cachedCamera;

		[System.NonSerialized]
		static CaptureCamera _main;

		[System.NonSerialized]
		static int _layer = -1;
		static int _hiddenLayer = -1;

		public const string Name = "Capture Camera";
		public const string LayerName = "VUI";
		public const string HiddenLayerName = "Hidden VUI";

		void OnEnable()
		{
			cachedCamera = this.GetComponent<Camera>();
			cachedTransform = this.gameObject.transform;

			if (this.gameObject.name == Name)
				_main = this;
		}

		/// <summary>
		/// 
		/// </summary>
		public static void CheckMain()
		{
			if (_main != null && _main.cachedCamera != null)
				return;

			GameObject go = GameObject.Find(Name);
			if (go != null)
			{
				_main = go.GetComponent<CaptureCamera>();
				return;
			}

			GameObject cameraObject = new GameObject(Name);
			Camera camera = cameraObject.AddComponent<Camera>();
			camera.depth = 0;
			camera.cullingMask = 1 << layer;
			camera.clearFlags = CameraClearFlags.Depth;
			camera.orthographic = true;
			camera.orthographicSize = 5;
			camera.nearClipPlane = -30;
			camera.farClipPlane = 30;
			camera.enabled = false;
#if UNITY_5_4_OR_NEWER
			camera.stereoTargetEye = StereoTargetEyeMask.None;
#endif

#if UNITY_5_6_OR_NEWER
			camera.allowHDR = false;
			camera.allowMSAA = false;
#endif
			cameraObject.AddComponent<CaptureCamera>();
		}

		/// <summary>
		/// 
		/// </summary>
		public static int layer
		{
			get
			{
				if (_layer == -1)
				{
					_layer = LayerMask.NameToLayer(LayerName);
					if (_layer == -1)
					{
						_layer = 30;
						Debug.LogWarning("Please define two layers named '" + CaptureCamera.LayerName + "' and '" + CaptureCamera.HiddenLayerName + "'");
					}
				}

				return _layer;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static int hiddenLayer
		{
			get
			{
				if (_hiddenLayer == -1)
				{
					_hiddenLayer = LayerMask.NameToLayer(HiddenLayerName);
					if (_hiddenLayer == -1)
					{
						Debug.LogWarning("Please define two layers named '" + CaptureCamera.LayerName + "' and '" + CaptureCamera.HiddenLayerName + "'");
						_hiddenLayer = 31;
					}
				}

				return _hiddenLayer;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="stencilSupport"></param>
		/// <returns></returns>
		public static RenderTexture CreateRenderTexture(int width, int height, bool stencilSupport)
		{
			RenderTexture texture = new RenderTexture(width, height, stencilSupport ? 24 : 0, RenderTextureFormat.ARGB32);
			texture.antiAliasing = 1;
			texture.filterMode = FilterMode.Bilinear;
			texture.anisoLevel = 0;
			texture.useMipMap = false;
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.hideFlags = DisplayOptions.hideFlags;
			return texture;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="texture"></param>
		/// <param name="offset"></param>
		public static void Capture(DisplayObject target, RenderTexture texture, Vector2 offset)
		{
			CheckMain();

			Matrix4x4 matrix = target.cachedTransform.localToWorldMatrix;
			float unitsPerPixel = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;

			Vector3 forward;
			forward.x = matrix.m02;
			forward.y = matrix.m12;
			forward.z = matrix.m22;

			Vector3 upwards;
			upwards.x = matrix.m01;
			upwards.y = matrix.m11;
			upwards.z = matrix.m21;

			float halfHeight = (float)texture.height / 2;

			Camera camera = _main.cachedCamera;
			camera.targetTexture = texture;
			camera.orthographicSize = halfHeight * unitsPerPixel;
			_main.cachedTransform.localPosition = target.cachedTransform.TransformPoint(halfHeight * camera.aspect - offset.x, -halfHeight + offset.y, 0);
			_main.cachedTransform.localRotation = Quaternion.LookRotation(forward, upwards);

			int oldLayer = 0;

			if (target.graphics != null)
			{
				oldLayer = target.graphics.gameObject.layer;
				target.graphics.gameObject.layer = CaptureCamera.layer;
			}

			if (target is Container)
			{
				oldLayer = ((Container)target).numChildren > 0 ? ((Container)target).GetChildAt(0).layer : CaptureCamera.hiddenLayer;
				((Container)target).SetChildrenLayer(CaptureCamera.layer);
			}

			RenderTexture old = RenderTexture.active;
			RenderTexture.active = texture;
			GL.Clear(true, true, Color.clear);
			camera.Render();
			RenderTexture.active = old;

			if (target.graphics != null)
				target.graphics.gameObject.layer = oldLayer;

			if (target is Container)
				((Container)target).SetChildrenLayer(oldLayer);
		}
	}
}
