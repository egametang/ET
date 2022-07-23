using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// UpdateContext is for internal use.
    /// </summary>
    public class UpdateContext
    {
        public struct ClipInfo
        {
            public Rect rect;
            public Vector4 clipBox;
            public bool soft;
            public Vector4 softness;//left-top-right-bottom
            public uint clipId;
            public int rectMaskDepth;
            public int referenceValue;
            public bool reversed;
        }

        Stack<ClipInfo> _clipStack;

        public bool clipped;
        public ClipInfo clipInfo;

        public int renderingOrder;
        public int batchingDepth;
        public int rectMaskDepth;
        public int stencilReferenceValue;
        public int stencilCompareValue;

        public float alpha;
        public bool grayed;

        public static UpdateContext current;
        public static bool working;

        public static event Action OnBegin;
        public static event Action OnEnd;

        static Action _tmpBegin;

        public UpdateContext()
        {
            _clipStack = new Stack<ClipInfo>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Begin()
        {
            current = this;

            renderingOrder = 0;
            batchingDepth = 0;
            rectMaskDepth = 0;
            stencilReferenceValue = 0;
            alpha = 1;
            grayed = false;

            clipped = false;
            _clipStack.Clear();

            Stats.ObjectCount = 0;
            Stats.GraphicsCount = 0;

            _tmpBegin = OnBegin;
            OnBegin = null;

            //允许OnBegin里再次Add，这里没有做死锁检查
            while (_tmpBegin != null)
            {
                _tmpBegin.Invoke();
                _tmpBegin = OnBegin;
                OnBegin = null;
            }

            working = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void End()
        {
            working = false;

            if (OnEnd != null)
                OnEnd.Invoke();

            OnEnd = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clipId"></param>
        /// <param name="clipRect"></param>
        /// <param name="softness"></param>
        public void EnterClipping(uint clipId, Rect clipRect, Vector4? softness)
        {
            _clipStack.Push(clipInfo);

            if (rectMaskDepth > 0)
                clipRect = ToolSet.Intersection(ref clipInfo.rect, ref clipRect);

            clipped = true;
            clipInfo.rectMaskDepth = ++rectMaskDepth;
            /* clipPos = xy * clipBox.zw + clipBox.xy
                * 利用这个公式，使clipPos变为当前顶点距离剪切区域中心的距离值，剪切区域的大小为2x2
                * 那么abs(clipPos)>1的都是在剪切区域外
                */
            clipInfo.rect = clipRect;
            clipRect.x = clipRect.x + clipRect.width * 0.5f;
            clipRect.y = clipRect.y + clipRect.height * 0.5f;
            clipRect.width *= 0.5f;
            clipRect.height *= 0.5f;
            if (clipRect.width == 0 || clipRect.height == 0)
                clipInfo.clipBox = new Vector4(-2, -2, 0, 0);
            else
                clipInfo.clipBox = new Vector4(-clipRect.x / clipRect.width, -clipRect.y / clipRect.height,
                    1.0f / clipRect.width, 1.0f / clipRect.height);
            clipInfo.clipId = clipId;
            clipInfo.soft = softness != null;
            if (clipInfo.soft)
            {
                clipInfo.softness = (Vector4)softness;
                float vx = clipInfo.rect.width * Screen.height * 0.25f;
                float vy = clipInfo.rect.height * Screen.height * 0.25f;

                if (clipInfo.softness.x > 0)
                    clipInfo.softness.x = vx / clipInfo.softness.x;
                else
                    clipInfo.softness.x = 10000f;

                if (clipInfo.softness.y > 0)
                    clipInfo.softness.y = vy / clipInfo.softness.y;
                else
                    clipInfo.softness.y = 10000f;

                if (clipInfo.softness.z > 0)
                    clipInfo.softness.z = vx / clipInfo.softness.z;
                else
                    clipInfo.softness.z = 10000f;

                if (clipInfo.softness.w > 0)
                    clipInfo.softness.w = vy / clipInfo.softness.w;
                else
                    clipInfo.softness.w = 10000f;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clipId"></param>
        /// <param name="reversedMask"></param>
        public void EnterClipping(uint clipId, bool reversedMask)
        {
            _clipStack.Push(clipInfo);

            if (stencilReferenceValue == 0)
                stencilReferenceValue = 1;
            else
                stencilReferenceValue = stencilReferenceValue << 1;

            if (reversedMask)
            {
                if (clipInfo.reversed)
                    stencilCompareValue = (stencilReferenceValue >> 1) - 1;
                else
                    stencilCompareValue = stencilReferenceValue - 1;
            }
            else
                stencilCompareValue = (stencilReferenceValue << 1) - 1;

            clipInfo.clipId = clipId;
            clipInfo.referenceValue = stencilReferenceValue;
            clipInfo.reversed = reversedMask;
            clipped = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void LeaveClipping()
        {
            clipInfo = _clipStack.Pop();
            stencilReferenceValue = clipInfo.referenceValue;
            rectMaskDepth = clipInfo.rectMaskDepth;
            clipped = stencilReferenceValue != 0 || rectMaskDepth != 0;
        }

        public void EnterPaintingMode()
        {
            //Reset clipping
            _clipStack.Push(clipInfo);

            clipInfo.rectMaskDepth = 0;
            clipInfo.referenceValue = 0;
            clipInfo.reversed = false;
            clipped = false;
        }

        public void LeavePaintingMode()
        {
            clipInfo = _clipStack.Pop();
            stencilReferenceValue = clipInfo.referenceValue;
            rectMaskDepth = clipInfo.rectMaskDepth;
            clipped = stencilReferenceValue != 0 || rectMaskDepth != 0;
        }

        public void ApplyClippingProperties(Material mat, bool isStdMaterial)
        {
            if (rectMaskDepth > 0) //在矩形剪裁下，且不是遮罩对象
            {
                mat.SetVector(ShaderConfig.ID_ClipBox, clipInfo.clipBox);
                if (clipInfo.soft)
                    mat.SetVector(ShaderConfig.ID_ClipSoftness, clipInfo.softness);
            }

            if (stencilReferenceValue > 0)
            {
                mat.SetInt(ShaderConfig.ID_StencilComp, (int)UnityEngine.Rendering.CompareFunction.Equal);
                mat.SetInt(ShaderConfig.ID_Stencil, stencilCompareValue);
                mat.SetInt(ShaderConfig.ID_Stencil2, stencilCompareValue);
                mat.SetInt(ShaderConfig.ID_StencilOp, (int)UnityEngine.Rendering.StencilOp.Keep);
                mat.SetInt(ShaderConfig.ID_StencilReadMask, stencilReferenceValue | (stencilReferenceValue - 1));
                mat.SetInt(ShaderConfig.ID_ColorMask, 15);
            }
            else
            {
                mat.SetInt(ShaderConfig.ID_StencilComp, (int)UnityEngine.Rendering.CompareFunction.Always);
                mat.SetInt(ShaderConfig.ID_Stencil, 0);
                mat.SetInt(ShaderConfig.ID_Stencil2, 0);
                mat.SetInt(ShaderConfig.ID_StencilOp, (int)UnityEngine.Rendering.StencilOp.Keep);
                mat.SetInt(ShaderConfig.ID_StencilReadMask, 255);
                mat.SetInt(ShaderConfig.ID_ColorMask, 15);
            }

            if (!isStdMaterial)
            {
                if (rectMaskDepth > 0)
                {
                    if (clipInfo.soft)
                        mat.EnableKeyword("SOFT_CLIPPED");
                    else
                        mat.EnableKeyword("CLIPPED");
                }
                else
                {
                    mat.DisableKeyword("CLIPPED");
                    mat.DisableKeyword("SOFT_CLIPPED");
                }
            }
        }

        public void ApplyAlphaMaskProperties(Material mat, bool erasing)
        {
            if (!erasing)
            {
                if (stencilReferenceValue == 1)
                {
                    mat.SetInt(ShaderConfig.ID_StencilComp, (int)UnityEngine.Rendering.CompareFunction.Always);
                    mat.SetInt(ShaderConfig.ID_Stencil, 1);
                    mat.SetInt(ShaderConfig.ID_StencilOp, (int)UnityEngine.Rendering.StencilOp.Replace);
                    mat.SetInt(ShaderConfig.ID_StencilReadMask, 255);
                    mat.SetInt(ShaderConfig.ID_ColorMask, 0);
                }
                else
                {
                    if (stencilReferenceValue != 0 & _clipStack.Peek().reversed)
                        mat.SetInt(ShaderConfig.ID_StencilComp, (int)UnityEngine.Rendering.CompareFunction.NotEqual);
                    else
                        mat.SetInt(ShaderConfig.ID_StencilComp, (int)UnityEngine.Rendering.CompareFunction.Equal);
                    mat.SetInt(ShaderConfig.ID_Stencil, stencilReferenceValue | (stencilReferenceValue - 1));
                    mat.SetInt(ShaderConfig.ID_StencilOp, (int)UnityEngine.Rendering.StencilOp.Replace);
                    mat.SetInt(ShaderConfig.ID_StencilReadMask, stencilReferenceValue - 1);
                    mat.SetInt(ShaderConfig.ID_ColorMask, 0);
                }
            }
            else
            {
                if (stencilReferenceValue != 0 & _clipStack.Peek().reversed)
                {
                    int refValue = stencilReferenceValue | (stencilReferenceValue - 1);
                    mat.SetInt(ShaderConfig.ID_StencilComp, (int)UnityEngine.Rendering.CompareFunction.Equal);
                    mat.SetInt(ShaderConfig.ID_Stencil, refValue);
                    mat.SetInt(ShaderConfig.ID_StencilOp, (int)UnityEngine.Rendering.StencilOp.Zero);
                    mat.SetInt(ShaderConfig.ID_StencilReadMask, refValue);
                    mat.SetInt(ShaderConfig.ID_ColorMask, 0);
                }
                else
                {
                    int refValue = stencilReferenceValue - 1;
                    mat.SetInt(ShaderConfig.ID_StencilComp, (int)UnityEngine.Rendering.CompareFunction.Equal);
                    mat.SetInt(ShaderConfig.ID_Stencil, refValue);
                    mat.SetInt(ShaderConfig.ID_StencilOp, (int)UnityEngine.Rendering.StencilOp.Replace);
                    mat.SetInt(ShaderConfig.ID_StencilReadMask, refValue);
                    mat.SetInt(ShaderConfig.ID_ColorMask, 0);
                }
            }
        }
    }
}
