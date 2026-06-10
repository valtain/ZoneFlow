# story-mode-test-verify — 탐색 질문

> StoryMode 테스트 커버리지가 전혀 없는 상황에서, 어떤 시나리오를 검증해야 하는가?

## 컨텍스트

`flexible-spawn` (#38–#41) 구현 완료 후 `StoryMode`는 동작 가능한 상태다:
- `OnModeInAsync` → `SpawnPlayer()` 호출
- `GamePlayMode.PlayedAsync()` → Zone 로드 + SpawnPoint 해석 + `_spawnPosition/_spawnRotation` 저장
- `GamePlayMode.SpawnPlayer()` → `Zone != null` 시 `PlayerService.SpawnAt(pos, rot)`

그러나 `GamePlayNavigationTests.cs`는 `ExplorationMode`, `ShellMode`, `PanelMode`만 커버하며,
`gameplay://story/{zoneId}?id={spawnPointId}` URI 경로는 **한 번도 테스트되지 않았다**.

`story-mode-stack` exploration(closed)이 이 설계를 다뤘으나, 테스트 케이스 정의까지는 진행되지 않았다.

## 탐색 범위

- StoryMode 기본 진입 경로 검증 (URI 파싱 → 모드 전환 → Zone 로드 → SpawnPlayer)
- 모드 스택 시나리오: ExplorationMode 위에 StoryMode 쌓기 + Pop 복귀
- 스폰 위치 정확도 검증 가능성 (PlayerService 접근성)
- 방어 케이스: ZoneAsset=null, SpawnPointId=null

Out of scope: StoryMode의 연출 확장(컷씬, 대화 등) — 현재 StoryMode는 빈 훅이므로 제외.

## 성공 기준

1. StoryMode 검증에 필요한 최소 테스트 케이스 목록이 정의됨
2. 각 케이스의 구현 가능성(PlayerService 접근, 씬 데이터 의존성) 평가 완료
3. promote 여부 및 방식(새 TASK vs. 기존 테스트 파일 확장) 결정됨
