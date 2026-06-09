# story-mode-stack — 탐색 질문

> StoryMode와 모드 스택에서 생명주기 전이가 설계 의도와 일치하는가?

## 컨텍스트

StoryMode는 `OnModeInAsync`만 구현한 단순한 구조로, 현재 SpawnPoint에 플레이어를 배치한다.
GamePlayDirector는 Replace / Stack / ReplaceAll / Pop 4가지 전환 방식으로 모드 스택을 관리한다.

유연한 이벤트 생성을 위해 SpawnPoint 외에 임의의 위치·자세로 플레이어를 배치하는 시나리오가 필요해졌다.
이를 포함해 5개 이슈의 설계 의도와 구현 정합성을 검증한다.

## 탐색 범위

- Pop → Resume 시 플레이어 Spawn 위치 처리 (현재 ModeInAsync에서 재배치)
- SpawnPoint 외 임의 위치 주입 구조 설계
- ShellMode 플레이어 배치 설계 의도 확인
- ReplaceAllAsync에서 비-Top 모드 상태 머신 순서 Skip 설계 의도 확인
- NavigationRequest.Parse 실패 처리
- Transition scope 예외 안전성 (낮은 우선순위)

Out of scope: ExplorationMode HUD 연출, TransitionFx 구현 세부사항

## 성공 기준

- 각 이슈의 설계 의도(버그 vs 의도) 명확히 분류
- 임의 위치 Spawn 구조의 설계 방향 결정 및 feature 승격
- NavigationRequest.Parse 로그 추가 처리 방안 확정
