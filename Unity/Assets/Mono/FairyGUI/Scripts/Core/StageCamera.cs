using System;
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

        /// <summary>
        /// 
        /// </summary>
        [NonSerialized]
        public float unitsPerPixel = 0.02f;

        [NonSerialized]
        public Transform cachedTransform;
        [NonSerialized]
        public Camera cachedCamera;

        [NonSerialized]
        int screenWidth;
        [NonSerialized]
        int screenHeight;
        [NonSerialized]
        bool isMain;
        [NonSerialized]
        Display _display;

        /// <summary>
        /// 
        /// </summary>
        [NonSerialized]
        public static Camera main;

        /// <summary>
        /// 
        /// </summary>
        [NonSerialized]
        public static int screenSizeVer = 1;

        public const string Name = "Stage Camera";
        public const string LayerName = "UI";

        public static float DefaultCameraSize = 5;
        public static float DefaultUnitsPerPixel = 0.02f;

        void OnEnable()
        {
            cachedTransform = this.transform;
            cachedCamera = this.GetComponent<Camera>();
            if (this.gameObject.name == Name)
            {
                main = cachedCamera;
                isMain = true;
            }

            if (Display.displays.Length > 1 && cachedCamera.targetDisplay != 0 && cachedCamera.targetDisplay < Display.displays.Length)
                _display = Display.displays[cachedCamera.targetDisplay];

            if (_display == null)
                OnScreenSizeChanged(Screen.width, Screen.height);
            else
                OnScreenSizeChanged(_display.renderingWidth, _display.renderingHeight);
        }

        void Update()
        {
            if (_display == null)
            {
                if (screenWidth != Screen.width || screenHeight != Screen.height)
                    OnScreenSizeChanged(Screen.width, Screen.height);
            }
            else
            {
                if (screenWidth != _display.renderingWidth || screenHeight != _display.renderingHeight)
                    OnScreenSizeChanged(_display.renderingWidth, _display.renderingHeight);
            }
        }

        void OnScreenSizeChanged(int newWidth, int newHeight)
        {
            if (newWidth == 0 || newHeight == 0)
                return;

            screenWidth = newWidth;
            screenHeight = newHeight;

            if (constantSize)
            {
                cachedCamera.orthographicSize = DefaultCameraSize;
                unitsPerPixel = cachedCamera.orthographicSize * 2 / screenHeight;
            }
            else
            {
                unitsPerPixel = DefaultUnitsPerPixel;
                cachedCamera.orthographicSize = screenHeight / 2 * unitsPerPixel;
            }
            cachedTransform.localPosition = new Vector3(cachedCamera.orthographicSize * screenWidth / screenHeight, -cachedCamera.orthographicSize);

            if (isMain)
            {
                screenSizeVer++;
                if (Application.isPlaying)
                    Stage.inst.HandleScreenSizeChanged(screenWidth, screenHeight, unitsPerPixel);
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
