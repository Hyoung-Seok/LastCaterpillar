Shader "Custom/ReticleRing"
{
    Properties
    {
        _Color      ("Color", Color)               = (0,1,0,1)
        _Radius     ("Radius", Range(0,0.5))       = 0.45
        _Thickness  ("Thickness", Range(0.001,0.3))= 0.03
        _Softness   ("Softness", Range(0.0001,0.05))= 0.005
        _DashCount  ("Dash Count", Float)          = 24
        _DashRatio  ("Dash Ratio", Range(0,1))     = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        // 항상 위에 보이게 하려면 아래 줄 주석 해제:
        // ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float  _Radius;
                float  _Thickness;
                float  _Softness;
                float  _DashCount;
                float  _DashRatio;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 p = IN.uv - 0.5;          // 중심을 원점으로
                float  d = length(p);            // 중심에서의 거리

                float halfW = _Thickness * 0.5;
                // 반지름 근처에서만 1인 얇은 링 (안쪽 halfW, 바깥 softness 페이드)
                float ring = 1.0 - smoothstep(halfW, halfW + _Softness, abs(d - _Radius));

                // 각도로 점선
                float ang  = atan2(p.y, p.x) * 0.1591549 + 0.5;   // 0..1
                float seg  = frac(ang * _DashCount);
                float dash = 1.0 - smoothstep(_DashRatio, _DashRatio + 0.03, seg);

                float alpha = ring * dash * _Color.a;
                return half4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
