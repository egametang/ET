Shader "YIUI/UIEffect"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

        _OverlayTex("Overlay Texture", 2D) = "white" {}
        _OverlaySpeed("Overlay Texture Speed", Float) = 1
        _BlurDistance("Blur Distance", Float) = 0.015
        _GrayLerp("Grayscale Lerp", Float) = 1
        _LightOffset("Light Offset", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            //#pragma multi_compile __ UIEFFECT_OVERLAY UIEFFECT_OVERLAY_ANIMATION
            #pragma multi_compile __ UIEFFECT_GRAYSCALE UIEFFECT_GRAYSCALE_LERP
            #pragma multi_compile __ UIEFFECT_WHITEALL

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                #if defined(UIEFFECT_OVERLAY) || defined(UIEFFECT_OVERLAY_ANIMATION) || defined(UIEFFECT_INNER_BEVEL)
				float2 texcoord1 : TEXCOORD1;
                #endif
                #if defined(UIEFFECT_INNER_BEVEL)
				float4 tangent : TANGENT;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                #if defined(UIEFFECT_OVERLAY) || defined(UIEFFECT_OVERLAY_ANIMATION) || defined(UIEFFECT_INNER_BEVEL)
				half4 texcoord : TEXCOORD0;
                #else
                half2 texcoord : TEXCOORD0;
                #endif
                #if defined(UIEFFECT_INNER_BEVEL)
				float4 tangent : TEXCOORD1;
                #endif
                float4 worldPosition : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            half _IsInUICamera;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                #if defined(UIEFFECT_OVERLAY) || defined(UIEFFECT_OVERLAY_ANIMATION) || defined(UIEFFECT_INNER_BEVEL)
				OUT.texcoord.xy = IN.texcoord;
				OUT.texcoord.zw = IN.texcoord1;
                #else
                OUT.texcoord = IN.texcoord;
                #endif

                #if defined(UIEFFECT_INNER_BEVEL)
				OUT.tangent = IN.tangent;
                #endif
                //IN.color.rgb *= 
                OUT.color = IN.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            // Overlay
            sampler2D _OverlayTex;
            int _OverlayColorMode;
            float _OverlaySpeed;

            // Inner Bevel
            fixed4 _HighlightColor;
            int _HighlightColorMode;
            fixed4 _ShadowColor;
            int _ShadowColorMode;
            half2 _HighlightOffset;

            // Blur.
            float _BlurDistance;

            // Grayscale.
            float _GrayLerp;

            // LightOffset.
            float _LightOffset;

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord.xy) + _TextureSampleAdd) * IN.color;

                #ifdef UIEFFECT_BLUR
				color += tex2D(_MainTex, half2(IN.texcoord.x + _BlurDistance, IN.texcoord.y + _BlurDistance)) * IN.color;
				color += tex2D(_MainTex, half2(IN.texcoord.x + _BlurDistance, IN.texcoord.y)) * IN.color;
				color += tex2D(_MainTex, half2(IN.texcoord.x, IN.texcoord.y + _BlurDistance)) * IN.color;
				color += tex2D(_MainTex, half2(IN.texcoord.x - _BlurDistance, IN.texcoord.y - _BlurDistance)) * IN.color;
				color += tex2D(_MainTex, half2(IN.texcoord.x + _BlurDistance, IN.texcoord.y - _BlurDistance)) * IN.color;
				color += tex2D(_MainTex, half2(IN.texcoord.x - _BlurDistance, IN.texcoord.y + _BlurDistance)) * IN.color;
				color += tex2D(_MainTex, half2(IN.texcoord.x - _BlurDistance, IN.texcoord.y)) * IN.color;
				color += tex2D(_MainTex, half2(IN.texcoord.x, IN.texcoord.y - _BlurDistance)) * IN.color;
				color = color / 9;
                #endif

                #if defined(UIEFFECT_OVERLAY) || defined(UIEFFECT_OVERLAY_ANIMATION)
				half2 uv = IN.texcoord.zw;
                #	ifdef UIEFFECT_OVERLAY_ANIMATION
				uv.x += _Time.y * _OverlaySpeed;
                #	endif
				half4 overlay = tex2D(_OverlayTex, uv);
				if (_OverlayColorMode == 0)
				{
					color.rgb = color.rgb * (1 - overlay.a) + overlay.rgb * overlay.a;
				}
				else if (_OverlayColorMode == 1)
				{
					color.rgb += overlay.rgb;
				}
				else if (_OverlayColorMode == 2)
				{
					color.rgb = half3(1, 1, 1) - (half3(1, 1, 1) - color.rgb) * (half3(1, 1, 1) - overlay.rgb);
				}
                #endif

                #if defined(UIEFFECT_INNER_BEVEL)
				float factor = min(IN.texcoord.z, 1);
				half2 highlightOffset = _HighlightOffset.x * half2(IN.tangent.xy) - _HighlightOffset.y * half2(IN.tangent.zw);
				fixed shadowColAlpha = (1 - tex2D(_MainTex, IN.texcoord + highlightOffset).a) * _ShadowColor.a * factor;

				if (_ShadowColorMode == 0)
				{
					color.rgb = color.rgb * (1 - shadowColAlpha) + _ShadowColor.rgb * shadowColAlpha;
				}
				else if (_ShadowColorMode == 1)
				{
					color.rgb = color.rgb + _ShadowColor.rgb * shadowColAlpha;
				}
				else
				{
					color.rgb = color.rgb * (1 - shadowColAlpha) + color.rgb * _ShadowColor.rgb * shadowColAlpha;
				}

				fixed highlightColAlpha = (1 - tex2D(_MainTex, IN.texcoord - highlightOffset).a) * _HighlightColor.a * factor;
				if (_HighlightColorMode == 0)
				{
					color.rgb = color.rgb * (1 - highlightColAlpha) + _HighlightColor.rgb * highlightColAlpha;
				}
				else if (_HighlightColorMode == 1)
				{
					color.rgb = color.rgb + _HighlightColor.rgb * highlightColAlpha;
				}
				else
				{
					color.rgb = color.rgb * (1 - highlightColAlpha) + color.rgb * _HighlightColor.rgb * highlightColAlpha;
				}
                #endif

                color.rgb = LinearToGammaSpace(color.rgb) * _LightOffset;

                #ifdef UIEFFECT_GRAYSCALE
				color.rgb = Luminance(color.rgb) * 1; //全灰状态下，设置X倍亮度 
                #elif UIEFFECT_GRAYSCALE_LERP
				color.rgb = lerp(Luminance(color.rgb), color.rgb, _GrayLerp);
                #endif

                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                #ifdef UNITY_UI_ALPHACLIP
			    clip(color.a - 0.001);
                #endif

                #ifdef UIEFFECT_WHITEALL
				color = (1, 1, 1, color.a);
                #endif
                return color;
            }
            ENDCG
        }
    }
}