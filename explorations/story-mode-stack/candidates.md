# 후보 방향들

## Candidate 1: 임의 위치 Spawn 구조 (ModeInAsync 이전 주입)

**상태**: promoted

**문제**: Pop→Resume 시 ModeInAsync가 다시 SpawnPoint에 플레이어를 배치한다.
유연한 이벤트 생성을 위해 SpawnPoint 외 임의의 위치·자세로 배치해야 하는 시나리오가 필요하다.

**설계 방향 후보**:
- A) `GamePlayMode`에 `SpawnOverride` 프로퍼티 추가 — ModeInAsync 호출 전에 외부에서 설정
- B) `NavigationRequest`에 position/rotation 파라미터 추가 — URI 또는 별도 오버로드
- C) `GamePlayDirector.NavigateAsync`에 SpawnConfig 오버로드 추가

**사용자 의견**: ModeInAsync 이전에 Spawn 위치(구체적 위치·자세)가 지정되는 구조를 원함.

---

## Candidate 2: ShellMode 플레이어 배치

**상태**: eliminated

**문제**: `OnModeInAsync`가 아무것도 하지 않아 플레이어가 배치되지 않는다.

**결론**: **의도된 설계.** Shell은 커스텀 모드의 일종으로, Spawn 안 하는 것이 기본값.
필요 시 하위 구현에서 직접 처리한다.

---

## Candidate 3: ReplaceAllAsync Top 외 모드 상태 머신 Skip

**상태**: eliminated

**문제**: 스택의 비-Top 모드들이 Slept 상태에서 바로 Stopped → Destroyed로 전이한다.
ModeOut을 거치지 않아 상태 머신 순서가 건너뛰어짐.

**결론**: **의도된 설계.** Top 모드에서만 ModeOut → Stopped로 처리.
스택 내 모든 모드의 ModeOut을 실행하면 과도한 복잡도와 연출 문제가 생긴다.

---

## Candidate 4: NavigationRequest.Parse 무소음 실패

**상태**: active

**문제**: 파싱 실패 시 `default(NavigationRequest)` (= Exploration 모드)로 조용히 폴백.
잘못된 URI 디버깅이 어렵다.

**처리 방향**: 파싱 실패 시 `Debug.LogWarning` 또는 `Debug.Assert` 추가.
`/quick`으로 빠르게 처리 예정.

---

## Candidate 5: Transition scope 예외 안전성 (Timeout)

**상태**: active (보류)

**문제**: `PlayedAsync`에서 예외 발생 시 `TransitionFxScope`가 Dispose되지 않아
검은 화면이 유지될 수 있다.

**처리 방향**: Timeout 지정 기능이 있으면 좋지만 급하지 않음. 추후 검토.
