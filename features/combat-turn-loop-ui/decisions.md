# combat-turn-loop-ui — 설계 결정

Source: [persona5-slice](../../explorations/persona5-slice/findings.md)

## 고유 구현 결정

| 결정 | 이유 |
| --- | --- |
| 엔진(`BattleEngine`/`Combatant`)을 건드리지 않고 UI 턴 루프를 구동 | 엔진은 pull 기반(`State`/`Current`/`SubmitAction`/`ToOutcome`)이라 충분. combat-battle-service의 "후속 UI가 그대로 구동" 명제를 무변경으로 실증(수술적 변경) |
| 표시명·스킬 라벨은 `BattleMode`가 `encounter`에서 파생한 뷰모델로 전달 | `Combatant`에 `DisplayName`/`SkillAsset`을 넣으면 엔진 스키마가 오염됨. 팩토리 Id 규칙(party 0..n→enemies, Damage 스킬 순서 보존)을 인덱스 zip으로 역산해 UI에만 이름을 준다 |
| 전투 아레나로 기존 `boss_room` Zone 재사용 | combat-code.md "아레나=별도 Zone, StackAsync push/Pop"를 신규 씬 저작 없이 충족. boss_room은 overworld-hub에서 무대만 생성돼 있어 그대로 전투 무대로 전용 |
| 전면 전투 UI는 MainView 레이어에 배치 | HUD 패널과 동일한 Set/Show/Hide/Clear 생명주기 재사용. Overlay(셸·PanelMode)·Floating(프롬프트)·Popup(모달)과 관심사 분리 |
| 플레이어 턴만 입력 await, 적 턴은 기존 auto-policy 유지 | MVP 슬라이스는 플레이어 에이전시 검증이 목적. 적 AI 고도화는 후속. `FindFirstAliveOpponent` 정책 재사용으로 변경 최소화 |
| 인카운터는 파티 1~2 vs 적 2, Damage 스킬 2~3으로 최소 저작 | 타겟 선택·스킬 선택이 의미를 갖는 최소 구성. Heal은 현재 팩토리가 드롭하므로 이번 범위 밖 |
| 결과 채널·복귀 분기는 combat-battle-service 그대로(무수정) | ADR-0002 계약 불변 — 이번 slice는 그 계약을 씬 왕복 실플레이로 검증할 뿐 재설계하지 않음 |
| 전투 루프·복귀 nav를 전환 훅 안에서 직접 호출하지 않고 `UniTask.Yield()+Forget`으로 분리 | #91 씬 왕복 검증에서 발견한 latent 버그: `GamePlayDirector`의 전환 재진입 가드(`_isNavigating`, 한 번에 하나의 전환)가 전환 훅(`OnModeInAsync`/`OnResumedAsync`) 내부의 `NavigateAsync`를 드롭 → 전투 종료 pop·패배 village 복귀가 무산되며 프리즈. combat-battle-service가 씬 왕복 미검증이라 잠복해 있었음. 전환 훅은 "진입/재개 연출"만, 그 후 gameplay 액션(전투 루프·복귀 분기)은 Active 상태에서 실행되도록 fire-and-forget으로 분리 |

## #91 종단 검증에서 발견·수정한 버그

- **증상**: 전투 종료 후 `BattleMode`의 `NavigateAsync("gameplay://pop")`가 `[GamePlayDirector] 전환 진행 중 — 내비게이션 드롭`으로 무산 → 전투에서 프리즈. 패배 시 `ExplorationMode.OnResumedAsync`의 village ReplaceAll도 동일 경로로 드롭.
- **근인**: 전환 훅이 진행 중인 전환 안에서 재차 `NavigateAsync`를 호출 → 재진입 가드가 드롭(`GamePlayDirector.cs:72`).
- **수정**: `BattleMode.OnModeInAsync`는 패널만 show하고 전투 루프를 `RunBattleAsync(ct).Forget()`(선두 `await UniTask.Yield()`)로 분리 → ModeIn 전환 완료 후 Active에서 구동, 종료 pop이 정상 발행. `ExplorationMode.OnResumedAsync`의 패배 복귀도 `DeferredGameOverAsync(ct).Forget()`로 동일 분리. 엔진·패널·결과 채널·복귀 목적지는 불변.
- **교훈(재사용)**: `[[navigation-reentry-transition-hooks]]` — 모드는 전환 훅 안에서 `NavigateAsync`를 직접 await하지 않는다.

## persona5-slice / combat-battle-service에서 상속받은 결정

- **ADR-0002** — 결과는 `SetOutcome`→`NavigateAsync(pop)`, 직전 모드 `OnResumedAsync`가 `ConsumeOutcome` 1회 pull. URI에 결과 미탑재.
- **패배 복귀지** = `gameplay://exploration/village?switch=replaceall`(현행 허브). P5 아지트 도입 시 교체 — combat-battle-service와 동일.
- 전투 축 분할: `combat-battle-service`(엔진) → `combat-turn-loop-ui`(UI·씬 왕복). 이 feature가 후자.
