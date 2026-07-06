# Feature: combat-battle-service — 헤드리스 결정론 전투 엔진

## 목표 / 검증 의도

던전 턴제 전투의 **로직 코어**를 UI·씬 왕복 없이 독립적으로 세운다. 전투는 시간/세이브/파티에 비의존이므로 먼저 헤드리스로 검증한다. 이번 슬라이스가 증명할 명제:

- **결정론**: 같은 시드·같은 입력이면 턴 순서·데미지·승패가 완전히 재현된다 → EditMode 테스트로 못 박는다.
- **결과 채널**: 전투 종료 결과가 Navigation URI가 아니라 `BattleService`를 통해 직전 모드로 1회 전달된다(ADR-0002 계약 실체화).
- **데이터 주도**: 전투원·스킬이 ScriptableObject로 저작되고 개시 시 런타임 POCO로 변환된다.

인플레이 전투 UI·명령 입력과 던전↔배틀 씬 왕복은 후속 feature `combat-turn-loop-ui`로 분리한다.

## 관련 AQ

- **AQ-8** (✅ ADR-0002) — BattleMode 종료 후 승/패/이탈 결과를 직전 스택에 전달하는 계약. 이번 feature가 그 계약을 코드로 구현한다.

## 범위

**In scope**
- 순수 C#(MonoBehaviour 무의존) 결정론 전투 엔진: 턴 순서 큐 · 기본공격 · 단일 스킬(데미지/힐) · HP 0 이탈 · 한쪽 전멸 시 win/lose.
- 주입식 시드 RNG, 순수 데미지 계산.
- 전투 데이터 SO: `SkillAsset` · `PersonaAsset` · `EnemyAsset` · `BattleEncounterAsset`.
- 결과 채널: `BattleService`(CoreServices 상주 `MonoService`) + 순수 `BattleOutcomeChannel`.
- 모드 배선: `BattleMode`가 종료 시 `SetOutcome`→`pop`, `ExplorationMode.OnResumedAsync`가 `ConsumeOutcome` pull·분기.
- EditMode 테스트.

**Out of scope (후속 슬라이스)**
- 전투 HUD·명령 UI·연출 (→ `combat-turn-loop-ui`).
- 던전 Portal→battle Zone→전투→pop 씬 왕복 플레이 검증.
- 약점(원소 affinity)·1 More/프레스턴·상태이상·캐릭터 특성(트레이트)·팀 액션포인트.
- `PartyService`/`SaveService`/`TimeService` 연동 (엔진은 전투원을 주입받아 이들에 비의존).

## 설계 개요

### 계층
```
Data(SO)          SkillAsset · PersonaAsset · EnemyAsset · BattleEncounterAsset
   │  CombatantFactory (SO→POCO 변환, 경계 1곳)
   ▼
Engine(순수)      BattleSetup → BattleEngine ├ TurnOrder ├ DamageCalculator ├ BattleRng
   │                                          └ Combatant · BattleAction · ActionResult
   ▼
Result            BattleOutcome ▶ BattleOutcomeChannel(순수) ◀ BattleService(MonoService)
   │
Wiring            BattleMode(SetOutcome→pop) · ExplorationMode.OnResumedAsync(ConsumeOutcome→분기)
```

### 엔진 공개 표면 (후속 UI가 그대로 구동)
- `BattleEngine(BattleSetup setup)` — 초기화.
- `BattleState State { get; }` — `Ongoing | PlayerWon | PlayerLost`.
- `Combatant Current { get; }` — 현재 행동자(없으면 종료).
- `ActionResult SubmitAction(BattleAction action)` — 결정론 적용 → 사망 판정 → 다음 행동자 전진 → 결과 반환.

헤드리스/테스트는 단순 결정론 정책(예: 첫 생존 적 타격)으로 `Current`가 빌 때까지 `SubmitAction`을 돌려 종료까지 구동한다.

### 결정론 규칙
- **턴 순서**(`TurnOrder`): Speed 내림차순, 동률은 전투원 `Id` 오름차순. 사망자 스킵 라운드로빈.
- **데미지**(`DamageCalculator.Compute`): attacker/defender 스탯 + power + 주입 `IBattleRng` 분산. RNG는 경량 LCG(`BattleRng`), 시드 주입.

### 데이터 (ScriptableObject)
- `SkillAsset` — `DisplayName` · `Kind`(Damage|Heal) · `Power` · `TargetSide`(Enemy|Ally|Self). 식별자는 `so.name`.
- `PersonaAsset` / `EnemyAsset` — `DisplayName` · `MaxHp` · `Attack` · `Speed` · `SkillAsset[] Skills`.
- `BattleEncounterAsset` — `PersonaAsset[] Party`(임시 직렬화 테스트 파티) · `EnemyAsset[] Enemies` · 기본 시드.

### 결과 채널 (ADR-0002)
- `BattleService : MonoService<BattleService>` — CoreServices 씬 상주. `SetOutcome`/`ConsumeOutcome`는 내부 `BattleOutcomeChannel`(순수)에 위임. `StartBattle(BattleSetup)→BattleEngine` 팩토리 편의. 슬라이스 임시로 `[SerializeField] BattleEncounterAsset _defaultEncounter` 보유.
- `BattleMode`: 조우→`BattleSetup` 구성→엔진 종료 구동→`BattleOutcome` 산출→`SetOutcome`→`NavigateAsync("gameplay://pop")`.
- `ExplorationMode.OnResumedAsync`: `ConsumeOutcome()` pull → `Lose`면 아지트 진입 URI `ReplaceAll`(게임오버), 그 외 탐색 계속.

## 작업 분해 (tasks)

`/feature plan`이 tasks.md에 채운다. 권장 순서:
1. `BattleRng` · `DamageCalculator` · `TurnOrder` (순수)
2. `Combatant` · `BattleAction` · `BattleSetup` · `ActionResult` · `BattleOutcome` · `BattleEngine`
3. SO 데이터(`SkillAsset`/`PersonaAsset`/`EnemyAsset`/`BattleEncounterAsset`) + `CombatantFactory`
4. `BattleOutcomeChannel` + `BattleService`(MonoService) + CoreServices 씬 배치
5. `BattleMode` / `ExplorationMode` 결과 채널 배선
6. EditMode 테스트
7. ADR-0004 작성

## 검증 방법

EditMode(`Assets/ZoneFlowAssets/Tests/Editor/Battle/`):
- `TurnOrderTests` — Speed 정렬·Id 타이브레이크·사망자 스킵 결정론.
- `DamageCalculatorTests` — 동일 시드→동일 데미지, 시드 변경 시 값 변화, 힐/데미지 부호.
- `BattleEngineTests` — 동일 시드 auto-resolve→동일 트랜스크립트, 우세 팀 승리, HP 0 큐 제외, 전멸 시 `PlayerWon`/`PlayerLost`, `BattleOutcome` 정확.
- `BattleOutcomeChannelTests` — `SetOutcome`→`ConsumeOutcome` 1회 pull 후 클리어(2회째 null).

컴파일 클린(`unity_get_compilation_errors`) + 전 케이스 green. 배선은 컴파일·리뷰로 확인(씬 왕복은 후속).
