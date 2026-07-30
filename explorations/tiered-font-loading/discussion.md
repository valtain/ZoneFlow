# 탐색 로그

- [2026-07-29 | start] #107(런타임 언어 스위처) 마무리 중 사용자가 폰트 용량 우려 제기 — "Intro 폰트는 locale 폰트와 달라야 하지 않나, CJK가 커서 전부 포함하면 Addressables 용량 큼, WebGL이라 필요 없나?" 파생.

- [2026-07-29 | explore] 실측: 폰트 = Dynamic 아틀라스(m_AtlasPopulationMode:1) + ClearDynamicDataOnBuild:1 → 빌드 아틀라스 비고, 런타임 TTF 렌더. 소스 TTF: NotoSans 2.0M / JP 9.2M / KR 10M / SC 17M = ~38M. Addressables Localization-Assets-* = Local.BuildPath/LoadPath. → WebGL은 디스크 지연로딩 없어 전 locale 폰트 ~38M이 초기 .data에 실림. "웹빌드라 불필요"가 아니라 오히려 더 중요.

- [2026-07-29 | brainstorm] 후보 도출: A(Static 서브셋 전면) / B(Remote per-locale 전면) / C(2단 하이브리드) / D(별도 Intro 폰트, 폐기) / baseline(현행). D 폐기 이유: Intro 태그라인이 지역화 CJK라 글리프 필요 → 경량 폰트 불가.

- [2026-07-29 | decision] 사용자: 콘텐츠 문자열은 무한이라 전면 서브셋(A) 불가 → 콘텐츠는 Remote 전체(B), Intro/부팅 UI는 고정이라 Static 서브셋(A). 즉 **Candidate C(2단) 채택 방향**. Tier-2 로드 seam 후보 = ContentServices 경계. 미결정: Tier-2 원격 호스팅 부담, 메뉴 계층(→ Tier-1 권장), Han-unification.

- [2026-07-29 | external] WebSearch 검증: (1) **Remote Addressables는 WebGL 동작** — remote LoadPath 온디맨드 로드로 초기 다운로드 축소·지역별 전달 지원. 단 WebGL 번들은 워커스레드 부재로 **LZ4/무압축 권장**, 호스트 **CORS** 필요. (2) **Static 폰트 에셋은 소스 TTF를 빌드에 미포함**(Dynamic만 포함해 빌드 증가) → Tier-1 Static 서브셋이 ~38MB TTF 제거를 확정. Custom Range/Characters로 서브셋(대개 게임은 ~1000자). "base + 추가 폰트 다중 에셋을 TMP가 필요 시 로드" = fallback/2단 뒷받침. → 후보 A·B·C 전제 모두 검증됨(평가 유지).

