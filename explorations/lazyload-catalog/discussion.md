# 탐색 로그

- [2026-06-10 | start] lazyload-catalog 탐색 시작. PanelCatalog 직접 레퍼런스가 SO 역직렬화 시 prefab 전체를 메모리에 올리는 문제 탐색.

- [2026-06-10 | brainstorm] WebSearch + 병렬 Agent 탐색 결과 도출.
  **LazyLoadReference<T>**: Unity 내장 API, 동기 접근, MonoBehaviour에 이론적 적용 가능하나 asset 파일 대상 설계. 암묵적 변환(`ref = menuPanel`)으로 CatalogBaker 변경 최소화 가능. `.isSet` 호출은 로드 미트리거.
  **Addressables**: 프로젝트에 **미설치** 확인 (manifest.json 검증). 패키지 추가 + Baker 전면 수정 + TryGetPanel async 전환 필요.
  **현황**: PanelCatalog는 MenuPanel, ExplorationHudPanel, StoryHudPanel 2~3개 보유. TryGetPanel 호출부는 PanelMode(L25), ExplorationMode(L19), StoryMode(L19) 총 3곳.
  **핵심 제약**: LazyLoadReference<T>는 동기이므로 첫 패널 전환 시 IO 블록 가능성 있음. Addressables는 패키지 의존성 + 빌드 워크플로우 추가.

- [2026-06-10 | decision] Candidate A 채택. 차후 Addressables 이동을 전제로 LazyLoadReference<UiPanel>을 중간 단계로 도입. B(Addressables) 및 C(Status Quo) 폐기.

- [2026-06-10 | close] 탐색 완료.
