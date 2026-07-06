# combat-battle-service — 검증 시나리오

## TC-01: 턴 순서 결정론 (TurnOrder)
- Speed 내림차순으로 행동 순서가 정해진다.
- Speed 동률이면 전투원 Id 오름차순으로 타이브레이크된다.
- 동일 전투원 집합·동일 입력이면 순서가 완전히 재현된다.

## TC-02: 사망자 스킵
- HP 0 전투원은 턴 순서에서 제외된다.
- 라운드 진행 중 사망하면 이후 그 전투원의 턴이 오지 않는다.

## TC-03: 데미지 결정론 (DamageCalculator)
- 동일 시드·동일 (attacker/defender/power) → 동일 데미지.
- 시드를 바꾸면 데미지 분산이 달라진다.
- Heal 스킬은 대상 HP를 회복시키고 MaxHp를 넘지 않는다.

## TC-04: 기본공격·스킬 적용 (BattleEngine.SubmitAction)
- 기본공격이 대상 HP를 예상량만큼 감소시킨다.
- Damage 스킬이 Power에 비례한 데미지를 준다.
- 대상 HP가 0이 되면 `ActionResult`에 사망이 표기되고 대상이 이탈한다.

## TC-05: 승패 판정
- 적 팀 전멸 시 `State == PlayerWon`.
- 파티 전멸 시 `State == PlayerLost`.
- 종료 후 `Current == null`.

## TC-06: auto-resolve 트랜스크립트 재현
- 동일 시드로 엔진을 2회 구동하면 HP·이벤트 트랜스크립트가 완전히 일치한다.
- 우세 스탯 팀이 승리한다.

## TC-07: 결과 채널 1회 pull (BattleOutcomeChannel)
- `SetOutcome(x)` 후 `ConsumeOutcome()`가 x를 반환한다.
- 2회째 `ConsumeOutcome()`는 null(빈 채널)을 반환한다.

## TC-08: 모드 배선 (컴파일·리뷰)
- BattleMode 종료 시 `BattleService.SetOutcome(...)` 후 `gameplay://pop` 네비게이션.
- ExplorationMode `OnResumedAsync`가 `ConsumeOutcome()`를 pull하고 `Lose`면 아지트 `ReplaceAll`, 그 외 탐색 계속.
- (씬 왕복 플레이 검증은 후속 `combat-turn-loop-ui`.)
