# combat-battle-service — Tasks

부모 에픽: #78 · 마일스톤: M8 · Combat

| # | 태스크 | 상태 |
| --- | --- | --- |
| 1 | 순수 결정론 프리미티브: `BattleRng`(주입 시드 LCG) · `DamageCalculator`(순수 static) · `TurnOrder`(Speed 정렬·Id 타이브레이크·사망자 스킵) | #79 ✅ |
| 2 | 엔진 코어: `Combatant`·`BattleAction`·`BattleSetup`·`ActionResult`·`BattleOutcome` + `BattleEngine`(State/Current/SubmitAction) | #80 ✅ |
| 3 | 전투 데이터 SO: `SkillAsset`·`PersonaAsset`·`EnemyAsset`·`BattleEncounterAsset` + `CombatantFactory`(SO→POCO) | #81 ✅ |
| 4 | 결과 채널: 순수 `BattleOutcomeChannel` + `BattleService`(MonoService) + CoreServices 씬에 GameObject 배치 | #82 ✅ |
| 5 | 모드 배선: `BattleMode`(조우→구동→SetOutcome→pop) · `ExplorationMode.OnResumedAsync`(ConsumeOutcome→분기) | #83 ✅ |
| 6 | EditMode 테스트: `TurnOrderTests`·`DamageCalculatorTests`·`BattleEngineTests`·`BattleOutcomeChannelTests` | #84 ✅ |
| 7 | ADR-0004 반영 확인 및 feature 문서 정합성 점검 | #85 ✅ |

## 검증 결과 (2026-07-03)

- 컴파일: 에러 0.
- EditMode(Test Runner): TurnOrder·DamageCalculator·BattleEngine·BattleOutcomeChannel 전부 실행, `failed: 0`.
- `BattleService`: CoreServices.unity에 GameObject+컴포넌트 실제 배치·저장 확인.

## 후속 (이번 슬라이스 밖)

- **`BattleEncounterAsset` `.asset` 저작 + `BattleService.DefaultEncounter` 배선** — 미저작 시 `BattleMode`가 `Debug.Assert` 후 즉시 pop(설계상 안전). 씬 왕복 실제 플레이는 `combat-turn-loop-ui`에서.
- **패배 복귀 목적지** — 현재 허브인 `village`로 `ReplaceAll`. P5 아지트가 생기면 그 진입 URI로 교체(ADR-0002의 "아지트"는 향후 허브를 지칭).
