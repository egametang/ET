Shader "Hidden/UI/Default (UIEffect)"
{
	Properties
	{
		[PerRendererData] _MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)

		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		_ParamTex ("Parameter Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
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
			#if !defined(SHADER_API_D3D11_9X) && !defined(SHADER_API_D3D9)
			#pragma target 2.0
			#else
			#pragma target 3.0
			#endif

			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			#pragma multi_compile __ GRAYSCALE SEPIA NEGA PIXEL
			#pragma multi_compile __ ADD SUBTRACT FILL
			#pragma multi_compile __ FASTBLUR MEDIUMBLUR DETAILBLUR
			#pragma multi_compile __ EX

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#define UI_EFFECT 1
			#include "UIEffect.cginc"
			#include "UIEffectSprite.cginc"

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 param = tex2D(_ParamTex, float2(0.25, IN.eParam));
                fixed effectFactor = param.x;
                fixed colorFactor = param.y;
                fixed blurFactor = param.z;

				#if PIXEL
				half2 pixelSize = max(2, (1-effectFactor*0.95) * _MainTex_TexelSize.zw);
				IN.texcoord = round(IN.texcoord * pixelSize) / pixelSize;
				#endif

				#if defined(UI_BLUR) && EX
				half4 color = (Tex2DBlurring(_MainTex, IN.texcoord, blurFactor * _MainTex_TexelSize.xy * 2, IN.uvMask) + _TextureSampleAdd);
				#elif defined(UI_BLUR)
				half4 color = (Tex2DBlurring(_MainTex, IN.texcoord, blurFactor * _MainTex_TexelSize.xy * 2) + _TextureSampleAdd);
				#else
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
				#endif

				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

				#if UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				#if defined (UI_TONE)
				color = ApplyToneEffect(color, effectFactor);
				#endif

				color = ApplyColorEffect(color, fixed4(IN.color.rgb, colorFactor));
				color.a *= IN.color.a;

				return color;
			}
		ENDCG
		}
	}
}
