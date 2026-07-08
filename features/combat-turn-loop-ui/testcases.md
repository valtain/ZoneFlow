# combat-turn-loop-ui — 검증 시나리오

## TC-01: 인카운터 자산 배선
- `CoreServices.unity`의 `BattleService.DefaultEncounter`가 저작한 `BattleEncounterAsset`을 참조한다(≠ `fileID:0`).
- `BattleMode` 진입 시 DefaultEncounter null Assert·즉시 pop이 더 이상 발생하지 않는다.

## TC-02: 전투 패널 표시
- 던전 트리거 → 전투 진입 시 `BattlePanel`이 MainView에 슬라이드-인 한다.
- 파티/적의 이름·HP가 표시되고, 현재 행동자가 표기된다.
- 액션 버튼에 `기본공격` + Damage 스킬 라벨(DisplayName)이 나열된다.

## TC-03: 플레이어 턴 입력
- 플레이어 턴에 스킬 버튼 + 타겟을 선택하면 `AwaitPlayerActionAsync`가 `BattleAction`을 반환한다.
- 기본공격은 `skillPower:null`, 스킬은 해당 `SkillPowers[i]`로 `SubmitAction`에 전달된다.
- 선택 대상 HP가 결과량만큼 감소하고 패널에 반영된다.

## TC-04: 적 턴 auto-policy
- 적 턴은 입력 없이 기존 정책(첫 생존 상대 타격)으로 진행되고 결과가 연출된다.

## TC-05: 승리 왕복
- 적 전멸 → `PlayerWon` → `SetOutcome(Win)` → `pop`.
- 던전 `ExplorationMode`가 resume되고 탐색이 계속된다(village 강제 복귀 없음).

## TC-06: 패배 왕복
- 파티 전멸 → `PlayerLost` → `SetOutcome(Lose)` → `pop`.
- `OnResumedAsync`가 `ConsumeOutcome`로 Lose를 pull → `village?switch=replaceall`로 복귀.

## TC-07: 아레나 씬 왕복
- 전투 진입 시 `boss_room` 아레나 Zone이 StackAsync로 push(던전 언로드 없이 위에 적재)된다.
- pop 시 아레나가 정리되고 던전 Zone이 재활성화된다.

## TC-08: 엔진 회귀 (EditMode)
- 기존 battle EditMode 테스트가 `failed: 0`으로 유지된다(엔진·팩토리 무변경 확인).
