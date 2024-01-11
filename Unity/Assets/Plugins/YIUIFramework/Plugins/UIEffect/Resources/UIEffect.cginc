#ifndef UI_EFFECT_INCLUDED
#define UI_EFFECT_INCLUDED


sampler2D _TransitionTex;
sampler2D _ParamTex;

#if GRAYSCALE | SEPIA | NEGA | PIXEL | MONO | CUTOFF | HUE
#define UI_TONE
#endif

#if ADD | SUBTRACT | FILL
#define UI_COLOR
#endif

#if FASTBLUR | MEDIUMBLUR | DETAILBLUR
#define UI_BLUR
#endif

// Unpack float to low-precision [0-1] fixed4.
fixed4 UnpackToVec4(float value)
{
	const int PACKER_STEP = 64;
	const int PRECISION = PACKER_STEP - 1;
	fixed4 unpacked;

	unpacked.x = (value % PACKER_STEP) / PRECISION;
	value = floor(value / PACKER_STEP);

	unpacked.y = (value % PACKER_STEP) / PRECISION;
	value = floor(value / PACKER_STEP);

	unpacked.z = (value % PACKER_STEP) / PRECISION;
	value = floor(value / PACKER_STEP);

	unpacked.w = (value % PACKER_STEP) / PRECISION;
	return unpacked;
}

// Unpack float to low-precision [0-1] fixed3.
fixed3 UnpackToVec3(float value)
{
	const int PACKER_STEP = 256;
	const int PRECISION = PACKER_STEP - 1;
	fixed3 unpacked;

	unpacked.x = (value % (PACKER_STEP)) / (PACKER_STEP - 1);
	value = floor(value / (PACKER_STEP));

	unpacked.y = (value % PACKER_STEP) / (PACKER_STEP - 1);
	value = floor(value / PACKER_STEP);

	unpacked.z = (value % PACKER_STEP) / (PACKER_STEP - 1);
	return unpacked;
}

// Unpack float to low-precision [0-1] half2.
half2 UnpackToVec2(float value)
{
	const int PACKER_STEP = 4096;
	const int PRECISION = PACKER_STEP - 1;
	half2 unpacked;

	unpacked.x = (value % (PACKER_STEP)) / (PACKER_STEP - 1);
	value = floor(value / (PACKER_STEP));

	unpacked.y = (value % PACKER_STEP) / (PACKER_STEP - 1);
	return unpacked;
}

// Sample texture with blurring.
// * Fast: Sample texture with 3x3 kernel.
// * Medium: Sample texture with 5x5 kernel.
// * Detail: Sample texture with 7x7 kernel.
fixed4 Tex2DBlurring (sampler2D tex, half2 texcood, half2 blur, half4 mask)
{
	#if FASTBLUR && EX
	const int KERNEL_SIZE = 5;
	const float KERNEL_[5] = { 0.2486, 0.7046, 1.0, 0.7046, 0.2486};
	#elif MEDIUMBLUR && EX
	const int KERNEL_SIZE = 9;
	const float KERNEL_[9] = { 0.0438, 0.1719, 0.4566, 0.8204, 1.0, 0.8204, 0.4566, 0.1719, 0.0438};
	#elif DETAILBLUR && EX
	const int KERNEL_SIZE = 13;
	const float KERNEL_[13] = { 0.0438, 0.1138, 0.2486, 0.4566, 0.7046, 0.9141, 1.0, 0.9141, 0.7046, 0.4566, 0.2486, 0.1138, 0.0438};
	#elif FASTBLUR
	const int KERNEL_SIZE = 3;
	const float KERNEL_[3] = { 0.4566, 1.0, 0.4566};
	#elif MEDIUMBLUR
	const int KERNEL_SIZE = 5;
	const float KERNEL_[5] = { 0.2486, 0.7046, 1.0, 0.7046, 0.2486};
	#elif DETAILBLUR
	const int KERNEL_SIZE = 7;
	const float KERNEL_[7] = { 0.1719, 0.4566, 0.8204, 1.0, 0.8204, 0.4566, 0.1719};
	#else
	const int KERNEL_SIZE = 1;
	const float KERNEL_[1] = { 1.0 };
	#endif
	float4 o = 0;
	float sum = 0;
	float2 shift = 0;
	for(int x = 0; x < KERNEL_SIZE; x++)
	{
		shift.x = blur.x * (float(x) - KERNEL_SIZE/2);
		for(int y = 0; y < KERNEL_SIZE; y++)
		{
			shift.y = blur.y * (float(y) - KERNEL_SIZE/2);
			float2 uv = texcood + shift;
			float weight = KERNEL_[x] * KERNEL_[y];
			sum += weight;
			#if EX
			fixed masked = min(mask.x <= uv.x, uv.x <= mask.z) * min(mask.y <= uv.y, uv.y <= mask.w);
			o += lerp(fixed4(0.5, 0.5, 0.5, 0), tex2D(tex, uv), masked) * weight;
			#else
			o += tex2D(tex, uv) * weight;
			#endif
		}
	}
	return o / sum;
}

