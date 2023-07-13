#include "UnityCG.cginc"

// DEFINES, CONSTRUCTORS AND STRUCTS

sampler2D _MainTex;
half4 _MainTex_ST;

// Color, brightness and contrast
#if COLOR_ON
half4 _Color;
half _Brightness;
half _Contrast;
#endif

// Phong point light
#if PHONG_ON
uniform half4 _PointLightColor;
uniform half3 _PointLightPosition;

half _AmbiencePower;
half _SpecularPower;
half _DiffusePower;
#endif

// Detail map
#if DETAIL_ON
sampler2D _DetailMap;
half _DetailStrength;
#endif

// Detail mask
#if DETAIL_ON && DETAIL_MASK_ON
sampler2D _DetailMask;
#endif

// Emission map
#if EMISSION_ON
sampler2D _EmissionMap;
half _EmissionStrength;
#endif

// Normal map
#if NORMAL_ON
sampler2D _NormalMap;
#endif

struct appdata
{
  float4 vertex : POSITION;
  half2 texcoord : TEXCOORD0;
  #if PHONG_ON
  float4 normal : NORMAL;
  #endif
  #if NORMAL_ON
  float4 tangent : TANGENT;
  #endif
};

struct appdata_lm
{
  float4 vertex : POSITION;
  half2 texcoord : TEXCOORD0;
  half2 texcoord_lm : TEXCOORD1;
  #if PHONG_ON
  float4 normal : NORMAL;
  #endif
  #if NORMAL_ON
  float4 tangent : TANGENT;
  #endif
};

struct v2f
{
  float4 vertex : SV_POSITION;
  half2 uv_main : TEXCOORD0;
  UNITY_FOG_COORDS(1)
  #if PHONG_ON
  float4 worldVertex : TEXCOORD2; // worldPos
  #endif
  #if PHONG_ON || NORMAL_ON
  half3 worldNormal : TEXCOORD3;
  #endif
  #if NORMAL_ON
  // these three vectors will hold a 3x3 rotation matrix
  // that transforms from tangent to world space
  half3 tspace0 : TEXCOORD4; // tangent.x, bitangent.x, normal.x
  half3 tspace1 : TEXCOORD5; // tangent.y, bitangent.y, normal.y
  half3 tspace2 : TEXCOORD6; // tangent.z, bitangent.z, normal.z
  #endif
};

struct v2f_lm
{
  float4 vertex : SV_POSITION;
  half2 uv_main : TEXCOORD0;
  half2 uv_lm : TEXCOORD1;
  UNITY_FOG_COORDS(2)
  #if PHONG_ON
  float4 worldVertex : TEXCOORD3;
  #endif
  #if PHONG_ON || NORMAL_ON
  half3 worldNormal : TEXCOORD4;
  #endif
  #if NORMAL_ON
  // these three vectors will hold a 3x3 rotation matrix
  // that transforms from tangent to world space
  half3 tspace0 : TEXCOORD5; // tangent.x, bitangent.x, normal.x
  half3 tspace1 : TEXCOORD6; // tangent.y, bitangent.y, normal.y
  half3 tspace2 : TEXCOORD7; // tangent.z, bitangent.z, normal.z
  #endif
};

// VERTEX SHADERS

v2f vert(appdata v)
{
  v2f o;
  o.vertex = UnityObjectToClipPos(v.vertex);

  #if PHONG_ON
  o.worldVertex = mul(unity_ObjectToWorld, v.vertex);
  o.worldNormal = UnityObjectToWorldNormal(v.normal);
  #endif

  #if NORMAL_ON
  half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
  // compute bitangent from cross product of normal and tangent
  half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  half3 wBitangent = cross(o.worldNormal, wTangent) * tangentSign;
  // output the tangent space matrix
  o.tspace0 = half3(wTangent.x, wBitangent.x, o.worldNormal.x);
  o.tspace1 = half3(wTangent.y, wBitangent.y, o.worldNormal.y);
  o.tspace2 = half3(wTangent.z, wBitangent.z, o.worldNormal.z);
  #endif

  o.uv_main = TRANSFORM_TEX(v.texcoord, _MainTex);
  UNITY_TRANSFER_FOG(o, o.vertex);

  return o;
}

v2f_lm vert_lm(appdata_lm v)
{
  v2f_lm o;
  o.vertex = UnityObjectToClipPos(v.vertex); // XXX: Is this efficient?

  #if PHONG_ON
  o.worldVertex = mul(unity_ObjectToWorld, v.vertex);
  o.worldNormal = UnityObjectToWorldNormal(v.normal);
  #endif

  #if NORMAL_ON && PHONG_ON
  half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
  // compute bitangent from cross product of normal and tangent
  half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  half3 wBitangent = cross(o.worldNormal, wTangent) * tangentSign;
  // output the tangent space matrix
  o.tspace0 = half3(wTangent.x, wBitangent.x, o.worldNormal.x);
  o.tspace1 = half3(wTangent.y, wBitangent.y, o.worldNormal.y);
  o.tspace2 = half3(wTangent.z, wBitangent.z, o.worldNormal.z);
  #endif

  o.uv_main = TRANSFORM_TEX(v.texcoord, _MainTex);
  // lightmapped uv
  o.uv_lm = v.texcoord_lm.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  UNITY_TRANSFER_FOG(o, o.vertex);

  return o;
}

// FRAGMENT SHADERS

