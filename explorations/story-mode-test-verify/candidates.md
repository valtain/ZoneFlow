# 후보 방향들

## A — 기본 진입 테스트

**상태**: promoted

`gameplay://story/{zoneId}?id={spawnPointId}` URI로 StoryMode 진입 후 상태 검증.

- 재사용: `AssertMode<StoryMode>()`, `FindActiveZone()`
- 추가 검증: `ZoneId` 일치, `Stack=1`
- 난이도: 낮음 (기존 패턴 그대로 적용)
- 가치: 높음 — URI 파싱 → Director 라우팅 → StoryMode 생성 전 경로 최초 검증

---

## B — 스택 진입 + Pop 복귀

**상태**: promoted

`ExplorationMode` 위에 `StoryMode`를 stack으로 올린 뒤 Pop 복귀를 검증.
`story-mode-stack` exploration이 설계한 핵심 시나리오.

- 전이 순서: ExplorationMode(active) → StoryMode(stack, active) → Pop → ExplorationMode(resumed→active)
- 검증: Stack 크기 1→2→1, 최종 ActiveMode=ExplorationMode
- 난이도: 낮음 (Pop 내비게이션은 `gameplay://pop` URI로 이미 구현됨)
- 가치: 높음 — 스택 생명주기(Sleep/Resume) 전이가 StoryMode에서도 올바르게 동작하는지 검증

---

## C — 스폰 위치 정확도 검증

**상태**: eliminated

PlayerController의 월드 좌표가 SpawnPoint.SpawnTransform.position과 일치하는지 검증.

- 문제: Runtime 테스트에서 PlayerService.Instance 접근은 가능하나, PlayerController 생성이 씬 설정에 의존 → 테스트 불안정 가능성
- 대안: Editor 단위 테스트에서 GamePlayMode + FakePlayerService로 격리 테스트하는 것이 더 적합
- 폐기 이유: 이 exploration 범위(통합 테스트)와 맞지 않음. 별도 이슈로 분리 권장.

---

## D — ZoneAsset=null 방어 케이스

**상태**: eliminated

StoryMode를 ZoneAsset=null로 생성 시 SpawnPlayer()가 안전하게 무시되는지 검증.

- 코드에 이미 `if (Zone != null)` 체크 존재 (`GamePlayMode.SpawnPlayer()` L41)
- A 케이스에서 Zone 로드 성공 경로가 검증되므로 분기 커버 충분
- 폐기 이유: 별도 케이스 대비 추가 가치 낮음
