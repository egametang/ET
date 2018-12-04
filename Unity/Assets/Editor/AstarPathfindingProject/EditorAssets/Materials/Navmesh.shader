// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/AstarPathfindingProject/Navmesh" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,0.5)
	_MainTex ("Texture", 2D) = "white" { }
	_Scale ("Scale", float) = 1
	_FadeColor ("Fade Color", Color) = (1,1,1,0.3)
}
SubShader {

	Pass {
		ZWrite On
		ColorMask 0
	}

	Tags {"Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200



	Offset -2, -20
	Cull Off
	
	Pass {
		ZWrite Off
		ZTest Greater
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"
		#include "Navmesh.cginc"

		sampler2D _MainTex;
		float _Scale;
		float4 _Color;
		float4 _FadeColor;

		struct v2f {
			float4  pos : SV_POSITION;
			float2  uv : TEXCOORD0;
			float4 col : COLOR;
		};

		float4 _MainTex_ST;

		v2f vert (appdata_color v) {
			v2f o;
			o.pos = UnityObjectToClipPos (v.vertex);

			float4 worldSpace = mul (unity_ObjectToWorld, v.vertex);
			o.uv = float2 (worldSpace.x*_Scale,worldSpace.z*_Scale);
			o.col = v.color * _Color * _FadeColor;
			if (!IsGammaSpaceCompatibility()) {
				o.col.rgb = GammaToLinearSpace(o.col.rgb);
			}
			return o;
		}

		float4 frag (v2f i) : COLOR {
			return tex2D (_MainTex, i.uv) * i.col;
		}
		ENDCG

	 }

	Pass {
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"
		#include "Navmesh.cginc"

		float4 _Color;
		sampler2D _MainTex;
		float _Scale;

		struct v2f {
			float4  pos : SV_POSITION;
			float2  uv : TEXCOORD0;
			float4 col : COLOR;
		};

		v2f vert (appdata_color v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos (v.vertex);

			float4 worldSpace = mul (unity_ObjectToWorld, v.vertex);
			o.uv = float2 (worldSpace.x*_Scale,worldSpace.z*_Scale);
			o.col = v.color * _Color;
			if (!IsGammaSpaceCompatibility()) {
				o.col.rgb = GammaToLinearSpace(o.col.rgb);
			}
			return o;
		}

		float4 frag (v2f i) : COLOR
		{
			return tex2D (_MainTex, i.uv) * i.col;
		}
		ENDCG

		}


	}
Fallback Off
}
