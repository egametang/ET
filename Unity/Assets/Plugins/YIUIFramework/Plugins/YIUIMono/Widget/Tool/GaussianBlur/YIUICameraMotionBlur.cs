using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 运动模糊特效
    /// </summary>
    [RequireComponent(typeof(Camera))] // 需要相机组件
    public class YIUICameraMotionBlur : MonoBehaviour
    {
        [Range(0.0f, 0.9f)]
        public float blurAmount = 0.5f; // 模糊值, 值越大拖尾效果越明显

        private RenderTexture historyTexture;  // 历史屏幕纹理
        private Material      material = null; // 材质

        private void Start()
        {
            material           = new Material(Shader.Find("YIUIShader/YIUICameraMotionBlur"));
            material.hideFlags = HideFlags.DontSave;
        }

        void OnDisable()
        {
            // 脚本不运行时立即销毁, 下次开始应用运动模糊时, 重新混合图像
            DestroyImmediate(historyTexture);
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (material != null)
            {
                // 初始化时或窗口尺寸变化时, 创建叠加纹理
                if (historyTexture == null || historyTexture.width != src.width || historyTexture.height != src.height)
                {
                    DestroyImmediate(historyTexture);
                    historyTexture           = new RenderTexture(src.width, src.height, 0);
                    historyTexture.hideFlags = HideFlags.HideAndDontSave;
                    Graphics.Blit(src, historyTexture);
                }

                material.SetFloat("_BlurAmount", 1.0f - blurAmount); // 设置模糊值, 通过alpha通道控制当前屏幕纹理与历史屏幕纹理进行混合
                Graphics.Blit(src, historyTexture, material);
                Graphics.Blit(historyTexture, dest);
            }
            else
            {
                Graphics.Blit(src, dest);
            }
        }
    }
}