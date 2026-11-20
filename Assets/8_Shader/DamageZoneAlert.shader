Shader "Custom/DamageZoneAlert"
{
    // 장판 원형 셰이더: 경고(빨강) → 산성(녹색, 부글부글) → 소멸.
    // 메시: 유니티 기본 Cylinder(오브젝트 공간 반지름 0.5)를 납작하게 눌러 쓴다고 가정.
    //       UV 대신 오브젝트 공간 xz 좌표로 원을 그리므로 Quad(XY 평면)에는 맞지 않음.
    // 두께/거리 계열 값은 "미터" 단위 → 장판 반지름이 바뀌어도 굵기 유지.
    //
    // 스크립트에서 움직일 값 (MaterialPropertyBlock):
    //   _Fill     0→1 : 경고가 중심에서 차오름 (비행 진행도)
    //   _Acid     0→1 : 산성이 중심에서 번짐 (착탄 후 짧게 0→1), 번진 곳의 경고는 사라짐
    //   _Dissolve 0→1 : 산성이 노이즈 모양으로 말라 사라짐 (수명 끝)
    Properties
    {
        [Header(Warning Outline)]
        _OutlineColor   ("Outline Color", Color)            = (1, 0.25, 0.1, 1)
        _OutlineWidth   ("Outline Width (m)", Range(0, 0.5)) = 0.06
        _GlowWidth      ("Inner Glow Width (m)", Range(0.001, 2)) = 0.35
        _GlowStrength   ("Inner Glow Strength", Range(0, 1)) = 0.5

        [Header(Warning Inner)]
        _InnerColor     ("Inner Color", Color)              = (1, 0.1, 0.05, 0.12)

        [Header(Warning Fill)]
        _FillColor      ("Fill Color", Color)               = (1, 0.15, 0.05, 0.3)
        _Fill           ("Fill (0=empty, 1=full)", Range(0, 1)) = 0
        _FrontColor     ("Fill Front Color", Color)         = (1, 0.4, 0.2, 0.8)
        _FrontWidth     ("Fill Front Width (m)", Range(0, 0.3)) = 0.04

        [Header(Acid State)]
        _Acid           ("Acid Spread (0=none, 1=full)", Range(0, 1)) = 0
        _Dissolve       ("Dissolve (0=full, 1=gone)", Range(0, 1)) = 0

        [Header(Acid Liquid)]
        _AcidDeep       ("Deep Color", Color)               = (0.05, 0.25, 0.02, 1)
        _AcidBright     ("Bright Color", Color)             = (0.35, 0.9, 0.1, 1)
        _AcidAlpha      ("Liquid Alpha", Range(0, 1))       = 0.75
        _AcidEmission   ("Emission Multiplier", Range(0, 4)) = 1.2
        _FlowScale      ("Flow Noise Scale (/m)", Range(0.1, 5)) = 1.2
        _FlowSpeed      ("Flow Speed", Range(0, 2))         = 0.25

        [Header(Acid Edge)]
        _EdgeNoise      ("Edge Breakup (m)", Range(0, 1))   = 0.35
        _EdgeNoiseScale ("Edge Noise Scale (/m)", Range(0.1, 5)) = 1.5
        _EdgeGlowColor  ("Edge Glow Color", Color)          = (0.6, 1, 0.2, 1)
        _EdgeGlowWidth  ("Edge Glow Width (m)", Range(0.001, 1)) = 0.12

        [Header(Acid Bubbles)]
        _BubbleColor    ("Bubble Color", Color)             = (0.7, 1, 0.35, 1)
        _BubbleScale    ("Bubble Cells (/m)", Range(0.5, 8)) = 2.5
        _BubbleDensity  ("Bubble Density", Range(0, 1))     = 0.55
        _BubbleSize     ("Bubble Max Size (cell)", Range(0.05, 0.5)) = 0.3
        _BubbleRim      ("Bubble Rim Width (cell)", Range(0.01, 0.2)) = 0.06
        _BubbleSpeed    ("Bubble Speed", Range(0, 3))       = 0.8
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
                float2 posWS       : TEXCOORD2; // 월드 xz (노이즈용: 원마다 무늬가 달라지도록)
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

                float  _Acid;
                float  _Dissolve;
                float4 _AcidDeep;
                float4 _AcidBright;
                float  _AcidAlpha;
                float  _AcidEmission;
                float  _FlowScale;
                float  _FlowSpeed;
                float  _EdgeNoise;
                float  _EdgeNoiseScale;
                float4 _EdgeGlowColor;
                float  _EdgeGlowWidth;
                float4 _BubbleColor;
                float  _BubbleScale;
                float  _BubbleDensity;
                float  _BubbleSize;
                float  _BubbleRim;
                float  _BubbleSpeed;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.posOS = IN.positionOS.xz;
                OUT.posWS = TransformObjectToWorld(IN.positionOS.xyz).xz;
                // 오브젝트 X축 스케일 × 0.5 = 월드 반지름 (xz 균일 스케일 가정)
                float3 axisX = float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20);
                OUT.worldRadius = length(axisX) * 0.5;
                return OUT;
            }

            // ---------- 유틸 ----------

            // 일반 알파 합성: dst 위에 src를 덮음 (둘 다 straight alpha)
            half4 Over(half4 dst, half4 src)
            {
                half a = src.a + dst.a * (1 - src.a);
                half3 rgb = (src.rgb * src.a + dst.rgb * dst.a * (1 - src.a)) / max(a, 1e-4);
                return half4(rgb, a);
            }

            // sin 없는 해시 (모바일 정밀도에서 sin 해시는 무늬가 깨짐)
            float Hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float2 Hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            // 값 노이즈 (0~1): 격자 꼭짓점 해시를 부드럽게 보간
            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = Hash12(i);
                float b = Hash12(i + float2(1, 0));
                float c = Hash12(i + float2(0, 1));
                float d = Hash12(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // 기포: 셀(보로노이식 격자)마다 기포 하나, 셀마다 다른 위상으로 커졌다가 터짐.
            // 반환값 0~1 = 기포 밝기 (테두리 강하게 + 안쪽 약하게)
            float Bubbles(float2 posWS)
            {
                float2 p   = posWS * _BubbleScale;    // 셀 단위 좌표
                float2 pi  = floor(p);
                float2 pf  = frac(p);
                float  aaB = max(fwidth(p.x), 1e-5);  // 셀 단위 픽셀 폭
                float  result = 0;

                // 기포가 이웃 셀로 넘어올 수 있으니 3x3 이웃을 봄
                [unroll] for (int y = -1; y <= 1; y++)
                [unroll] for (int x = -1; x <= 1; x++)
                {
                    float2 g    = float2(x, y);
                    float2 cell = pi + g;

                    float present = step(Hash12(cell + 41.7), _BubbleDensity); // 이 셀에 기포가 있나
                    float2 rnd    = Hash22(cell);
                    float  phase  = Hash12(cell + 17.3);

                    float2 center = g + 0.2 + rnd * 0.6;                 // 셀 안쪽 랜덤 위치
                    float  speed  = _BubbleSpeed * (0.7 + 0.6 * rnd.x);  // 셀마다 속도 다르게
                    float  life   = frac(_Time.y * speed + phase);       // 0→1 반복: 생겨서 커지고 터짐

                    float rr = _BubbleSize * smoothstep(0.0, 0.8, life); // 반지름: 커짐
                    float d  = length(center - pf);

                    float disk  = 1.0 - smoothstep(rr - aaB, rr + aaB, d);
                    float inner = 1.0 - smoothstep(rr - _BubbleRim - aaB, rr - _BubbleRim + aaB, d);
                    float ring  = saturate(disk - inner);
                    float pop   = 1.0 - smoothstep(0.85, 1.0, life);     // 끝에서 톡 사라짐

                    result = max(result, (ring + inner * 0.25) * pop * present);
                }
                return result;
            }

            // ---------- 프래그먼트 ----------

            half4 frag (Varyings IN) : SV_Target
            {
                float R    = IN.worldRadius;
                float r    = length(IN.posOS) * 2.0;           // 0=중심, 1=테두리
                float dist = (1.0 - r) * R;                    // 테두리에서 안쪽으로 몇 m (밖은 음수)
                float aa   = max(fwidth(dist), 1e-5);          // 화면 픽셀 1개 폭 → 계단 방지

                float inside = smoothstep(-aa, aa, dist);

                // ===== 경고 레이어 =====
                // 1) 바탕: 옅은 빨강
                half4 warn = half4(_InnerColor.rgb, _InnerColor.a * inside);

                // 2) 채움: 중심에서 _Fill 반지름까지
                float fillDist = (_Fill - r) * R;              // 채움 경계까지 m
                float fillMask = smoothstep(-aa, aa, fillDist) * inside * step(1e-3, _Fill);
                warn = Over(warn, half4(_FillColor.rgb, _FillColor.a * fillMask));

                // 3) 채움 앞선: 차오르는 중(0<Fill<1)일 때만 경계에 얇은 선
                float filling = step(1e-3, _Fill) * step(_Fill, 0.999);
                float front = (1.0 - smoothstep(_FrontWidth - aa, _FrontWidth + aa, abs(fillDist))) * inside * filling;
                warn = Over(warn, half4(_FrontColor.rgb, _FrontColor.a * front));

                // 4) 테두리 안쪽 글로우 + 5) 외곽선
                float glow    = exp(-max(dist, 0) / _GlowWidth) * _GlowStrength * inside;
                float outline = (1.0 - smoothstep(_OutlineWidth - aa, _OutlineWidth + aa, dist)) * inside;
                warn = Over(warn, half4(_OutlineColor.rgb, _OutlineColor.a * max(outline, glow)));

                // 산성이 0이면 여기서 끝 (경고 단계는 노이즈 계산 비용 없음)
                // 분기는 머티리얼 값 기준이라 한 장판 안에서는 모든 픽셀이 같은 쪽으로 감
                if (_Acid <= 0.0) return warn;

                // ===== 산성 레이어 =====
                float hasAcid = step(1e-3, _Acid);
                float fullAcid = step(0.999, _Acid);

                // 번짐: 중심에서 _Acid 반지름까지. 경고는 번진 만큼 지움(노이즈 없는 깔끔한 원 기준)
                float spreadClean = max(smoothstep(-aa, aa, (_Acid - r) * R), fullAcid) * hasAcid;
                warn.a *= 1.0 - spreadClean;

                // 웅덩이 가장자리: 원 안쪽으로만 노이즈만큼 파먹어 울퉁불퉁하게 (원 밖으로는 안 나감)
                float edgeN    = ValueNoise(IN.posWS * _EdgeNoiseScale + _Time.y * 0.15);
                float acidDist = dist - edgeN * _EdgeNoise;     // 웅덩이 경계에서 안쪽으로 m
                float puddle   = smoothstep(-aa, aa, acidDist);

                // 번지는 앞선도 노이즈로 흐트러뜨림 (다 번지면 무시)
                float spreadN = max(smoothstep(-aa, aa, (_Acid - r) * R + (edgeN - 0.5) * _EdgeNoise), fullAcid) * hasAcid;

                // 소멸: 노이즈 값이 _Dissolve보다 낮은 곳부터 사라짐 (0=전부, 1=없음)
                float dissN    = ValueNoise(IN.posWS * _EdgeNoiseScale * 1.7 + 7.1);
                float dissolve = smoothstep(_Dissolve * 1.1 - 0.1, _Dissolve * 1.1, dissN);

                float acidMask = puddle * spreadN * dissolve;

                // 액체 색: 노이즈 2겹을 서로 다른 방향으로 흘려 일렁임
                float2 flowP = IN.posWS * _FlowScale;
                float  flow  = ValueNoise(flowP + float2(_Time.y, _Time.y * 0.6) * _FlowSpeed) * 0.6
                             + ValueNoise(flowP * 2.3 - float2(_Time.y * 0.8, _Time.y) * _FlowSpeed) * 0.4;
                half3 liquid = lerp(_AcidDeep.rgb, _AcidBright.rgb, flow);

                // 웅덩이 가장자리 글로우
                float edgeGlow = exp(-max(acidDist, 0) / _EdgeGlowWidth);
                liquid = lerp(liquid, _EdgeGlowColor.rgb, edgeGlow * 0.8);

                // 기포
                float bub = Bubbles(IN.posWS) * (1.0 - edgeGlow); // 가장자리 글로우 위에는 기포 X
                liquid = lerp(liquid, _BubbleColor.rgb, saturate(bub));

                liquid *= _AcidEmission;   // 1 초과면 HDR → 블룸 켜진 렌더러에서 빛남

                half acidA = lerp(_AcidAlpha, 1.0, saturate(bub) * 0.6) * acidMask;
                return Over(warn, half4(liquid, acidA));
            }
            ENDHLSL
        }
    }
}
