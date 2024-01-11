using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YIUIFramework;

namespace ET.Client
{
    public partial class YIUIMgrComponent
    {
        public static bool          IsLowQuality = false;           //低品质 将会影响动画等逻辑 也可以根据这个参数自定义一些区别
        public        bool          BindInit { get; internal set; } //绑定初始化情况
        public        GameObject    UIRoot;
        public        GameObject    UICanvasRoot;
        public        RectTransform UILayerRoot;
        public        Camera        UICamera;
        public        Canvas        UICanvas;
        public const  int           DesignScreenWidth    = 1920;
        public const  int           DesignScreenHeight   = 1080;
        public const  float         DesignScreenWidth_F  = 1920f;
        public const  float         DesignScreenHeight_F = 1080f;

        public const int RootPosOffset = 1000;
        public const int LayerDistance = 1000;

        #region 以下名称 禁止修改

        public const string UIRootName      = "YIUIRoot";
        public const string UILayerRootName = "YIUILayerRoot";
        public const string UIRootPkgName   = "Common";

        #endregion

        //K1 = 层级枚举 V1 = 层级对应的rect
        //List = 当前层级中的当前所有UI 前面的代表这个UI在前面以此类推
        public Dictionary<EPanelLayer, Dictionary<RectTransform, List<PanelInfo>>> m_AllPanelLayer =
                new Dictionary<EPanelLayer, Dictionary<RectTransform, List<PanelInfo>>>();

        #region 快捷获取层级

        private RectTransform m_UICache;

        public RectTransform UICache
        {
            get
            {
                if (m_UICache == null)
                {
                    m_UICache = this.GetLayerRect(EPanelLayer.Cache);
                }

                return m_UICache;
            }
        }

        private RectTransform m_UIPanel;

        public RectTransform UIPanel
        {
            get
            {
                if (m_UIPanel == null)
                {
                    m_UIPanel = this.GetLayerRect(EPanelLayer.Panel);
                }

                return m_UIPanel;
            }
        }

        #endregion

        internal async ETTask<bool> InitRoot()
        {
            #region UICanvasRoot 查找各种组件

            UIRoot = GameObject.Find(YIUIMgrComponent.UIRootName);
            if (UIRoot == null)
            {
                UIRoot = await YIUILoadHelper.LoadAssetAsyncInstantiate(YIUIMgrComponent.UIRootPkgName, YIUIMgrComponent.UIRootName);
            }

            if (UIRoot == null)
            {
                Debug.LogError($"初始化错误 没有找到UIRoot");
                return false;
            }

            UIRoot.name = UIRoot.name.Replace("(Clone)", "");
            UnityEngine.Object.DontDestroyOnLoad(UIRoot);

            //root可修改位置防止与世界3D场景重叠导致不好编辑
            UIRoot.transform.position = new Vector3(YIUIMgrComponent.RootPosOffset, YIUIMgrComponent.RootPosOffset, 0);

            UICanvas = UIRoot.GetComponentInChildren<Canvas>();
            if (UICanvas == null)
            {
                Debug.LogError($"初始化错误 没有找到Canvas");
                return false;
            }

            UICanvasRoot = UICanvas.gameObject;

            UILayerRoot = UICanvasRoot.transform.FindChildByName(YIUIMgrComponent.UILayerRootName)?.GetComponent<RectTransform>();
            if (UILayerRoot == null)
            {
                Debug.LogError($"初始化错误 没有找到UILayerRoot");
                return false;
            }

            UICamera = UICanvasRoot.GetComponentInChildren<Camera>();
            if (UICamera == null)
            {
                Debug.LogError($"初始化错误 没有找到UICamera");
                return false;
            }

            var canvas = UICanvasRoot.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError($"初始化错误 没有找到UICanvasRoot - Canvas");
                return false;
            }

            canvas.renderMode  = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = UICamera;

            var canvasScaler = UICanvasRoot.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                Debug.LogError($"初始化错误 没有找到UICanvasRoot - CanvasScaler");
                return false;
            }

            canvasScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(YIUIMgrComponent.DesignScreenWidth, YIUIMgrComponent.DesignScreenHeight);

            #endregion

            //分层
            const int len = (int)EPanelLayer.Count;
            m_AllPanelLayer.Clear();
            for (var i = len - 1; i >= 0; i--)
            {
                var layer = new GameObject($"Layer{i}-{(EPanelLayer)i}");
                var rect  = layer.AddComponent<RectTransform>();
                rect.SetParent(UILayerRoot);
                rect.localScale    = Vector3.one;
                rect.pivot         = new Vector2(0.5f, 0.5f);
                rect.anchorMax     = Vector2.one;
                rect.anchorMin     = Vector2.zero;
                rect.sizeDelta     = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localPosition = new Vector3(0, 0, i * YIUIMgrComponent.LayerDistance); //这个是为了3D模型时穿插的问题
                var rectDic = new Dictionary<RectTransform, List<PanelInfo>> { { rect, new List<PanelInfo>() } };
                m_AllPanelLayer.Add((EPanelLayer)i, rectDic);
            }

            InitAddUIBlock(); //所有层级初始化后添加一个终极屏蔽层 可根据API 定时屏蔽UI操作

            UICamera.transform.localPosition =
                    new Vector3(UILayerRoot.localPosition.x, UILayerRoot.localPosition.y, -YIUIMgrComponent.LayerDistance);

            UICamera.clearFlags   = CameraClearFlags.Depth;
            UICamera.orthographic = true;
            //根据需求可以修改摄像机的远裁剪平面大小 没必要设置的很大
            //UICamera.farClipPlane = ((len + 1) * YIUIMgrComponent.LayerDistance) * UICanvasRoot.transform.localScale.x;
            
            return true;
        }

        internal bool RemoveLayerPanelInfo(EPanelLayer panelLayer, PanelInfo panelInfo)
        {
            var list = this.GetLayerPanelInfoList(panelLayer);
            return list != null && list.Remove(panelInfo);
        }

        internal List<PanelInfo> GetLayerPanelInfoList(EPanelLayer panelLayer)
        {
            m_AllPanelLayer.TryGetValue(panelLayer, out var rectDic);
            if (rectDic == null)
            {
                Debug.LogError($"没有这个层级 请检查 {panelLayer}");
                return null;
            }

            //只能有一个所以返回第一个
            foreach (var infoList in rectDic.Values)
            {
                return infoList;
            }

            return null;
        }
    }
}