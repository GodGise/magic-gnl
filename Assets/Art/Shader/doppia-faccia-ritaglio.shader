Shader "Custom/StandardTwoSidedLeaves"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [BumpMap] _BumpMap ("Normal Map", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 200

        // Выключаем отсечение задней стороны
        Cull Off

        CGPROGRAM
        // Используем точно такую же модель Standard, как в оригинальном шейдере
        #pragma surface surf Standard fullforwardshadows alphatest:_Cutoff addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            fixed facing : VFACE; // Определяем, какая сторона полигона перед нами
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            // Разворачиваем нормаль для обратной стороны полигонов,
            // чтобы они не уходили в полную черную тень
            float3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            if (IN.facing < 0)
                normal.z = -normal.z;
                
            o.Normal = normal;
        }
        ENDCG
    }
    FallBack "Transparent/Cutout/Diffuse"
}