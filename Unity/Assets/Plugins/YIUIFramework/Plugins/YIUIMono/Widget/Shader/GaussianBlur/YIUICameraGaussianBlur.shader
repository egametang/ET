Shader "YIUIShader/YIUICameraGaussianBlur"
{
    // 高斯模糊
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {} // 主纹理
        _BlurSize ("Blur Size", Float) = 1.0 // 模糊尺寸(纹理坐标的偏移量)
    }

    SubShader
    {
        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex; // 主纹理
        half4 _MainTex_TexelSize; // _MainTex的像素尺寸大小, float4(1/width, 1/height, width, height)
        float _BlurSize; // 模糊尺寸(纹理坐标的偏移量)

        struct v2f
        {
            float4 pos : SV_POSITION; // 模型空间顶点坐标
            half2 uv[5]: TEXCOORD0; // 5个邻域的纹理坐标
        };

        v2f vertBlurVertical(appdata_img v)
        {
            // 垂直模糊顶点着色器
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex); // 模型空间顶点坐标变换到裁剪空间, 等价于: mul(UNITY_MATRIX_MVP, v.vertex)
            half2 uv = v.texcoord;
            o.uv[0] = uv;
            o.uv[1] = uv + float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
            o.uv[2] = uv - float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
            o.uv[3] = uv + float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;
            o.uv[4] = uv - float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;
            return o;
        }

        v2f vertBlurHorizontal(appdata_img v)
        {
            // 水平模糊顶点着色器
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex); // 模型空间顶点坐标变换到裁剪空间, 等价于: mul(UNITY_MATRIX_MVP, v.vertex)
            half2 uv = v.texcoord;
            o.uv[0] = uv;
            o.uv[1] = uv + float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[2] = uv - float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[3] = uv + float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;
            o.uv[4] = uv - float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;
            return o;
        }

        fixed4 fragBlur(v2f i) : SV_Target
        {
            float weight[3] = {0.4026, 0.2442, 0.0545}; // 大小为5的一维高斯核，实际只需记录3个权值
            fixed3 sum = tex2D(_MainTex, i.uv[0]).rgb * weight[0];
            for (int j = 1; j < 3; j++)
            {
                sum += tex2D(_MainTex, i.uv[j * 2 - 1]).rgb * weight[j]; // 中心右侧或下侧的纹理*权值
                sum += tex2D(_MainTex, i.uv[j * 2]).rgb * weight[j]; // 中心左侧或上侧的纹理*权值
            }
            return fixed4(sum, 1.0);
        }
        ENDCG

        ZTest Always Cull Off ZWrite Off

        Pass
        {
            NAME "GAUSSIAN_BLUR_VERTICAL"

            CGPROGRAM
            #pragma vertex vertBlurVertical
            #pragma fragment fragBlur
            ENDCG
        }

        Pass
        {
            NAME "GAUSSIAN_BLUR_HORIZONTAL"

            CGPROGRAM
            #pragma vertex vertBlurHorizontal
            #pragma fragment fragBlur
            ENDCG
        }
    }

    FallBack "Diffuse"
}