#if COLOR_ON
// Fix the brightness, contrast and color
half4 bcc(half4 main_color)
{
  main_color.rgb /= main_color.a;
  main_color.rgb = ((main_color.rgb - 0.5f) * max(_Contrast, 0)) + 0.5f;
  main_color.rgb += _Brightness * 0.05;
  main_color.rgb *= main_color.a;

  //main_color.rgb = lerp(main_color.rgb, _Color.rgb, _Color.a);
  main_color *= _Color;

  return main_color;
}
#endif

fixed4 frag(v2f i) : SV_Target
{
  half4 returnColor = tex2D(_MainTex, i.uv_main);

  #if DETAIL_ON
  half4 mask = half4(1, 1, 1, 1);
  #endif
  #if DETAIL_ON && DETAIL_MASK_ON
  mask = tex2D(_DetailMask, i.uv_main);
  #endif
  #if DETAIL_ON
  half4 detailMap = tex2D(_DetailMap, i.uv_main) * mask;
  const fixed3 constantList = fixed3(1.0, 0.5, 0.0);
  returnColor = (returnColor + _DetailStrength*detailMap) * constantList.xxxz + (returnColor + _DetailStrength*detailMap) * constantList.zzzy;
  #endif

  #if EMISSION_ON
  returnColor += tex2D(_EmissionMap, i.uv_main)*_EmissionStrength*0.2;
  #endif

  #if NORMAL_ON
  // sample the normal map, and decode from the Unity encoding
  half3 tnormal = UnpackNormal(tex2D(_NormalMap, i.uv_main));
  // transform normal from tangent to world space
  half3 worldNormal;
  worldNormal.x = dot(i.tspace0, tnormal);
  worldNormal.y = dot(i.tspace1, tnormal);
  worldNormal.z = dot(i.tspace2, tnormal);
  float3 normal = normalize(worldNormal);
  #endif
  #if !NORMAL_ON && PHONG_ON
  float3 normal = normalize(i.worldNormal.xyz);
  #endif

  #if PHONG_ON
  float3 localCoords = i.worldVertex.xyz;
  // ambient intensities
  half3 amb = returnColor.rgb * unity_AmbientSky * _AmbiencePower;
  // diffuse intensities
  half3 L = normalize(_PointLightPosition - localCoords);
  half LdotN = dot(L, normal);
  half3 dif = _PointLightColor.rgb * returnColor.rgb * saturate(LdotN) * _DiffusePower;
  // specular intensities
  half3 V = normalize(_WorldSpaceCameraPos - localCoords);
  half3 H = normalize(V+L);
  half3 spe = _PointLightColor.rgb * pow(saturate(dot(normal, H)), 25) * _SpecularPower;

  returnColor.rgb = lerp(returnColor.rgb, amb.rgb+dif.rgb+spe.rgb, _PointLightColor.a);
  #endif

  UNITY_APPLY_FOG(i.fogCoord, returnColor);

  #if COLOR_ON
  returnColor = bcc(returnColor);
  #endif

  return returnColor;
}

fixed4 frag_lm(v2f_lm i) : SV_Target
{
  half4 returnColor = tex2D(_MainTex, i.uv_main);

  #if DETAIL_ON
  half4 mask = half4(1, 1, 1, 1);
  #endif
  #if DETAIL_ON && DETAIL_MASK_ON
  mask = tex2D(_DetailMask, i.uv_main);
  #endif
  #if DETAIL_ON
  half4 detailMap = tex2D(_DetailMap, i.uv_main) * mask;
  const fixed3 constantList = fixed3(1.0, 0.5, 0.0);
  returnColor = (returnColor + _DetailStrength*detailMap) * constantList.xxxz + (returnColor + _DetailStrength*detailMap) * constantList.zzzy;
  #endif

  #if EMISSION_ON
  returnColor += tex2D(_EmissionMap, i.uv_main)*_EmissionStrength/5;
  #endif
  returnColor.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv_lm));

  #if NORMAL_ON
  // sample the normal map, and decode from the Unity encoding
  half3 tnormal = UnpackNormal(tex2D(_NormalMap, i.uv_main));
  // transform normal from tangent to world space
  half3 worldNormal;
  worldNormal.x = dot(i.tspace0, tnormal);
  worldNormal.y = dot(i.tspace1, tnormal);
  worldNormal.z = dot(i.tspace2, tnormal);
  float3 normal = normalize(worldNormal);
  #endif
  #if !NORMAL_ON && PHONG_ON
  float3 normal = normalize(i.worldNormal.xyz);
  #endif

  #if PHONG_ON
  float3 localCoords = i.worldVertex.xyz;
  // ambient intensities
  half3 amb = returnColor.rgb * unity_AmbientSky * _AmbiencePower;
  // diffuse intensities
  half3 L = normalize(_PointLightPosition - localCoords);
  half LdotN = dot(L, normal);
  half3 dif = _PointLightColor.rgb * returnColor.rgb * saturate(LdotN) * _DiffusePower;
  // specular intensities
  half3 V = normalize(_WorldSpaceCameraPos - localCoords);
  half3 H = normalize(V+L);
  half3 spe = _PointLightColor.rgb * pow(saturate(dot(normal, H)), 25) * _SpecularPower;

  returnColor.rgb = lerp(returnColor.rgb, amb.rgb+dif.rgb+spe.rgb, _PointLightColor.a);
  #endif

  UNITY_APPLY_FOG(i.fogCoord, returnColor);

  #if COLOR_ON
  returnColor = bcc(returnColor);
  #endif

  return returnColor;
}

// SURFACE SHADERS
