# 탐색 로그

- [2026-07-21 | start] tmp-multilingual-font-engine 탐색 시작. 스톡 LiberationSans(라틴 전용) 하나뿐이라 CJK가 tofu로 뜨고, 두 rule이 런타임 텍스트를 영문으로 강제 중. 이 회피책을 걷어낼 폰트 엔진 탐색.

- [2026-07-21 | brainstorm] 병렬 Explore 2개 + WebSearch로 근거 수집.
  **코드베이스**: TMP = uGUI 2.0 내장본. Localization·Addressables 미설치. 18개 프리팹이 폰트 guid(LiberationSans) 하드코딩, 런타임 `.font` 변경 코드 없음. 서비스 = `MonoService<T>` CoreServices 배치(ADR-0001), 비동기 UniTask 전용. 선례 lazyload-catalog가 "Addressables는 seam으로 미루고 LazyLoadReference로 시작" 결정.
  **웹**: CJK는 동적 SDF 아틀라스가 정석. 한자는 지역 자형(SC/TC/JP/KR)이 달라 단일 fallback 체인은 혼입 위험 → locale별 폰트 분리 권장. 동적 폰트는 렌더 시 글리프를 자산에 직렬화(오염) → `ClearFontAssetData()`/"Clear Dynamic Data on Build"로 방지. Localization은 `LocalizedTmpFont` + Game View locale 스위처로 에디터 프리뷰 제공.

- [2026-07-21 | decision] 범위 = **폰트 엔진만**(런타임 문자열 계층은 후속 Unity Localization). 대상 = 한/영/일/중, Addressables 확장 가능(=seam).

- [2026-07-21 | decision] "스타일" = **TMP Style Sheet**. 폰트 모델 = 기본 폰트 + (스타일 × 언어) 지정. Locale = **시작 시 1회** 결정(런타임 swap 없음) → swap 인프라 불필요. CJK 폰트 파일 미확보 → Noto Sans CJK 전제 동적 SDF.

- [2026-07-21 | decision] 기존 자산 이관 = **전역 설정만**. TMP 전역 fallback에 활성 locale 폰트를 넣으면 하드코딩 프리팹도 fallback 경로로 CJK 렌더.

- [2026-07-21 | decision] 추가 요구: 에디터 locale 프리뷰(Unity Localization 활용, 지금 프리뷰용 도입) + **폰트 오염 방지**. 오염 = 동적 폰트가 프리뷰 글리프를 소스 .asset에 베이킹. 방지 = 저장/빌드 가드. 나아가 **저작 제어**로 Inspector에서 Style만 선택·폰트 필드 잠금 → 기본 폰트 외 serialize 금지.

- [2026-07-21 | decision] 기본 폰트를 **locale별로 지정**(글리프 커버리지엔 불필요하나 타이포 품질엔 필요 — 고정 라틴 기본 + CJK fallback은 한 문자열에 라틴/CJK 폰트 혼용 메트릭 불일치). 저장 가드가 컴포넌트 하드코딩 폰트를 **자동 스트립** → 모든 컴포넌트가 locale 기본 폰트 상속(기존 18개 프리팹 1회 diff 허용). → "전역 설정만/프리팹 무변경"을 "가드 자동 처리"로 대체.

- [2026-07-21 | decision] Candidate A 채택. B(단일 fallback 체인)·C(Localization Property Variants 주 메커니즘) 폐기. Localization은 프리뷰 도구로만 채택.

- [2026-07-21 | close] 탐색 완료.

- [2026-07-21 | explore] (close 후 요구 확정) 폰트/locale **선택 지점과 생애주기** 못박음: 선택은 **Intro 씬의 피커에서만** 가능하고, 1회 확정 후 불변 — 타 지점(설정 메뉴 등)에 선택 옵션 없음, Intro 재방문 시에도 이미 고른 폰트 변경 안 됨. 흐름: 첫 실행 1회 선택 → 영구 저장 → 이후 부팅마다 `FontService`가 그 값을 읽어 적용(미선택 시 기본 locale). 저장 위치는 세이브 슬롯 이전 **기기/프로파일 사전 설정**이므로 ISaveable(ADR-0003)이 아니라 **PlayerPrefs/설정 스토어** 권장 — 후속 feature에서 systems-designer 확정.
