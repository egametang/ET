struct appdata_color {
	float4 vertex : POSITION;
	fixed4 color : COLOR;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};

struct line_v2f {
	half4 col : COLOR;
	float2 normal : TEXCOORD0;
	float4 screenPos : TEXCOORD1;
	float4 originScreenPos : TEXCOORD2;
};

// Redefine the matrix here in order to confuse the Unity shader upgrader.
// Otherwise it will start modifying some things below even though this script is specifically
// written to be compatible with multiple versions of Unity.
#define MVP_Matrix UNITY_MATRIX_MVP

// d = normalized distance to line
float lineAA(float d) {
	d = max(min(d, 1.0), 0) * 1.116;
	float v = 0.93124*d*d*d - 1.42215*d*d - 0.42715*d + 0.95316;
	v /= 0.95316;
	return max(v, 0);
}

line_v2f line_vert (appdata_color v, float pixelWidth, out float4 outpos : SV_POSITION) {
	line_v2f o;
	// UnityObjectToClipPos only exists in Unity 5.4 or above and there it has to be used
#if defined(UNITY_USE_PREMULTIPLIED_MATRICES)
	float4 Mv = UnityObjectToClipPos(v.vertex);
	float4 Mn = UnityObjectToClipPos(float4(v.normal.x, v.normal.y, v.normal.z, 0));
#else
	float4 Mv = mul(MVP_Matrix, v.vertex);
	float4 Mn = mul(MVP_Matrix, float4(v.normal.x, v.normal.y, v.normal.z, 0));
#endif
	// delta is the limit value of doing the calculation
	// x1 = M*v
	// x2 = M*(v + e*n)
	// lim e->0 (x2/x2.w - x1.w)/e
	// Where M = UNITY_MATRIX_MVP, v = v.vertex, n = v.normal, e = a very small value
	// Previously the above calculation was done with just e = 0.001, however this could yield graphical artifacts
	// at large coordinate values as the floating point coordinates would start to run out of precision.
	// Essentially we calculate the normal of the line in screen space.
	float4 delta = (Mn - Mv*Mn.w/Mv.w) / Mv.w;

	// Handle DirectX properly. See https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
	float2 screenSpaceNormal = float2(-delta.y, delta.x) * _ProjectionParams.x;
	float2 normalizedScreenSpaceNormal = normalize(screenSpaceNormal);
	screenSpaceNormal = normalizedScreenSpaceNormal / _ScreenParams.xy;
	float4 sn = float4(screenSpaceNormal.x, screenSpaceNormal.y, 0, 0);
	
	if (Mv.w < 0) {
		// Seems to have a very minor effect, but the distance
		// seems to be more accurate with this enabled
		sn *= -1;
	}
	
	float side = (v.uv.x - 0.5)*2;
	outpos = (Mv / Mv.w) + side*sn*pixelWidth*0.5;
	// Multiply by w because homogeneous coordinates (it still needs to be clipped)
	outpos *= Mv.w;
	o.normal = normalizedScreenSpaceNormal;
	o.originScreenPos = ComputeScreenPos(Mv);
	o.screenPos = ComputeScreenPos(outpos);
	return o;
}

/** Copied from UnityCG.cginc because this function does not exist in Unity 5.2 */ 
inline bool IsGammaSpaceCompatibility() {
#if defined(UNITY_NO_LINEAR_COLORSPACE)
	return true;
#else
	// unity_ColorSpaceLuminance.w == 1 when in Linear space, otherwise == 0
	return unity_ColorSpaceLuminance.w == 0;
#endif
}