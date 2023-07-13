// Standard shader for mobile
// Written by Nihal Mirpuri @nylonee

Shader "Mobile/Standard"
{
  Properties
  {
    _MainTex("Albedo", 2D) = "white" {}

    [Toggle(COLOR_ON)] _ColorToggle("Color, Brightness, Contrast Toggle", Int) = 0
    _Color("Color", Color) = (1,1,1)
    _Brightness ("Brightness", Range(-10.0, 10.0)) = 0.0
    _Contrast ("Contrast", Range(0.0, 3.0)) = 1

    [Toggle(PHONG_ON)] _Phong("Point Light Toggle", Int) = 0
    _PointLightColor("Point Light Color", Color) = (1,1,1,1)
    _PointLightPosition("Point Light Position", Vector) = (0.0,0.0,0.0)
    _AmbiencePower("Ambience intensity", Range(0.0,2.0)) = 1.0
    _SpecularPower("Specular intensity", Range(0.0,2.0)) = 1.0
    _DiffusePower("Diffuse intensity", Range(0.0,2.0)) = 1.0

    [Toggle(DETAIL_ON)] _Detail("Detail Map Toggle", Int) = 0
    _DetailMap("Detail Map", 2D) = "white" {}
    _DetailStrength("Detail Map Strength", Range(0.0, 2.0)) = 1
    [Toggle(DETAIL_MASK_ON)] _Mask("Detail Mask Toggle", Int) = 0
    _DetailMask("Detail Mask", 2D) = "white" {}

    [Toggle(EMISSION_ON)] _Emission("Emission Map Toggle", Int) = 0
    _EmissionMap("Emission", 2D) = "white" {}
    _EmissionStrength("Emission Strength", Range(0.0,10.0)) = 1

    [Toggle(NORMAL_ON)] _Normal("Normal Map Toggle", Int) = 0
    _NormalMap("Normal Map", 2D) = "bump" {}
  }

  SubShader {
  	Tags { "RenderType" = "Opaque" }
  	LOD 150

    // Render the relevant pass based on whether lightmap data is being passed in
    Pass {
      Tags { "LightMode" = "VertexLM" }
      Lighting Off
      Cull Back
  		CGPROGRAM
      #pragma vertex vert_lm
      #pragma fragment frag_lm

      #pragma multi_compile_fog
      #pragma skip_variants FOG_LINEAR FOG_EXP

      #pragma shader_feature COLOR_ON
      #pragma shader_feature PHONG_ON
      #pragma shader_feature DETAIL_ON
      #pragma shader_feature DETAIL_MASK_ON
      #pragma shader_feature EMISSION_ON
      #pragma shader_feature NORMAL_ON

      #include "StandardMobile.cginc"
      ENDCG
    }

    Pass {
      Tags { "LightMode" = "VertexLMRGBM" }
      Lighting Off
      Cull Back
      CGPROGRAM
      #pragma vertex vert_lm
      #pragma fragment frag_lm

      #pragma multi_compile_fog
      #pragma skip_variants FOG_LINEAR FOG_EXP

      #pragma shader_feature COLOR_ON
      #pragma shader_feature PHONG_ON
      #pragma shader_feature DETAIL_ON
      #pragma shader_feature DETAIL_MASK_ON
      #pragma shader_feature EMISSION_ON
      #pragma shader_feature NORMAL_ON

      #include "StandardMobile.cginc"
      ENDCG
    }

    Pass {
      Tags { "LightMode" = "Vertex" }
      Lighting Off
      Cull Back
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #pragma multi_compile_fog
      #pragma skip_variants FOG_LINEAR FOG_EXP

      #pragma shader_feature COLOR_ON
      #pragma shader_feature PHONG_ON
      #pragma shader_feature DETAIL_ON
      #pragma shader_feature DETAIL_MASK_ON
      #pragma shader_feature EMISSION_ON
      #pragma shader_feature NORMAL_ON

      #include "StandardMobile.cginc"
      ENDCG
    }

  }

  FallBack "Mobile/VertexLit"
}
