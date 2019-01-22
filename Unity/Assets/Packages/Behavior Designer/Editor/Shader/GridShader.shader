Shader "Hidden/Behavior Designer/Grid" {
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 frag(v2f_img i) : Color {
                return fixed4(0.21, 0.21, 0.21, 1);
            }
            ENDCG
        }
		Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 frag(v2f_img i) : Color {
                return fixed4(0.33, 0.33, 0.33, 1);
            }
            ENDCG
        }
		Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 frag(v2f_img i) : Color {
                return fixed4(0.26, 0.26, 0.26, 1);
            }
            ENDCG
        }
		Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 frag(v2f_img i) : Color {
                return fixed4(0.27, 0.27, 0.27, 1);
            }
            ENDCG
        }
    }
}