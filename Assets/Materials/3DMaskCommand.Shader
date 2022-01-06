Shader "Block/3DMaskCommand" {
	Properties {
		_Color ("Tint", Color) = (0, 0, 0, 1)
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        //stencil operation
		Stencil{
			Ref 1
			Comp Equal
		}

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows 
		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
          
            clip(c.a - 1);
            
            o.Albedo = c.rgb * _Color;
            o.Alpha = c.a;          
		}
		ENDCG
	}
	FallBack "Diffuse"
}