- [2026-07-29 | explore] **seam 검증 — 티어 분리 기구 (task 1) → 1a 추천.** 코드 접합부 확인: `AddressablesFontProvider.LoadAsync(localeCode)`는 `"Fonts"` Asset Table에서 고정 `EntryKey="font"` 하나를 해석해 `FontSet`(default+fallback+stylesheet+presets)을 반환하는 **stateless** 프로바이더다. `FontEngine.BootAsync` = `GetActiveLocaleCode → LoadAsync → facade.Apply`. `FontService.SelectLocaleAsync`가 이미 **재부팅=재적용 채널**(set locale → persist → BootAsync)이다. 세 후보 평가:
  - **1a (phase별 두 FontRef, 재부팅으로 재적용)** — `EntryKey`를 티어 선택자로 승격(`"font-boot"` = Local Static 서브셋 / `"font"` = Remote Dynamic 전체)하고, `FontEngine.BootAsync(FontTier)` + `provider.LoadAsync(locale, tier)`로 티어 파라미터를 흘린다. 기존 구조에 1:1 매핑: FontRef=폰트 세트, Asset Table 엔트리=선택자, SelectLocaleAsync/BootAsync=재적용. 프로바이더는 계속 stateless, ADR-0006(폰트=Localization Asset Table) 결정과 정합. **트레이드오프**: 콘텐츠 경계에서 default 폰트를 서브셋→전체로 바꾸는 것은 TMP_Settings 전면 재적용이라 모든 표시 텍스트가 재레이아웃된다 — 하지만 그 시점은 씬 전환(ContentServices 로드)과 겹쳐 비용이 숨는다.
  - **1b (단일 프로바이더 async 업그레이드)** — 프로바이더가 boot 폰트 먼저 반환 후 내부에서 content로 재적용. `LoadAsync`의 "하나의 FontSet 반환" 계약을 깨고 프로바이더에 상태·2차 Apply를 넣어야 한다. Apply는 facade 책임이고 재부팅이 이미 그 일을 한다 → **재부팅 채널을 상태만 추가해 재발명**. 기각.
  - **1c (TMP fallback 체인, 재부팅 회피)** — boot 서브셋=default, 콘텐츠 경계에서 원격 전체 폰트를 로드해 fallback으로 추가. 재적용 churn은 없다. 그러나 `FontSet.GlobalFallback`은 코드 주석상 "없는 글리프 보강(이모지/기호 등)" 채널이고, 피커의 `ApplyPickerFallbacks`와 `SelectLocaleAsync`의 per-locale fallback이 **이미 같은 리스트를 경합**한다. 여기에 "티어-2 콘텐츠 폰트"를 얹으면 fallback의 의미가 3중 오버로드된다(보강 vs 피커 vs 티어). 채널 오염이 1c의 실제 비용 → 비추천(후보로는 유지, candidates.md에 명시).
  - **결론**: 1a. 이유 — 티어를 이미 존재하는 구조(FontRef·엔트리키·재부팅)에 얹을 뿐 새 개념·새 상태가 없고, `GlobalFallback`의 "글리프 보강" 의미를 깨끗이 유지한다.

- [2026-07-29 | explore] **피커가 Tier-1 구조를 강제한다 (task 3 핵심 리스크).** `IntroScreen.ShowLanguagePickerAsync`는 4개 네이티브명(English/한국어/日本語/简体中文)을 **동시** 표시하고, 이를 위해 `FontService.ApplyPickerFallbacks()`가 `Catalog.AllFonts()`(현재는 전체 Dynamic 폰트 4개)를 전역 fallback으로 깐다. 함의: **Tier-1 boot는 단일 공유 Static 아틀라스일 수 없다.** (a) 한 아틀라스는 한 소스 폰트로만 베이크되므로 4개 스크립트를 한 폰트로 담으면 Han-unification 오류(简/日 자형이 한 리전 글리프로 렌더). (b) 피커는 4스크립트 동시 표시라 per-glyph 자형 정확성이 필요. → **Tier-1 = per-locale Static 서브셋 4개**(각각 자기 로케일 라벨+ASCII+메뉴 고정 글리프, 각자 소스 폰트로 베이크). 피커는 오늘의 `ApplyPickerFallbacks` 패턴을 **그대로 재사용**하되 `Catalog.AllFonts()`가 전체 폰트 대신 **boot-tier 서브셋 카탈로그**를 가리키게 한다 → ~38M TTF 제거 + per-script 자형 정확. 선택 후(메뉴)엔 선택 로케일 서브셋 하나만 default. 즉 Tier-1은 "boot-tier FontCatalog(per-locale 서브셋 FontRef)"라는 **병렬 카탈로그**를 요구한다 — 1a의 `"font-boot"` 엔트리가 이 카탈로그를 가리킨다.

