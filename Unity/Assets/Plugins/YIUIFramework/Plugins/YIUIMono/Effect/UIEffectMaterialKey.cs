using System;
using UnityEngine;

namespace YIUIFramework
{
    internal struct UIEffectMaterialKey : IEquatable<UIEffectMaterialKey>
    {
        internal float BlurDistance;

        internal Texture2D     OverlayTexture;
        internal ColorModeEnum OverlayColorMode;
        internal float         OverlaySpeed;

        internal bool          EnableInnerBevel;
        internal Color         HighlightColor;
        internal ColorModeEnum HighlightColorMode;
        internal Color         ShadowColor;
        internal ColorModeEnum ShadowColorMode;
        internal Vector2       HighlightOffset;

        internal byte GrayScale;

        public bool Equals(UIEffectMaterialKey o)
        {
            if (this.BlurDistance != o.BlurDistance)
            {
                return false;
            }

            if (this.OverlayTexture != o.OverlayTexture)
            {
                return false;
            }

            if (this.OverlayColorMode != o.OverlayColorMode)
            {
                return false;
            }

            if (this.OverlaySpeed != o.OverlaySpeed)
            {
                return false;
            }

            if (this.EnableInnerBevel != o.EnableInnerBevel)
            {
                return false;
            }

            if (this.HighlightColor != o.HighlightColor)
            {
                return false;
            }

            if (this.HighlightColorMode != o.HighlightColorMode)
            {
                return false;
            }

            if (this.ShadowColor != o.ShadowColor)
            {
                return false;
            }

            if (this.ShadowColorMode != o.ShadowColorMode)
            {
                return false;
            }

            if (this.HighlightOffset != o.HighlightOffset)
            {
                return false;
            }

            if (this.GrayScale != o.GrayScale)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = this.BlurDistance.GetHashCode();
            if (this.OverlayTexture != null)
            {
                hash = (397 * hash) ^ this.OverlayTexture.GetHashCode();
            }

            hash = (397 * hash) ^ this.OverlayColorMode.GetHashCode();
            hash = (397 * hash) ^ this.OverlaySpeed.GetHashCode();
            hash = (397 * hash) ^ this.EnableInnerBevel.GetHashCode();
            hash = (397 * hash) ^ this.HighlightColor.GetHashCode();
            hash = (397 * hash) ^ this.HighlightColorMode.GetHashCode();
            hash = (397 * hash) ^ this.ShadowColor.GetHashCode();
            hash = (397 * hash) ^ this.ShadowColorMode.GetHashCode();
            hash = (397 * hash) ^ this.HighlightOffset.GetHashCode();
            hash = (397 * hash) ^ this.GrayScale.GetHashCode();

            return hash;
        }

        internal Material CreateMaterial()
        {
            if (this.BlurDistance == 0 &&
                this.OverlayTexture == null &&
                !this.EnableInnerBevel &&
                this.GrayScale == 0)
            {
                return null;
            }

            var shader = Shader.Find("YIUI/UIEffect");
            if (shader == null)
            {
                Debug.LogError("Can not found shader: 'YIUI/UIEffect'");
                return null;
            }

            var material = new Material(shader);
            if (this.BlurDistance > 0)
            {
                material.EnableKeyword("UIEFFECT_BLUR");
                material.SetFloat(
                    "_BlurDistance", this.BlurDistance);
            }

            if (this.OverlayTexture != null)
            {
                if (this.OverlaySpeed > 0)
                {
                    material.EnableKeyword("UIEFFECT_OVERLAY_ANIMATION");
                    material.SetFloat(
                        "_OverlaySpeed", this.OverlaySpeed);
                }
                else
                {
                    material.EnableKeyword("UIEFFECT_OVERLAY");
                }

                material.SetTexture(
                    "_OverlayTex", this.OverlayTexture);
                material.SetInt(
                    "_OverlayColorMode", (int)this.OverlayColorMode);
            }

            if (this.EnableInnerBevel)
            {
                material.EnableKeyword("UIEFFECT_INNER_BEVEL");
                material.SetColor(
                    "_HighlightColor", this.HighlightColor);
                material.SetInt(
                    "_HighlightColorMode", (int)this.HighlightColorMode);
                material.SetColor(
                    "_ShadowColor", this.ShadowColor);
                material.SetInt(
                    "_ShadowColorMode", (int)this.ShadowColorMode);
                material.SetVector(
                    "_HighlightOffset", this.HighlightOffset);
            }

            if (this.GrayScale > 0)
            {
                if (this.GrayScale == 255)
                {
                    material.EnableKeyword("UIEFFECT_GRAYSCALE");
                }
                else
                {
                    material.EnableKeyword("UIEFFECT_GRAYSCALE_LERP");
                    material.SetFloat(
                        "_GrayLerp",
                        1.0f - (this.GrayScale / 255.0f));
                }
            }

            return material;
        }
    }
}