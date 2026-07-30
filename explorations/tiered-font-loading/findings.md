# 탐색 결과

**결론**: WebGL 초기 다운로드에 실리는 폰트 **~25.5M(실측: en 1.4 / ko 5.8 / ja 6.3 / zh 12M)** 을 **2단 폰트 로딩**으로 줄인다. 그룹이 Local이라 어떤 언어든 전량 초기 다운로드된다는 것이 실측으로 확증됐다(AQ-11).

**채택된 방향**: **Candidate C — 2단 폰트 로딩** (티어 기구 **1a**).
- **Tier-1 Boot** = **per-locale Static 서브셋**(Local, 부팅 UI 고정 글리프 = Intro·메뉴·피커 라벨·ASCII). 피커가 4스크립트 동시 표시 + Han-unification → 공유 아틀라스 불가, locale별 4개. TTF 제거로 즉시 ~22M 절감, 원격 호스팅 불요.
- **Tier-2 Content** = **Remote per-locale 전체 폰트**, 콘텐츠 진입(`ContentServices` 로드) 시 선택 locale만 로드·재적용.
- **기구 1a**: Asset Table 엔트리키를 티어 선택자(`"font-boot"`/`"font"`)로 승격 + `FontEngine.BootAsync(FontTier)` — 기존 FontRef·재부팅 seam(ADR-0007/0008)에 1:1. (1b 기각, 1c 비추천=`GlobalFallback` 오버로드.)
- **소유권 분할**: ContentServices 부트스트랩=트리거(언제) / AddressableService=원격 다운로드 / FontService=적용. content 티어 수명=ContentServices 씬 수명(AQ-4 첫 구체 사례).

**미결(feature 내 결정)**:
- **Tier-2 아틀라스 모드 — C(Dynamic 전체) vs E(String Table 베이크 Static)**. ZoneFlow 콘텐츠는 저작 내러티브(유한)라 E도 가능(TTF·FreeType 0), 단 콘텐츠 증가마다 재베이크(AQ-6 병목 패턴). 서브셋 베이크 정확값 측정 후 결정 권장.
- **권장 단계화**: Tier-1 먼저(원격 무의존, 즉시 절감·리스크 낮음) → Tier-2는 후속 phase.

**폐기된 방향**:
- **D(별도 Intro 폰트)** — Intro 태그라인이 지역화 CJK라 글리프 필요, 경량 폰트로 렌더 불가 → 절감 없음.
- **A(Static 전면)/B(Remote 전면)** 단독 — A는 콘텐츠 가변, B는 부팅 네트워크 의존으로 부적합. 각각 Tier-1/Tier-2 근간으로만 흡수.

**후속 Feature 후보**: `tiered-font-loading` — Tier-1(per-locale Static 서브셋 + 1a 기구 + boot-tier 카탈로그) 우선, Tier-2(Remote, C/E) 후속 phase. 첫 태스크 후보 = 서브셋 베이크 before/after 측정(spend 한도로 탐색 중 보류된 정확값).

**CLAUDE.md 반영 필요**: 조건부 — content 티어 원격 부재 시 "boot 티어=기능적 바닥(floor)" 불변식이 서면화되면 **ADR 후보**(원격 Addressables 도입 일반에 재발, 메모리 `zoneflow-remote-asset-floor-invariant`에 기록됨). AQ-4 인접.
