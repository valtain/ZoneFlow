---
paths:
  - "Assets/ZoneFlowAssets/Scenes/**"
  - "Assets/ZoneFlowAssets/Story/**"
---

# Rule: 레벨 콘텐츠 (존·내러티브)

`Assets/ZoneFlowAssets/Scenes/**`·`Assets/ZoneFlowAssets/Story/**`를 저작하기 전에 이 규칙을 적용한다.
원문(canonical): [docs/architecture/scene-hierarchy.md](../../docs/architecture/scene-hierarchy.md), [docs/architecture/system-layers.md](../../docs/architecture/system-layers.md), [docs/architecture/constraints.md](../../docs/architecture/constraints.md).

## 필수

- **카탈로그는 손편집 금지** — `ZoneAssetCatalog`/`SpawnPointCatalog`/`InteractableCatalog`/`PanelCatalog`(`Runtime/Data/*.asset`)는 CatalogBaker 출력물이다. 씬에서 Zone/Interactable/SpawnPoint를 추가·이동·삭제했으면 카탈로그를 직접 고치지 말고 **재베이크**한다.
- **베이크는 `BakeAll` 단일 진입점** — 부분 베이크가 없다. 에이전트가 작업 중 개별 베이크하지 않고, **작업 종료 시 메인 세션이 1회 직렬 베이크**한다. (한 사이클에 여러 존을 바꿔도 베이크는 마지막에 한 번.)
- **NavigationUri 하드코딩 금지** — `NavigationUriBuilder`(`Runtime/GamePlay/Navigation/`)로 생성한다. 형식 `gameplay://<mode>/<zone>?id=<interactable>`.
- **SpawnPoint** — 존당 default 1개 + 선택적 named. 포탈은 목적지 named spawn을 지정한다.
- **존 구성** — Zone 루트 GameObject = `Zone` 컴포넌트 + 자식 interactable. multi-zone-per-scene 허용(Dungeon=5)은 설계 선택지다.
- **내러티브** — 스토리 상태는 ContentServices(영속)에 있고 존 씬에 두지 않는다. **Yarn 대사는 영문 유지**(한글 폰트 글리프 누락 회피). 파일은 `Story/Scripts/*.yarn`.
- **아트 디렉션** — 그레이박스 가독성, 머티리얼/광원 대비 기반 웨이파인딩, 무드 라이팅은 허용. 신규 머티리얼은 `unity_material_create`로 생성하고, 순수 오서링 콘텐츠(머티리얼·스프라이트)는 **패키지 최상위**(`Assets/ZoneFlowAssets/Materials/` 등)에 둔다([scriptable-data.md](scriptable-data.md)). 셰이더 작성·렌더 파이프라인 변경은 unity-specialist 몫.
- **`.meta` 동반** — 씬/자산 이동 시 `.unity`/`.asset`/`.prefab`과 `.meta`(GUID)를 항상 함께 옮긴다.
- **시각 검증** — `unity_graphics_scene_capture`/`unity_play_mode`로 레이아웃·동선·가독성을 확인한다.

## 모호하면

새 상호작용 타입(NPC·아이템·트리거·퍼즐)이나 런타임 시스템이 필요하면 멈추고 `unity-specialist`, Zone 생명주기·Mode 스택 경계가 불분명하면 `architecture-director` 검토를 권한다.
