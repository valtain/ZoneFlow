---
name: project_slice1_core
description: 슬라이스 1 순수 코어(#79·#80) 구현 완료 — 생성 파일·검증 결과·설계 결정
metadata:
  type: project
---

## 슬라이스 1 코어 구현 완료 (2026-07-03)

`Assets/ZoneFlowAssets/Runtime/GamePlay/Battle/` 아래 8개 파일 생성:
- `IBattleRng.cs` + `BattleRng.cs` — LCG(a=1664525, c=1013904223), 시드→uint 상태
- `BattleSide.cs` — Player / Enemy enum
- `Combatant.cs` — POCO, ApplyDamage/ApplyHeal, IsAlive=Hp>0, SkillPowers=IReadOnlyList<int>
- `TurnOrder.cs` — Speed내림차순·Id오름차순 타이브레이크, 사망자 스킵 라운드로빈
- `DamageCalculator.cs` — static Compute, VariancePercent=20%, 최소 1 보장
- `BattleAction.cs` + `ActionResult.cs` — readonly struct 값 객체
- `BattleSetup.cs` + `BattleOutcome.cs` — POCO, PartyService 무의존
- `BattleEngine.cs` — State/Current/SubmitAction/AllCombatants/ToOutcome

테스트 파일 (`Tests/Editor/Battle/`):
- `TurnOrderTests.cs`, `DamageCalculatorTests.cs`, `BattleEngineTests.cs`

**Why:** ADR-0004 — 헤드리스 결정론, EditMode 검증.

**How to apply:** 후속 슬라이스(#81~#83)는 이 표면(`BattleEngine.SubmitAction`) 위에 얹는다. BattleService·BattleMode 배선 시 `BattleSetup`을 생성자로 주입, `ToOutcome()`으로 채널 기록.
