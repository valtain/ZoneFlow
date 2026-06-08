# Feature: multi-zone-scene

## 개요

단일 Unity 씬 파일 안에 Zone 컴포넌트를 여러 개 배치할 수 있도록 ZoneRegistry·CatalogBaker를 확장한다.

현재 구조(씬:Zone = 1:1)를 씬:Zone = 1:N으로 확장하여, 지형이 연결된 구역을 하나의 씬에 묶거나 씬 파일 수를 줄이는 것이 목적이다.

## 변경 범위

### ZoneRegistry

- `FindZoneInScene(sceneName)` → `FindZoneInScene(sceneName, zoneId)` 변경
  - 씬 루트 오브젝트를 순회하면서 `Zone.ZoneId == zoneId`인 Zone을 찾아 반환
- `ReleaseAsync(zoneId)`: 씬 내 모든 Zone의 RefCount가 0일 때만 씬 언로드
  - `_handles`에서 동일 씬을 참조하는 다른 ZoneHandle이 남아 있으면 씬 언로드 생략

### CatalogBaker

- `BakeZoneAssetCatalog()`: 씬당 복수 ZoneAsset 항목 등록 지원 (`ExtractZoneIdFromScene()` 인라인 후 제거)
- `BakeSpawnPointCatalog()` / `BakeInteractableCatalog()`: 이미 루트 전체 순회 구조이므로 변경 불필요

## 제약 조건

- Zone 컴포넌트는 반드시 씬 루트 GameObject에 배치 (`root.GetComponent<Zone>()` 패턴 유지)
- ZoneId는 전역 고유성 유지 (씬 내에서도, 씬 간에서도 중복 불가)
- 기존 World1/World2 씬 구조(씬 1개 = Zone 1개)는 그대로 동작해야 함 (하위 호환)

## 성공 기준

- 단일 씬에 Zone 2개를 배치하고 각각 독립적으로 Navigate·Acquire·Release 가능
- 한 Zone Release 시 다른 Zone이 살아있으면 씬 언로드 발생하지 않음
- CatalogBaker Bake 후 두 Zone 모두 ZoneAssetCatalog·SpawnPointCatalog·InteractableCatalog에 등록됨
- 기존 `GamePlayNavigationTests` (World1→World2) 통과
