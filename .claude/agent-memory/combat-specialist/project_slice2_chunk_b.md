---
name: project-slice2-chunk-b
description: 청크 B(#81·#82·#83) 구현 완료 — SO 데이터·결과 채널·모드 배선·채널 테스트
metadata:
  type: project
---

## 완료 상태 (2026-07-03)

청크 B 구현 완료. 컴파일 오류 0, EditMode TC-07 6개 케이스 전부 passed.

### 생성 파일

| 경로 | 역할 |
|---|---|
| `Runtime/Data/Battle/SkillAsset.cs` | SO — SkillKind{Damage,Heal}, BattleTargetSide{Enemy,Ally,Self} |
| `Runtime/Data/Battle/PersonaAsset.cs` | SO — 파티 전투원 정의 |
| `Runtime/Data/Battle/EnemyAsset.cs` | SO — 적 전투원 정의 |
| `Runtime/Data/Battle/BattleEncounterAsset.cs` | SO — 조우 설정(Party+Enemies+Seed) |
| `Runtime/GamePlay/Battle/CombatantFactory.cs` | SO→POCO 변환 경계 1곳, 결정론 Id 부여 |
| `Runtime/GamePlay/Battle/BattleOutcomeChannel.cs` | 순수 POCO 채널, ConsumeOutcome 1회 pull |
| `Runtime/GamePlay/Battle/BattleService.cs` | MonoService<BattleService>, DefaultExecutionOrder(-1000) |
| `Tests/Editor/Battle/BattleOutcomeChannelTests.cs` | TC-07 6개 케이스 |

### 수정 파일

| 경로 | 변경 |
|---|---|
| `Runtime/GamePlay/ModeImpl/BattleMode.cs` | OnModeInAsync 배선: DefaultEncounter→CombatantFactory→BattleEngine→auto-policy→SetOutcome→pop |
| `Runtime/GamePlay/ModeImpl/ExplorationMode.cs` | OnResumedAsync 오버라이드: ConsumeOutcome pull → Lose면 ReplaceAll |

### CoreServices 씬 배치

`Assets/ZoneFlowAssets/Scenes/CoreServices.unity`에 `BattleService` GameObject(ZoneFlow.Battle.BattleService 컴포넌트) 배치·저장 완료.

### 허브 URI (ExplorationMode 선택 근거)

`"gameplay://exploration/village?switch=replaceall"` — GamePlayNavigationTests·포털 사용처에서 확인한 실제 허브 진입 URI.

### 후속 진입점

- `BattleService.DefaultEncounter` Inspector 할당 필요 (BattleEncounterAsset SO를 생성하고 배선)
- 카탈로그 베이크(`CatalogBaker.BakeAll`) 불필요 — 이번 슬라이스는 카탈로그 미사용
- 다음: `combat-turn-loop-ui` feature (전투 HUD·명령 UI·던전↔배틀 씬 왕복)

**Why:** 청크 A(순수 엔진) 위에 데이터 계층·결과 채널·모드 배선을 올려 ADR-0002 계약을 코드로 실체화.
**How to apply:** 후속 feature가 이 표면(BattleService.StartBattle, BattleMode, ExplorationMode.OnResumedAsync)을 그대로 사용.
