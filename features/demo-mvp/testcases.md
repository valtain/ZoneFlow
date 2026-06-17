# demo-mvp — 검증 시나리오

## 개발자 실행 가이드

처음 데모를 띄우는 절차. 셋업·베이크는 이미 커밋(`9b00a79` 외)에 반영돼 있으므로
재현이 필요할 때만 1~2단계를 실행한다.

1. **(재현 시) Zone 셋업** — 메뉴 `ZoneFlow/Setup/Setup Demo MVP`
   - World1에 `village` Zone, World2에 `dungeon` Zone을 추가한다. 멱등(동일 ZoneId 존재 시 스킵).
   - 각 Zone에 `{zone}_default`·`{zone}_entrance` SpawnPoint와 포털을 생성:
     village→`portal_to_dungeon`(`gameplay://exploration/dungeon?id=dungeon_entrance`),
     dungeon→`portal_to_village`(`gameplay://exploration/village?id=village_entrance`).
2. **(재현 시) 카탈로그 베이크** — 메뉴 `ZoneFlow/Bake Catalogs`
   - ZoneAssetCatalog / SpawnPointCatalog / InteractableCatalog에 village·dungeon 엔트리를 반영한다.
   - Zone·SpawnPoint·Portal을 씬에서 추가/이동한 뒤에는 반드시 다시 실행한다.
3. **Play 진입** — `Assets/ZoneFlowAssets/Scenes/DevBootstrap.unity`를 열고 Play
   - `DevBootstrap`이 `GamePlayDirector.BootstrapAsync`로 초기화하고 MenuPanel을 띄운다.
   - MenuPanel **New Game** → village로 진입 (#52에서 `NewGameUri` =
     `gameplay://exploration/village?switch=replaceall`로 변경됨).
   - 빠른 Zone 단독 확인이 필요하면 `World1.unity`/`World2.unity`를 직접 열고 Play해도
     `ColdStartup`이 부트스트랩한다.

## 기본 흐름 (명제 A)

- [x] village 진입 시 ExplorationMode가 Active 상태, 플레이어 조종 가능
- [x] `portal_to_dungeon` 진입 → dungeon Zone 로드 + village Zone 언로드 (로그 또는 씬 전환 관찰)
- [x] Zone 전환 전후 ExplorationMode 스택이 Push/Pop/Replace 없이 그대로 유지 (**명제 A**)
- [x] 전환 후 플레이어가 `dungeon_entrance`에 배치됨 (Dungeon 입구 위치)
- [x] 역방향: `portal_to_village` 진입 → village 복귀, `village_entrance` 배치, ExplorationMode 유지 (**명제 A 확인**)

## 추가 검증

- [x] NavigationUri 파싱 성공 여부 확인 (로그: `Navigation: gameplay://exploration/dungeon`)
- [x] Zone 언로드 시 village 오브젝트 정리됨 (Hierarchy 또는 메모리 관찰)
- [x] 양방향 이동 반복 시 ExplorationMode가 계속 유지 (스택 손상 X)
