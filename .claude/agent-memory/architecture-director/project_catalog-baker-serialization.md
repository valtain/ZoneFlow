---
name: catalog-baker-serialization
description: CatalogBaker는 단일 진입점 BakeAll로 4개 카탈로그를 한꺼번에 베이크 — 멀티 에이전트 동시 작업의 직렬화 병목
metadata:
  type: project
---

`Assets/ZoneFlowAssets/Editor/CatalogBaker.cs`는 `[MenuItem("ZoneFlow/Bake Catalogs")] BakeAll()` 단일 진입점이며, 호출 시 Zone/Spawn/Interactable/Panel 4개 카탈로그를 **전부 재스캔·재베이크**한다. 카탈로그 `.asset` 4개는 모두 `Runtime/Data/`에 위치(`ZoneAssetCatalog`, `SpawnPointCatalog`, `InteractableCatalog`, `PanelCatalog`). PanelCatalog 베이크는 `Assets/ZoneFlowAssets` 전체 프리팹을 스캔하며, UI 패널 프리팹은 `Runtime/Prefabs/`에 있다(`Runtime/Ui/`가 아님).

**Why:** level-designer/ui-designer 같은 콘텐츠 저작 에이전트를 여러 개 둘 때, "각 에이전트가 자기 카탈로그만 베이크" 같은 분리가 **불가능**하다 — 베이크는 부분 베이크가 안 되고 전량이며, 두 에이전트의 진행 중 씬 변경을 서로 끌어들여 카탈로그를 오염시킬 수 있다.

**How to apply:** 멀티 에이전트 콘텐츠 작업 설계 시, 베이크는 작업 종료 시 메인 세션이 1회 직렬 수행하도록 못박는다. "에이전트별 베이크" 전제는 거부한다. 콘텐츠 스케일이 커지면 BakeAll의 전량 재스캔이 병목이 되는지 → [[zoneflow-aq6-catalog-bake-scale]] 참조.
