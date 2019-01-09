Shader "Diffuse - Always visible" {
	Properties {
		_NotVisibleColor ("X-ray color (RGB)", Color) = (0,1,0,1)
		_Color ("Main Color",Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque-1" }
		LOD 200
		
            

            Pass {
            	ZTest LEqual
	            Material {
	                Diffuse [_Color]
	                Ambient [_Color]
	            }
	            Lighting On
	            
	            SetTexture [_MainTex] {
					Combine texture * primary DOUBLE, texture * primary
				} 
	        }
           
		 Pass {
            
        	ZTest Greater
        	
        	Material {
        		Diffuse [_NotVisibleColor]
        	}
        	
        	Color [_NotVisibleColor]
        	
        }
        
		
		
		
        
		
	} 
	FallBack "Diffuse"
}