// Sample texture with blurring.
// * Fast: Sample texture with 3x3 kernel.
// * Medium: Sample texture with 5x5 kernel.
// * Detail: Sample texture with 7x7 kernel.
fixed4 Tex2DBlurring (sampler2D tex, half2 texcood, half2 blur)
{
	return Tex2DBlurring(tex, texcood, blur, half4(0,0,1,1));
}


// Sample texture with blurring.
// * Fast: Sample texture with 3x1 kernel.
// * Medium: Sample texture with 5x1 kernel.
// * Detail: Sample texture with 7x1 kernel.
fixed4 Tex2DBlurring1D (sampler2D tex, half2 uv, half2 blur)
{
	#if FASTBLUR
	const int KERNEL_SIZE = 3;
	#elif MEDIUMBLUR
	const int KERNEL_SIZE = 5;
	#elif DETAILBLUR
	const int KERNEL_SIZE = 7;
	#else
	const int KERNEL_SIZE = 1;
	#endif
	float4 o = 0;
	float sum = 0;
	float weight;
	half2 texcood;
	for(int i = -KERNEL_SIZE/2; i <= KERNEL_SIZE/2; i++)
	{
		texcood = uv;
		texcood.x += blur.x * i;
		texcood.y += blur.y * i;
		weight = 1.0/(abs(i)+2);
		o += tex2D(tex, texcood)*weight;
		sum += weight;
	}
	return o / sum;
}

fixed3 shift_hue(fixed3 RGB, half VSU, half VSW)
{
	fixed3 result;
	result.x = (0.299 + 0.701*VSU + 0.168*VSW)*RGB.x
		+ (0.587 - 0.587*VSU + 0.330*VSW)*RGB.y
		+ (0.114 - 0.114*VSU - 0.497*VSW)*RGB.z;

	result.y = (0.299 - 0.299*VSU - 0.328*VSW)*RGB.x
		+ (0.587 + 0.413*VSU + 0.035*VSW)*RGB.y
		+ (0.114 - 0.114*VSU + 0.292*VSW)*RGB.z;

	result.z = (0.299 - 0.3*VSU + 1.25*VSW)*RGB.x
		+ (0.587 - 0.588*VSU - 1.05*VSW)*RGB.y
		+ (0.114 + 0.886*VSU - 0.203*VSW)*RGB.z;

	return result;
}

// Apply tone effect.
fixed4 ApplyToneEffect(fixed4 color, fixed factor)
{
	#ifdef GRAYSCALE
	color.rgb = lerp(color.rgb, Luminance(color.rgb), factor);

	#elif SEPIA
	color.rgb = lerp(color.rgb, Luminance(color.rgb) * half3(1.07, 0.74, 0.43), factor);

	#elif NEGA
	color.rgb = lerp(color.rgb, 1 - color.rgb, factor);
	#endif

	return color;
}

