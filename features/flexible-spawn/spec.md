# flexible-spawn — Feature Spec

## 목표

SpawnPoint 기반 배치에 더해, 임의의 위치·자세(Vector3 + Quaternion)로 플레이어를 배치할 수 있는 구조를 제공한다.
유연한 이벤트 연출(NPC 대화 후 특정 위치에서 재개, 컷씬 종료 후 임의 위치 복귀 등)을 지원한다.

## 배경

현재 `StoryMode.OnModeInAsync`는 SpawnPoint에만 플레이어를 배치한다.
Pop→Resume 시에도 ModeInAsync가 재실행되므로, SpawnPoint로만 복귀한다.
이벤트나 컷씬 연출에서는 임의의 좌표·방향이 필요하다.

## 설계 방향 (결정 필요)

| 후보 | 설명 | 장점 | 단점 |
| --- | --- | --- | --- |
| A | `GamePlayMode`에 `SpawnOverride` 프로퍼티 추가 | 기존 구조 최소 변경 | 모드 외부에서 상태 주입, 순서 의존성 |
| B | `NavigationRequest`에 position/rotation 파라미터 추가 | URI 기반 완전한 선언적 지정 | URI 직렬화 복잡, 실수 여지 |
| C | `GamePlayDirector.NavigateAsync`에 SpawnConfig 오버로드 추가 | 호출부에서 명시적 지정, 기존 URI 호환 유지 | 오버로드 조합 증가 |

## Out of scope

- SpawnPoint 시스템 자체 변경
- 씬 로드 방식 변경

## 태스크

(결정 후 tasks.md에 기록)