- [2026-07-29 | explore] **전환 트리거 소유권 (task 2).** `MenuPanel.OnNewGame → SceneService.EnsureContentServicesLoaded`는 **트리거 지점으로 옳다**(콘텐츠 세션 수명 경계 = 가변 콘텐츠 텍스트가 등장하는 유일 구간). 단 **MenuPanel이 직접 폰트 API를 호출하면 안 된다**(패널은 얇게 유지). 소유권 3분할 권장: **① ContentServices 씬 부트스트랩 = "언제"(트리거)** — `EnsureContentServicesLoaded`가 로드하는 씬의 부트스트랩이 현재 로케일 content 티어 로드를 발주(씬 생명주기에 묶여 `UnloadContentServicesAsync`와 대칭). **② AddressableService = "원격 다운로드"** — 이 서비스 doc 주석이 이미 "원격 콘텐츠 도입 시 카탈로그 업데이트·프리로드·다운로드도 이 서비스가 소유할 자리"라 명시 → 원격 번들 전송은 여기. **③ FontService = "적용"** — 재부팅 seam으로 default를 서브셋→전체 swap. **Zone-Mode/AQ-4 경계**: content 티어의 수명 = ContentServices 씬 수명. 언로드(메뉴 복귀) 시 content 폰트 릴리스 + boot 서브셋 복원 → boot 서브셋=CoreServices 수명(상시), content 전체=ContentServices 수명(세션)의 **대칭 생명주기**. 이는 AQ-4("Addressable 전환 시 Zone 생명주기 인터페이스 변화")의 구체 사례 하나를 만든다 — content 티어 로드/릴리스가 씬 생명주기 훅에 얹히는 첫 원격 에셋 사례.

- [2026-07-29 | explore] **리스크 & 미해결 질문 (task 3).** ① **피커 Han-unification** — 위 항목에서 per-locale 서브셋으로 해소(구조 결정 사항). ② **Tier-2 다운로드 실패(오프라인/CORS)** — 실패 시 boot 서브셋엔 콘텐츠 글리프가 없어 **tofu**. 현재 코드는 `Debug.Assert(fontRef != null)`로 원격 실패 경로를 모델링하지 않음. **새 계약 필요**: content 티어 로드 실패 시 boot 폰트 적용을 유지하고 오류를 표면화(TMP를 깨진 상태로 두지 않음) — "**boot 티어가 content 부재 시 기능적 바닥(floor)**"이라는 불변식. 이건 새 아키텍처 질문 후보(AQ-11에서 파생, AQ-4 인접): *원격 에셋 부재 시 Zone/세션이 저하 모드로 진입하는 규약이 있는가.* ③ **WebGL Dynamic FreeType 비용** — Tier-2가 Dynamic이면 CJK 글리프 최초 렌더가 메인스레드 래스터화(워커 부재) + LZ4/무압축이라 다운로드도 큼 → 콘텐츠 CJK 대량 표시 시 hitch. 완화: 흔한 글리프 프리워밍 or 콘텐츠가 유한하면 Static content 아틀라스(→ Candidate E). ④ **유저 입력 텍스트(세이브명 등)** — 있으면 Static content 아틀라스로 못 덮음, 해당 필드만 Dynamic fallback 필요(소규모).

- [2026-07-29 | explore] **미발견 후보 E 추가 — "콘텐츠는 무한" 전제 재검토.** Candidate A(전면 Static)를 기각한 근거는 "콘텐츠 문자열 무한". 그러나 ZoneFlow 콘텐츠는 **저작된 내러티브/대사(String Table)** — 유저 생성이 아니라 **빌드 시점에 알 수 있는 유한 집합**이다. 진짜 무한은 유저 입력(이름/채팅)뿐. → content 글리프를 **String Table 스캔으로 베이크**하면 Dynamic+TTF 없이 Static content 아틀라스(Remote per-locale)가 가능. TTF 0 + 런타임 FreeType 0. 트레이드오프: 콘텐츠 증가마다 재베이크 — 이는 **[[zoneflow-aq6-catalog-bake-scale]](AQ-6) 전량 재스캔 병목**과 동일 패턴(폰트 아틀라스 베이크가 저작 루프에 얹힘). candidates.md에 Candidate E로 추가.

