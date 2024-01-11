#ifndef UI_EFFECT_SPRITE_INCLUDED
#define UI_EFFECT_SPRITE_INCLUDED

fixed4 _Color;
fixed4 _TextureSampleAdd;
float4 _ClipRect;
sampler2D _MainTex;
float4 _MainTex_TexelSize;

struct appdata_t
{
	float4 vertex   : POSITION;
	float4 color    : COLOR;
	float2 texcoord : TEXCOORD0;
#if EX
	float2	uvMask	: TEXCOORD1;
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float4 vertex   : SV_POSITION;
	fixed4 color    : COLOR;
	half2 texcoord  : TEXCOORD0;
	float4 worldPosition : TEXCOORD1;
#if UI_DISSOLVE || UI_TRANSITION
	half3	eParam	: TEXCOORD2;
#elif UI_SHINY
	half2	eParam	: TEXCOORD2;
#else
	half	eParam	: TEXCOORD2;
#endif
#if EX
	half4	uvMask	: TEXCOORD3;
#endif
	UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert(appdata_t IN)
{
	v2f OUT;
	UNITY_SETUP_INSTANCE_ID(IN);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
	OUT.worldPosition = IN.vertex;
	OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

	#if UI_EFFECT
	OUT.texcoord = UnpackToVec2(IN.texcoord.x) * 2 - 0.5;
	#else
	OUT.texcoord = UnpackToVec2(IN.texcoord.x);
	#endif
	
	#ifdef UNITY_HALF_TEXEL_OFFSET
	OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
	#endif

	OUT.color = IN.color * _Color;

	#if UI_DISSOLVE || UI_TRANSITION
	OUT.eParam = UnpackToVec3(IN.texcoord.y);
	#elif UI_SHINY
	OUT.eParam = UnpackToVec2(IN.texcoord.y);
	#else
	OUT.eParam = IN.texcoord.y;
	#endif

	#if EX
	OUT.uvMask = half4(UnpackToVec2(IN.uvMask.x), UnpackToVec2(IN.uvMask.y));
	#endif

	return OUT;
}

#endif // UI_EFFECT_SPRITE_INCLUDED
