# Feature: combat-turn-loop-ui — 인터랙티브 턴제 전투 플레이

## 목표 / 검증 의도

`combat-battle-service`가 세운 **헤드리스 결정론 엔진** 위에 **플레이어가 실제로 조작하는 턴 루프**를 얹어, north-star 수직 슬라이스 `던전 → 턴제 전투 → 귀가`를 씬 왕복까지 실동작시킨다. 이번 슬라이스가 증명할 명제:

- **엔진 무변경 구동**: 엔진의 pull 표면(`State`/`Current`/`SubmitAction`/`ToOutcome`)만으로 UI 턴 루프가 성립한다 — 엔진 스키마를 바꾸지 않고도 인터랙티브 전투가 가능하다.
- **씬 왕복 계약**: 던전 → `gameplay://battle/boss_room?switch=stack` push → 전투 → `pop` → 직전 모드 resume·`ConsumeOutcome` 분기가 실제 플레이에서 동작한다(ADR-0002 계약의 씬 왕복 검증).
- **MainView 생명주기 재사용**: 전면 전투 UI가 HUD 패널과 동일한 `SetMainView`/`Show`/`Hide`/`Clear` 생명주기에 얹힌다.

## 관련 AQ

- **AQ-8** (✅ ADR-0002) — 이 slice가 결과 채널 계약을 **씬 왕복 실플레이**로 검증한다(`combat-battle-service`는 컴파일·리뷰까지만).

## 범위

**In scope**
- 전투 인카운터 콘텐츠 저작: `SkillAsset`/`PersonaAsset`/`EnemyAsset`/`BattleEncounterAsset` `.asset` + `BattleService.DefaultEncounter` 배선.
- 전면 전투 패널 `BattlePanel`(MainView): 파티/적 HP·이름, 현재 행동자, 액션 버튼(기본공격 + Damage 스킬), 타겟 선택, 결과 연출.
- `BattleMode` 인터랙티브 루프: 플레이어 턴은 패널 입력 await, 적 턴은 기존 auto-policy.
- 던전 전투 트리거 Interactable → 아레나(`boss_room` 재사용) push.
- 종단 PlayMode/수동 왕복 검증.

**Out of scope (후속/심화)**
- 팰리스 인플레이스 전투(아레나 없이) — combat-code.md 명시적 비-MVP.
- Heal/버프 스킬(팩토리가 Damage만 보존), 적 AI 고도화, 전투 보상 채널, 파티 다중 행동·예약.
- `Combatant`에 `DisplayName`/`SkillAsset` 내장(엔진 스키마 변경) — 뷰모델 우회로 회피.
- 약점/1More/상태이상/트레이트.

## 설계 개요

### 흐름
```
던전 Zone (ExplorationMode)
  └ 전투 트리거 Interactable ─▶ gameplay://battle/boss_room?switch=stack (StackAsync push)
BattleMode (아레나 = boss_room Zone)
  OnPlayedAsync : SetMainViewAsync(BattlePanel) + Initialize(뷰모델)
  OnModeInAsync : ShowMainViewAsync → 인터랙티브 루프 → SetOutcome → NavigateAsync(pop)
  OnModeOutAsync: HideMainViewAsync     OnStoppedAsync: ClearMainViewIfIs
  ▼ pop
ExplorationMode.OnResumedAsync : ConsumeOutcome → Win=탐색 계속 / Lose=village ReplaceAll
```

### 인터랙티브 루프 (BattleMode.OnModeInAsync, 현행 auto-policy while 교체)
```
while (engine.State == Ongoing)
    actor = engine.Current
    action = actor.Side == Player
             ? await _panel.AwaitPlayerActionAsync(actor, aliveTargets, ct)   // 버튼+타겟
             : BattleAction(Attack, actor.Id, FindFirstAliveOpponent.Id, null) // 기존 정책
    result = engine.SubmitAction(action)
    await _panel.PresentActionAsync(result, actor, target, ct)                 // 데미지·HP 연출
outcome = engine.ToOutcome() → service.SetOutcome(outcome) → NavigateAsync(pop)
```

### 표시명 뷰모델 (엔진 무변경 우회)
`Combatant`은 이름·스킬 참조가 없고 `int[] SkillPowers`만 갖는다. `CombatantFactory`는 party Id `0..n` → enemies 순으로 Id를 매기고 `Damage` 스킬만 순서 보존해 `SkillPowers`에 담는다. → `BattleMode`가 `encounter`에서 인덱스 zip으로 `Id → (DisplayName, Damage SkillAsset 라벨[])`을 파생해 `_panel.Initialize`에 전달. 스킬 버튼 i = i번째 Damage 스킬(라벨=DisplayName, power=`SkillPowers[i]`).

### BattlePanel 공개 표면 (UniTask 전용)
- `const string PanelId = "battle"`.
- `void Initialize(뷰모델, 초기 로스터)`.
- `UniTask<BattleAction> AwaitPlayerActionAsync(Combatant current, IReadOnlyList<Combatant> aliveTargets, CancellationToken ct)`.
- `UniTask PresentActionAsync(ActionResult result, Combatant actor, Combatant target, CancellationToken ct)`.
- 슬라이드/페이드는 `On*Async` 훅(PrimeTween), `#if UNITY_EDITOR [ContextMenu]` 빌더로 저작(ExplorationHudPanel 준용).

## 작업 분해 (tasks)

`/feature plan`이 tasks.md에 채운다. 권장 순서:
1. 인카운터 콘텐츠 저작 + `BattleService.DefaultEncounter` 배선.
2. `BattlePanel` UI + PanelCatalog 등록(BakeAll).
3. `BattleMode` 인터랙티브 루프 + 뷰모델 파생.
4. 던전 전투 트리거 Interactable + battle host→BattleMode 매핑 확인 + 카탈로그 bake.
5. 종단 PlayMode/수동 왕복 검증.

## 검증 방법

- 컴파일 0 에러(`unity_get_compilation_errors`).
- 자산 배선: `CoreServices.unity`의 `BattleService.DefaultEncounter` ≠ `fileID:0`, `PanelCatalog`에 `battle` 엔트리.
- PlayMode/수동 왕복: 던전 트리거 → 아레나 진입 → 플레이어 턴 입력→HP 감소 → 승/패 → pop → 복귀 분기. `unity_play_mode` + `unity_screenshot_game` 단계 캡처.
- EditMode 회귀: 기존 battle 테스트 `failed: 0` 유지(엔진 무변경 확인).
