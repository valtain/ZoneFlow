# 탐색 로그

- [2026-06-10 | start] story-mode-test-verify 탐색 시작. flexible-spawn 구현 완료 후 StoryMode 테스트 공백 확인.
- [2026-06-10 | brainstorm] 기존 테스트 파일(`GamePlayNavigationTests.cs`) 분석 완료. `AssertMode<T>()`, `FindActiveZone()` 패턴 재사용 가능. `ShellMode`, `PanelMode`, `ExplorationMode` 커버됨. `StoryMode` 케이스 전무. URI `gameplay://story/{zoneId}?id={spawnPointId}` 미검증 확인.
- [2026-06-10 | brainstorm] 후보 A(기본 진입): 기존 패턴 그대로 적용 가능. ZoneId만 있으면 즉시 작성 가능. 난이도 낮음, 가치 높음.
- [2026-06-10 | brainstorm] 후보 B(스택+Pop): story-mode-stack exploration이 설계한 핵심 시나리오. ExplorationMode sleep → StoryMode active → Pop → ExplorationMode resumed 전이가 검증 대상. 기존 테스트에 Pop 케이스 없음 → 추가 가치 있음.
- [2026-06-10 | brainstorm] 후보 C(스폰 위치): `PlayerService.Instance`가 Runtime 테스트에서 접근 가능하면 구현 가능. 그러나 PlayerController 생성 여부가 씬 데이터에 의존 → 검증 불안정 가능성. 분리된 Unit 테스트(Editor) 형태가 더 적합.
- [2026-06-10 | brainstorm] 후보 D(null 방어): `SpawnPlayer()` 내 `Zone != null` 체크는 이미 코드에 존재. 명시적 테스트로 회귀를 방지할 가치가 있으나 우선순위 낮음.
- [2026-06-10 | decision] A + B 우선 채택. C는 Editor 단위 테스트 별도 이슈로 분리. D는 A 케이스에서 자연스럽게 커버(Zone 로드 성공 시 SpawnPlayer 호출 검증).
- [2026-06-10 | close] 탐색 완료. findings.md 작성. BACKLOG.md 업데이트.
