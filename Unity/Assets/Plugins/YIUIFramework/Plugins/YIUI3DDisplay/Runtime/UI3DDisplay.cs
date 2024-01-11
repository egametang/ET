//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using ET.Client;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YIUIFramework
{
    /// <summary>
    /// 这个类用于在UI中显示3D对象。
    /// </summary>
    public sealed partial class UI3DDisplay: SerializedMonoBehaviour,
            IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [OdinSerialize]
        [ShowInInspector]
        [LabelText("[动态] 展示的对象")]
        private GameObject m_ShowObject;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("[动态] 观察的摄像机")]
        private Camera m_LookCamera = null;

        [Required]
        [OdinSerialize]
        [ShowInInspector]
        [LabelText("面板")]
        private RawImage m_ShowImage;

        [Required]
        [OdinSerialize]
        [ShowInInspector]
        [LabelText("摄像机")]
        private Camera m_ShowCamera;

        //相机的初始化位置
        private Vector3 m_ShowCameraDefPos;
        
        [Required]
        [OdinSerialize]
        [ShowInInspector]
        [LabelText("摄像机控制器")]
        private UI3DDisplayCamera m_ShowCameraCtrl;

        [Required]
        [OdinSerialize]
        [ShowInInspector]
        [LabelText("灯光")]
        private Light m_ShowLight;

        [Required]
        [OdinSerialize]
        [ShowInInspector]
        [LabelText("这个变换将自动适应比例")]
        private Transform m_FitScaleRoot;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("自动设置图像大小")]
        private bool m_AutoChangeSize = true;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("图像宽")]
        [MinValue(0)]
        private int m_ResolutionX = 512;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("图像高")]
        [MinValue(0)]
        private int m_ResolutionY = 512;

        [LabelText("深度值")] //默认16
        private readonly int m_RenderTextureDepthBuffer = 16;

        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("当前显示层级")]
        private int m_ShowLayer = 0;

        private const string YIUI3DLayer = "YIUI3DLayer";

        [ShowInInspector]
        [LabelText("当前显示层级")]
        [ReadOnly]
        private string m_ShowLayerName = YIUI3DLayer;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("允许拖拽")]
        private bool m_CanDrag = true;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("拖拽速度")]
        [ShowIf("m_CanDrag")]
        private float m_DragSpeed = 10.0f;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("显示对象的位置偏移值")]
        private Vector3 m_ShowOffset = Vector3.zero;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("显示对象的旋转偏移值")]
        private Vector3 m_ShowRotation = Vector3.zero;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("显示对象的比例")]
        private Vector3 m_ShowScale = Vector3.one;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("镜面反射面")]
        private Transform m_ReflectionPlane = null;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("阴影面")]
        private Transform m_ShadowPlane = null;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("使用观察摄像机的颜色")]
        private bool m_UseLookCameraColor = false;

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("自动同步")]
        private bool m_AutoSync = false;

        //显示的拖动旋转
        private float m_DragRotation;

        //显示渲染纹理
        private RenderTexture m_ShowTexture;

        //记录显示位置
        private Vector3 m_ShowPosition;

        //正交大小
        private float m_OrthographicSize;

        //每显示一次就会+1 用于位置偏移
        private static int g_DisPlayUIIndex = 0;

        //当前模型偏移位置
        private Vector3 m_ModelGlobalOffset = Vector3.zero;

        //所有已采集的阴影
        private List<Renderer> m_RenderList = new List<Renderer>();

        private Camera m_UICamera = null;

        private Camera UICamera
        {
            get
            {
                if (m_UICamera != null) return m_UICamera;
                m_UICamera = YIUIMgrComponent.Inst.UICamera;
                return m_UICamera;
            }
        }

        /// <summary>
        /// 设置动画选择目标
        /// </summary>
        private void SetAnimatorCullingMode(Transform target)
        {
            var animator = target.GetComponent<Animator>();
            if (animator)
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        //设置所有动画
        //总是让整个角色动画化。对象即使在屏幕外也是动画的。
        //因为我们会吧对象丢到屏幕外否则动画可能会不动
        private void SetAllAnimatorCullingMode(Transform target)
        {
            SetAnimatorCullingMode(target);
            for (var i = 0; i < target.childCount; ++i)
            {
                SetAnimatorCullingMode(target.GetChild(i));
            }
        }

        //吧指定对象的层级改为设定的层级
        private void SetupShowLayer()
        {
            if (m_ShowCameraCtrl != null && m_ShowCameraCtrl.ShowObject != null)
            {
                m_ShowCameraCtrl.SetupRenderer(m_ShowCameraCtrl.ShowObject.transform);
            }
        }

        //修改层级 //目前使用默认层级最好不要修改
        private void ChangeLayerName(string layerName)
        {
            m_ShowLayerName = layerName;
            m_ShowLayer     = LayerMask.NameToLayer(layerName);
            if (m_ShowLayer != -1) return;
            if (layerName == YIUI3DLayer)
            {
                Debug.LogError($"第一次使用请手动创建 YIUI3DLayer 层级");
                return;
            }

            Debug.LogError($"当前设定的UI层级不存在请检查 {layerName} 强制修改层级为 {YIUI3DLayer}");
            m_ShowLayerName = YIUI3DLayer;
            m_ShowLayer     = LayerMask.NameToLayer(YIUI3DLayer);
        }

        /// <summary>
        /// 设置对象的显示层级
        /// </summary>
        private void SetupShowLayerTarget(Transform target)
        {
            if (m_ShowCameraCtrl != null && m_ShowCameraCtrl.ShowObject != null)
            {
                m_ShowCameraCtrl.SetupRenderer(target);
            }
        }

        //回收回调
        private Action<GameObject> m_RecycleLastAction;

        //回收之前的对象 如果有回调就回调自行处理  否则会被无视
        private void RecycleLastShow(GameObject lastShowObject)
        {
            if (lastShowObject == null) return;
            lastShowObject.SetActive(false);
            m_RecycleLastAction?.Invoke(lastShowObject);
        }

        //更新显示的对象
        //父级 位置 旋转
        private void UpdateShowObject(GameObject showObject)
        {
            if (showObject != m_ShowObject)
                RecycleLastShow(m_ShowObject);

            showObject.SetActive(true);

            m_ShowObject = showObject;

            m_DragRotation = 0f;

            m_DragTarge = m_MultipleTargetMode? null : m_ShowObject;

            var showTransform = m_ShowObject.transform;
            if (m_FitScaleRoot != null)
            {
                m_FitScaleRoot.localScale = Vector3.one;
                showTransform.SetParent(m_FitScaleRoot, true);
            }
            else
            {
                showTransform.SetParent(transform, true);
            }

            m_ShowCameraCtrl ??= m_ShowCamera.GetOrAddComponent<UI3DDisplayCamera>();
            if (m_ShowCameraCtrl == null)
            {
                Debug.LogError($"必须有 UI3DDisplayCamera 组件 请检查");
                return;
            }

            //对象层级
            m_ShowCameraCtrl.ShowLayer  = m_ShowLayer;
            m_ShowCameraCtrl.ShowObject = m_ShowObject;

            //动画屏幕外也可动
            SetAllAnimatorCullingMode(m_ShowObject.transform);

            //位置大小旋转
            var showRotation = Quaternion.Euler(m_ShowRotation);
            var showUp       = showRotation * Vector3.up;
            showRotation                *= Quaternion.AngleAxis(m_DragRotation, showUp);
            showTransform.localRotation =  showRotation;
            showTransform.localScale    =  m_ShowScale;
            showTransform.localPosition =  m_ModelGlobalOffset + m_ShowOffset;
            m_ShowPosition              =  showTransform.localPosition;

            //镜面反射
            if (m_ReflectionPlane != null)
            {
                m_ReflectionPlane.position = showTransform.position;
                m_ReflectionPlane.rotation = showTransform.rotation;
            }

            //阴影
            DisableMeshRectShadow();
            if (m_ShadowPlane != null)
            {
                EnableMeshRectShadow(m_ShowObject.transform);
                m_ShadowPlane.position = showTransform.position;
                m_ShadowPlane.rotation = showTransform.rotation;
            }

            if (m_FitScaleRoot != null)
            {
                var lossyScale = m_FitScaleRoot.lossyScale;
                var localScale = transform.localScale;
                m_FitScaleRoot.localScale = new Vector3(1f / lossyScale.x * localScale.x,
                    1f / lossyScale.y * localScale.y,
                    1f / lossyScale.z * localScale.z);
            }
        }

        //更新摄像机配置根据传入的摄像机
        private void UpdateLookCamera(Camera lookCamera)
        {
            Assert.IsNotNull(m_ShowCamera);

            lookCamera.gameObject.SetActive(false);
            m_ShowCamera.orthographic     = lookCamera.orthographic;
            m_ShowCamera.orthographicSize = lookCamera.orthographicSize;
            m_ShowCamera.fieldOfView      = lookCamera.fieldOfView;
            m_ShowCamera.nearClipPlane    = Mathf.Max(lookCamera.nearClipPlane, 1);
            m_ShowCamera.farClipPlane     = Mathf.Min(lookCamera.farClipPlane, 100);
            m_ShowCamera.orthographic     = lookCamera.orthographic;
            m_ShowCamera.orthographicSize = lookCamera.orthographicSize;
            m_ShowCamera.clearFlags       = CameraClearFlags.SolidColor;
            m_ShowCamera.backgroundColor  = m_UseLookCameraColor? lookCamera.backgroundColor : Color.clear;
            m_OrthographicSize            = lookCamera.orthographicSize;
            m_LookCamera                  = lookCamera;

            m_ShowCamera.cullingMask = 1 << m_ShowLayer;

            if (m_ShowLight)
                m_ShowLight.cullingMask = m_ShowCamera.cullingMask;


            var lookCameraTsf = lookCamera.transform;
            if (lookCamera == m_ShowCamera)
            {
                //当使用默认相机作为显示相机时 需要处理每个显示物体的额外偏移
                lookCamera.transform.localPosition = m_ShowCameraDefPos + m_ModelGlobalOffset; 
            }

            m_ShowCamera.transform.SetPositionAndRotation(lookCameraTsf.position,
                lookCameraTsf.rotation);

            m_ShowCamera.enabled = true;
            m_ShowCamera.gameObject.SetActive(true);
        }

        //RawImage 用的RenderTexture就是这样动态创建的
        private void SetTemporaryRenderTexture()
        {
            Assert.IsNotNull(m_ShowImage);

            if (m_ShowTexture != null)
                RenderTexture.ReleaseTemporary(m_ShowTexture);

            m_ShowTexture              = RenderTexture.GetTemporary(m_ResolutionX, m_ResolutionY, m_RenderTextureDepthBuffer);
            m_ShowImage.texture        = m_ShowTexture;
            m_ShowCamera.targetTexture = m_ShowTexture;
            m_ShowImage.enabled        = true;
        }

        //阴影采集
        private void EnableMeshRectShadow(Transform goNode)
        {
            var render = goNode.GetComponent<SkinnedMeshRenderer>();
            if (render != null)
            {
                if (render.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off)
                {
                    render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    m_RenderList.Add(render);
                }
            }

            for (var i = 0; i < goNode.childCount; ++i)
            {
                EnableMeshRectShadow(goNode.GetChild(i));
            }
        }

        //关闭所有已采集的阴影
        private void DisableMeshRectShadow()
        {
            foreach (var render in m_RenderList)
            {
                render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            m_RenderList.Clear();
        }

        //交换显示的rawImage  一般不需要
        private void ExchangeShowImage(RawImage img)
        {
            m_ShowImage = img;
            if (null != m_ShowTexture)
            {
                m_ShowImage.texture = m_ShowTexture;
            }
            else
            {
                m_ShowTexture              = RenderTexture.GetTemporary(m_ResolutionX, m_ResolutionY, m_RenderTextureDepthBuffer);
                m_ShowImage.texture        = m_ShowTexture;
                m_ShowCamera.targetTexture = m_ShowTexture;
            }
        }
    }
}