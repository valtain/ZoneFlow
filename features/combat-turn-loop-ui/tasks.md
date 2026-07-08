# combat-turn-loop-ui — Tasks

부모 에픽: #86 · 마일스톤: M8 · Combat

| # | 태스크 | 상태 |
| --- | --- | --- |
| 1 | 전투 인카운터 콘텐츠 저작(`SkillAsset`×2~3 Damage · `PersonaAsset`×1~2 · `EnemyAsset`×2 · `BattleEncounterAsset`) + `BattleService.DefaultEncounter`를 `CoreServices.unity`에서 배선 | #87 ✅ |
| 2 | `BattlePanel : UiPanel`(MainView) 저작 — HP/이름·현재 행동자·액션 버튼(기본공격+Damage 스킬)·타겟 선택·결과 연출 + `AwaitPlayerActionAsync`/`PresentActionAsync` API, PanelCatalog 등록·BakeAll | #88 ✅ |
| 3 | `BattleMode` 인터랙티브 루프 — 뷰모델 파생(Id→이름/스킬라벨) + `OnPlayedAsync`/`OnModeIn/Out`/`OnStopped` 패널 생명주기 + auto-policy while를 플레이어턴 await 루프로 교체 | #89 ✅ |
| 4 | 던전 전투 트리거 Interactable(`gameplay://battle/boss_room?switch=stack`) 배치 + battle host→`BattleMode(zoneAsset=boss_room)` 매핑 확인·배선 + Interactable 카탈로그 bake | #90 ✅ |
| 5 | 종단 검증 — 던전→아레나→인터랙티브 턴→승/패→복귀 PlayMode/수동 왕복(TC-01~08), EditMode 회귀 green 확인 | #91 ✅ |
