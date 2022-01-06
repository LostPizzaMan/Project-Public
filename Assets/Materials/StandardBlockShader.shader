Shader "Block/StandardBlockShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 0

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows addshadow 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        fixed4 _Color;
        float _sunlightIntensity;
        float4 torchLight;
        float4 sunLight;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 colorPacked = IN.color;

            torchLight = colorPacked;
            torchLight.a = 1;

            sunLight.r = colorPacked.a;
            sunLight.g = colorPacked.a;
            sunLight.b = colorPacked.a;
            sunLight.a = 1;

            float4 lightIntensity = clamp(torchLight + sunLight * _sunlightIntensity, 0.0625, 1);

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * lightIntensity;
          
            clip(c.a - 1);
            
            o.Albedo = c.rgb * _Color;
            o.Alpha = c.a;          
        }
        ENDCG
    }
    FallBack "Diffuse"
}