// Apply color effect.
fixed4 ApplyColorEffect(half4 color, half4 factor)
{
	#if FILL
	color.rgb = lerp(color.rgb, factor.rgb, factor.a);

	#elif ADD
	color.rgb += factor.rgb * factor.a;

	#elif SUBTRACT
	color.rgb -= factor.rgb * factor.a;

	#else
	color.rgb = lerp(color.rgb, color.rgb * factor.rgb, factor.a);
	#endif

	#if CUTOFF
	color.a = factor.a;
	#endif

	return color;
}

// Apply transition effect.
fixed4 ApplyTransitionEffect(half4 color, half3 transParam)
{
	fixed4 param = tex2D(_ParamTex, float2(0.25, transParam.z));
	float alpha = tex2D(_TransitionTex, transParam.xy).a;

	#if REVERSE
    fixed effectFactor = 1 - param.x;
	#else
    fixed effectFactor = param.x;
	#endif

	#if FADE
	color.a *= saturate(alpha + (1 - effectFactor * 2));
	#elif CUTOFF
	color.a *= step(0.001, color.a * alpha - effectFactor);
	#elif DISSOLVE
    fixed width = param.y/4;
    fixed softness = param.z;
	fixed3 dissolveColor = tex2D(_ParamTex, float2(0.75, transParam.z)).rgb;
	float factor = alpha - effectFactor * ( 1 + width ) + width;
	fixed edgeLerp = step(factor, color.a) * saturate((width - factor)*16/ softness);
	color = ApplyColorEffect(color, fixed4(dissolveColor, edgeLerp));
	color.a *= saturate((factor)*32/ softness);
	#endif

	return color;
}


// Apply shiny effect.
half4 ApplyShinyEffect(half4 color, half2 shinyParam)
{
	fixed nomalizedPos = shinyParam.x;
	fixed4 param1 = tex2D(_ParamTex, float2(0.25, shinyParam.y));
	fixed4 param2 = tex2D(_ParamTex, float2(0.75, shinyParam.y));
	half location = param1.x * 2 - 0.5;
	fixed width = param1.y;
	fixed soft = param1.z;
	fixed brightness = param1.w;
	fixed gloss = param2.x;
	half normalized = 1 - saturate(abs((nomalizedPos - location) / width));
	half shinePower = smoothstep(0, soft, normalized);
	half3 reflectColor = lerp(fixed3(1,1,1), color.rgb * 7, gloss);

	color.rgb += color.a * (shinePower / 2) * brightness * reflectColor;


	return color;
}

half3 RgbToHsv(half3 c) {
	half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
	half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

	half d = q.x - min(q.w, q.y);
	half e = 1.0e-10;
	return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

half3 HsvToRgb(half3 c) {
	c = half3(c.x, clamp(c.yz, 0.0, 1.0));
	half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, clamp(p.xyz - K.xxx, 0.0, 1.0), c.y);
}


// Apply Hsv effect.
half4 ApplyHsvEffect(half4 color, half param)
{
	fixed4 param1 = tex2D(_ParamTex, float2(0.25, param));
	fixed4 param2 = tex2D(_ParamTex, float2(0.75, param));
    fixed3 targetHsv = param1.rgb;

    fixed3 targetRange = param1.w;
    fixed3 hsvShift = param2.xyz - 0.5;
	half3 hsv = RgbToHsv(color.rgb);
	half3 range = abs(hsv - targetHsv);
	half diff = max(max(min(1-range.x, range.x), min(1-range.y, range.y)/10), min(1-range.z, range.z)/10);

	fixed masked = step(diff, targetRange);
	color.rgb = HsvToRgb(hsv + hsvShift * masked);

	return color;
}


#endif // UI_EFFECT_INCLUDED
