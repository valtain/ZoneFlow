# ADR-0002: 전투 복귀 결과는 모드 간 결과 채널로 전달한다 (Navigation URI 파라미터 기각)

- **상태**: 채택
- **날짜**: 2026-07-01
- **관련 AQ**: AQ-8 (전투 복귀 계약)

## 맥락

BattleMode는 stack switch로 push되어 직전 모드(Exploration)가 Slept된다. 종료 시 Pop으로
Resume하는데, 승/패/이탈 결과를 직전 스택에 어떻게 전달하는가. 골격 확인 결과 StackAsync
(GamePlayDirector.cs 151-166)가 직전 Zone을 `SetActive(false)`하고 battle Zone을 스왑하며,
PopAsync(196-215)가 battle Zone Release + 직전 모드 ResumedAsync로 위치 재스폰까지 이미
수행한다. **유일한 결손은 "결과 전달 채널"이다.**

## 결정

**신설 `BattleService`가 `BattleOutcome`(win/lose/fled + 페이로드) POCO를 보관하는 모드 간
결과 채널을 채택한다.** BattleMode는 전투 종료 시 `BattleService.SetOutcome(...)` 후
`NavigateAsync(pop)`, 직전 모드의 `OnResumedAsync`가 `ConsumeOutcome()`로 1회 pull·소비한다.
이유 — 결과는 승/패 bool을 넘어 획득 아이템·소모 HP·도주 여부 등 구조화 페이로드로 자라며,
Navigation URI는 "어디로"를 표현하는 계층이지 "무슨 일이 있었는지"를 나르는 계층이 아니다.

**패배 경로**: 패배 시 직전 모드의 `OnResumedAsync`가 `BattleOutcome`을 pull해 아지트로
`ReplaceAll`(게임오버 복귀)한다. 승리 시 팰리스 복귀(Pop 그대로). **복귀 경로 분기 결정은
Resume된 모드가 outcome을 보고 수행**한다(BattleMode가 아님 — 전투는 결과만 보고, 목적지는
호출 맥락이 안다).

## 고려한 대안

| 대안 | 장점 | 단점 / 탈락 이유 |
| --- | --- | --- |
| C (채택) 모드 간 결과 채널(BattleService pull) | 타입 안전 페이로드, `OnResumedAsync` 시그니처 무변경, 미니게임/심문 등 재사용 | Service 1개 신설 |
| A `gameplay://pop?result=win` URI 파라미터 | 배선 단순해 보임 | pop 파서가 쿼리 폐기(NavigationRequest.cs 63-67), 파서·구조체·PopAsync 확장 강요, 관심사 분리 위반 |
| B BattleService 보관(전투 전용) | pull 모델 동일 | "결과 소유자"를 전투에 특화해 미래 서브모드 재사용 불가 → C로 일반화 |

## 결과

- **강제**: 신규 `BattleService`(CoreServices 상주) + `BattleOutcome` POCO. Resume된 모드가
  pull·소비(Director는 결과를 push하지 않음 — `OnResumedAsync`는 파라미터 없음, GamePlayMode.cs 143).
- **금지**: Navigation URI에 게임플레이 결과 페이로드 탑재.
- **MVP 계약**: 패배 → 아지트 ReplaceAll(게임오버), 승리 → 팰리스 Pop 복귀.
- 관련 결정: [ADR-0001](0001-sim-state-in-service-layer.md).
