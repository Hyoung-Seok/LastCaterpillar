# LastCaterpillar

> 어두운 도심에서 탱크로 기생체 변이 무리를 막아내며 탈출하는 톱다운 로그라이크

![Status](https://img.shields.io/badge/status-in%20development-orange)
![Unity](https://img.shields.io/badge/Unity-6000.3%20LTS-black?logo=unity)
![URP](https://img.shields.io/badge/URP-17.3-blue)
![Platform](https://img.shields.io/badge/platform-Android%20%7C%20PC-lightgrey)

---

# 프로젝트 정보

| 항목 | 내용 |
|---|---|
| 개발 기간 | 2026.06 ~ 진행 중 |
| 개발 인원 | 1인 |
| 장르 | 톱다운 / 로그라이크 / 슈터 |
| 플랫폼 | Android (주), Windows (보조) |
| 엔진 | Unity 6000.3 LTS |
| 렌더 파이프라인 | URP 17.3 (Mobile / PC 듀얼 렌더러) |
| 언어 | C# 9.0 / .NET Framework 4.7.1 |
| 목적 | 학습 + 포트폴리오 + 모바일 출시 사이클 경험 |

---

# 핵심 구현 포인트

- **시야 시스템**
  - URP Shader Graph Sub Graph 기반
  - 부채꼴(헤드라이트) + 원형(주변 인지) 이중 시야 마스킹 구현

- **대규모 적 처리**
  - Flow Field + Spatial Hash + Separation 기반 군집 이동/회피
  - 수백~수천 단위 적 처리 목표

- **절차적 도심 생성**
  - Grid + Perlin Noise + 모듈러 Prefab 기반 PCG
  - 생성 결과를 `LevelData ScriptableObject`로 출력

- **에디터 툴 (MapBuilder)**
  - PCG 결과를 수동 보정하는 Editor 기반 툴 제작
  - POCO 구조 + 멀티 렌더러 + HashSet 참조 Swap 최적화 적용

- **출시 사이클 경험**
  - 모바일 빌드 파이프라인 구축
  - Google AdMob SDK 연동
  - Google Play 스토어 등록 절차 진행 중

---

# 사용 패키지

| 패키지 | 용도 |
|---|---|
| `com.unity.render-pipelines.universal` | URP 렌더링 |
| `com.unity.inputsystem` | 모바일 / PC 통합 입력 |
| `com.unity.ai.navigation` | NavMesh 기반 이동 |
| `com.unity.timeline` | 시네마틱 |
| `com.unity.test-framework` | EditMode / PlayMode 테스트 |
| `Google AdMob SDK` | 광고 연동 |

---

# 스크린샷

> 개발 진행에 따라 추가 예정

| 시야 시스템 | PCG 도심 | 대규모 적 군집 |
|---|---|---|
| TBA | TBA | TBA |

---

# 개발 노트

https://www.notion.so/Last-Caterpillar-36c150d2d34380f7b342ec4b67aaca61?source=copy_link
