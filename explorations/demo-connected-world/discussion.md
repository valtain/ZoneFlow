# 탐색 로그

- [2026-06-19 | start] demo-mvp 씬 구조 개선 exploration 시작. 사용자 핵심 동기: World1/World2의
  village↔dungeon(및 그 외) 연결이 "단절"되어 있고, 더 넓고 시나리오에 적합한 환경을 원함. 씬
  리네임 허용.

- [2026-06-19 | brainstorm] 현 구조 분석. `SetupDemoMvp()`가 World1=village, World2=dungeon Zone을
  원시 박스로 추가. 연결은 분리 씬 간 순간이동 Portal 하나뿐. `ZoneRegistry.ReleaseAsync`는 씬 내
  모든 Zone RefCount=0일 때만 씬 언로드 → 별도 씬 유지가 명제 A의 "전환 관찰력"을 보장한다는 제약
  확인. 후보 C1~C4 도출.

- [2026-06-19 | decision] C2 vs C3 상세 비교 후 **C3 → C2 단계결합** 확정.
  - 공통: 둘 다 씬 단위 실제 load/unload 유지 → 명제 A 증명력 동등.
  - 차이: C2는 **구조적**(허브로 연결 조직 추가), C3는 **연출·비주얼**(순간이동을 페이드로 포장 +
    에셋 스케일로 확장). C2는 작업량 最多·후속 Zone 증설 우아, C3는 저비용·저위험·체감 직격.
  - 결정: 1단계 C3(리네임+페이드+리테마)로 즉시 개선, 2단계 C2 허브는 demo-boss에서 증설. 두 후보는
    배타적이지 않고 단계적으로 결합 가능.
  - C1은 같은 씬 Zone 전환이 SetActive 토글뿐이라 보류, C4는 항상-로드로 명제 A 충돌이라 폐기.

- [2026-06-19 | brainstorm] 레거시·부트 의존성 코드 확인.
  - `intro` ZoneId는 부트 플로우 진입점(Splash→Intro 씬→IntroScreen→menu→
    `MenuPanel.NewGameUri = gameplay://exploration/village`) → **보존 필수**, demo 미사용 cruft 아님.
  - 레거시 ZoneId(`world1`,`world1_b`,`story_w1`,`world2`,`world2_b`)는 ZoneAssetCatalog +
    SpawnPointCatalog + InteractableCatalog 3곳에 baked 상태. 카탈로그는 `CatalogBaker`가 씬에서
    생성하므로, 정리는 **.asset 직접 편집이 아니라 씬에서 레거시 Zone 루트 제거 후 re-bake**.
  - `SceneSetupTool`은 `"World1"/"World2"` 문자열·메뉴 항목을 하드코딩 → 리네임 시 함께 갱신 대상.

- [2026-06-19 | close] 탐색 완료. C3→C2 단계결합 채택, findings.md 작성.
