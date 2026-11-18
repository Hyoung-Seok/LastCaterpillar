Shader "Custom/DamageZoneAlert"
{
    // 장판 경고 표시용 원형 셰이더.
    // 메시: 유니티 기본 Cylinder(오브젝트 공간 반지름 0.5)를 납작하게 눌러 쓴다고 가정.
    //       UV 대신 오브젝트 공간 xz 좌표로 원을 그리므로 Quad(XY 평면)에는 맞지 않음.
    // 두께 계열 값(OutlineWidth/GlowWidth/FrontWidth)은 "미터" 단위 → 장판 반지름이 바뀌어도 외곽선 굵기 유지.
    Properties
    {
        [Header(Outline)]
        _OutlineColor   ("Outline Color", Color)            = (1, 0.25, 0.1, 1)
        _OutlineWidth   ("Outline Width (m)", Range(0, 0.5)) = 0.06
        _GlowWidth      ("Inner Glow Width (m)", Range(0.001, 2)) = 0.35
        _GlowStrength   ("Inner Glow Strength", Range(0, 1)) = 0.5

        [Header(Inner)]
        _InnerColor     ("Inner Color", Color)              = (1, 0.1, 0.05, 0.12)

        [Header(Fill)]
        _FillColor      ("Fill Color", Color)               = (1, 0.15, 0.05, 0.3)
        _Fill           ("Fill (0=empty, 1=full)", Range(0, 1)) = 0
        _FrontColor     ("Fill Front Color", Color)         = (1, 0.4, 0.2, 0.8)
        _FrontWidth     ("Fill Front Width (m)", Range(0, 0.3)) = 0.04
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back   // Cylinder 바닥 뚜껑까지 그리면 안쪽 색이 이중으로 겹침

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 posOS       : TEXCOORD0; // 오브젝트 공간 xz
                float  worldRadius : TEXCOORD1; // 월드 반지름(m)
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float  _OutlineWidth;
                float  _GlowWidth;
                float  _GlowStrength;
                float4 _InnerColor;
                float4 _FillColor;
                float  _Fill;
                float4 _FrontColor;
                float  _FrontWidth;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.posOS = IN.positionOS.xz;
                // 오브젝트 X축 스케일 × 0.5 = 월드 반지름 (xz 균일 스케일 가정)
                float3 axisX = float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20);
                OUT.worldRadius = length(axisX) * 0.5;
                return OUT;
            }

            // 일반 알파 합성: dst 위에 src를 덮음 (둘 다 straight alpha)
            half4 Over(half4 dst, half4 src)
            {
                half a = src.a + dst.a * (1 - src.a);
                half3 rgb = (src.rgb * src.a + dst.rgb * dst.a * (1 - src.a)) / max(a, 1e-4);
                return half4(rgb, a);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float r    = length(IN.posOS) * 2.0;           // 0=중심, 1=테두리
                float dist = (1.0 - r) * IN.worldRadius;       // 테두리에서 안쪽으로 몇 m (밖은 음수)
                float aa   = max(fwidth(dist), 1e-5);          // 화면 픽셀 1개 폭 → 계단 방지

                float inside = smoothstep(-aa, aa, dist);

                // 1) 바탕: 옅은 빨강
                half4 col = half4(_InnerColor.rgb, _InnerColor.a * inside);

                // 2) 채움: 중심에서 _Fill 반지름까지
                float fillDist = (_Fill - r) * IN.worldRadius; // 채움 경계까지 m
                float fillMask = smoothstep(-aa, aa, fillDist) * inside * step(1e-3, _Fill);
                col = Over(col, half4(_FillColor.rgb, _FillColor.a * fillMask));

                // 3) 채움 앞선: 차오르는 중(0<Fill<1)일 때만 경계에 얇은 선
                float filling = step(1e-3, _Fill) * step(_Fill, 0.999);
                float front = (1.0 - smoothstep(_FrontWidth - aa, _FrontWidth + aa, abs(fillDist))) * inside * filling;
                col = Over(col, half4(_FrontColor.rgb, _FrontColor.a * front));

                // 4) 테두리 안쪽 글로우 (테두리에서 안쪽으로 지수 감쇠)
                float glow = exp(-max(dist, 0) / _GlowWidth) * _GlowStrength * inside;
                // 5) 외곽선 (테두리에서 안쪽으로 _OutlineWidth 만큼)
                float outline = (1.0 - smoothstep(_OutlineWidth - aa, _OutlineWidth + aa, dist)) * inside;
                col = Over(col, half4(_OutlineColor.rgb, _OutlineColor.a * max(outline, glow)));

                return col;
            }
            ENDHLSL
        }
    }
}