- [2026-07-29 | explore] **최소 측정 프로토타입 (task 4) — atlas-mode 변수만 격리.** 목표 숫자 = "한 로케일 boot 폰트를 Static 서브셋으로 바꿀 때 초기 `.data`에서 빠지는 MB". Local/Remote 변수는 섞지 않는다(별도 2차 측정). 후속 구현 이슈가 그대로 실행할 스텝:
  1. **베이스라인 빌드** — 현행 WebGL 빌드, `Build/*.data` 크기 + 총 다운로드 기록(4 TTF ~38M 포함).
  2. **글리프 집합 추출(ko)** — `Localization-String-Tables-ko` 전 엔트리 + 피커 라벨(`English 한국어 日本語 简体中文`) + ASCII + 메뉴 문자열의 유니크 문자를 char 리스트 파일로.
  3. **Static 서브셋 폰트 생성(KR)** — NotoSansKR에서 `m_AtlasPopulationMode:0`(Static) + Custom Characters=위 리스트로 TMP_FontAsset 베이크. 소스 TTF 미포함 확인(WebSearch 검증대로 Static은 TTF 제외).
  4. **FontRef 스왑** — `FontRef_ko` 복제 → `DefaultFont`를 서브셋 에셋으로. `"Fonts"` 테이블 ko 엔트리(또는 boot 엔트리)가 이를 가리키게. KR 델타 격리를 위해 다른 로케일은 그대로(또는 임시 제외).
  5. **WebGL 재빌드** — `.data` 크기 기록.
  6. **델타 산출** = 베이스라인 `.data` − 서브셋 `.data` ≈ NotoSansKR TTF(~10M) 제거분 − 추가된 소형 Static 아틀라스 텍스처. 기대: KR 단독 ~9–10M 절감, 4로케일 외삽 ~38M.
  7. **렌더 검증(선택)** — WebGL 빌드에서 피커+메뉴가 KR 경로로 tofu 없이 렌더되는지 확인.
  → 이 프로토타입은 **Tier-1 전제(Static 서브셋이 TTF를 제거)만** 격리 검증한다. Remote 전제(초기 `.data`가 전체 폰트를 제외)는 별도 2차 측정(전체 폰트를 Remote 그룹에 두고 초기 `.data` 미포함 확인)으로 분리.

- [2026-07-30 | measure] **실측(빌드 0, 기존 Addressables 출력 측정).** `Library/com.unity.addressables/aa/WebGL/WebGL/`의 기존 번들에서 locale별 폰트 번들 압축 크기 확인: **en 1.4M / ko 5.8M / ja 6.3M / zh-Hans 12M = 합계 ~25.5M**. 원시 TTF ~38M이 LZ4 압축 후 ~25.5M로 실제 빌드에 실림(그룹이 Local이라 전량 초기 다운로드). ko 5.8M ≈ NotoSansKR.ttf(10M) 압축분 → 번들이 Dynamic 폰트+소스 TTF를 담는 것 확증. **AQ-11 우려가 수치로 확증됨**: 어떤 언어를 쓰든 ~25.5M 폰트가 초기 `.data`에 포함. 남은 정확값(서브셋 아틀라스 크기)은 서브셋 베이크가 필요 — 측정 서브에이전트가 monthly spend limit로 중단돼 before/after 완전 diff는 보류. 추정: Tier-1 Static 서브셋(<1M/locale, TTF 제거) → ~22M 절감; Tier-2 Remote → 초기 다운로드에서 전량 제외, 선택 locale만 온디맨드.

- [2026-07-30 | close] 탐색 완료. Candidate C(2단, 기구 1a) 채택 — Tier-1 per-locale Static 서브셋 우선(즉시 ~22M 절감·원격 무의존), Tier-2 Remote(C/E 미결)는 후속 phase. findings.md 작성. feature `tiered-font-loading`로 승격.
