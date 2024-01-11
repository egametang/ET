Shader "YIUIShader/YIUICameraMotionBlur"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {} // 主纹理
        _BlurAmount ("Blur Amount", Float) = 1.0 // 模糊值, 通过alpha通道控制当前屏幕纹理与历史屏幕纹理进行混合
    }

    SubShader
    {
        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex; // 主纹理
        fixed _BlurAmount; // 模糊值, 通过alpha通道控制当前屏幕纹理与历史屏幕纹理进行混合

        fixed4 fragRGB(v2f_img i) : SV_Target
        {
            // v2f_img为内置结构体, 里面只包含pos和uv
            return fixed4(tex2D(_MainTex, i.uv).rgb, _BlurAmount);
        }

        half4 fragA(v2f_img i) : SV_Target
        {
            // v2f_img为内置结构体, 里面只包含pos和uv
            return tex2D(_MainTex, i.uv);
        }
        ENDCG

        ZTest Always Cull Off ZWrite Off

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB // 允许通过的颜色通道, 取值有: 0、R、G、B、A、RGBA的组合(RG、RGB等)

            CGPROGRAM
            #pragma vertex vert_img // 使用内置的vert_img顶点着色器
            #pragma fragment fragRGB // _BlurAmount只参与混合, 不影响alpha值
            ENDCG
        }

        Pass
        {
            Blend One Zero
            ColorMask A // 允许通过的颜色通道, 取值有: 0、R、G、B、A、RGBA的组合(RG、RGB等)

            CGPROGRAM
            #pragma vertex vert_img // 使用内置的vert_img顶点着色器
            #pragma fragment fragA // 使用纹理原本的alpha值
            ENDCG
        }
    }

    FallBack Off
}