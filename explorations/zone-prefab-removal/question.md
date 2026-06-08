# Zone-Prefab-Removal — 탐색 질문

> ZonePrefab 및 프리팹 기반 Zone 로드 경로를 지금 제거해야 하는가?

## 컨텍스트

ZoneAsset은 Zone 로드 방식을 두 가지로 설계했다:
- **씬 기반** (SceneName): Content 씬을 additively 로드 ← 현재 유일하게 사용
- **프리팹 기반** (ZonePrefab): GameObject를 Instantiate ← 미사용, dead code

현재 상태:
- ZoneAssetCatalog.asset의 모든 ZonePrefab = null
- CatalogBaker가 bake 시 ZonePrefab을 항상 null로 초기화
- ZoneRegistry.AcquireAsync/ReleaseAsync에 프리팹 분기 dead code 존재
- GamePlayDirector.ZonePrefabRoot 필드 미사용

## 탐색 범위

- ZonePrefab이 사용될 만한 미래 시나리오 검토
- 제거 시 영향 범위 확인
- Addressables 도입 가능성과의 관계

Out of scope: Addressables 시스템 자체 설계, 런타임 에셋 로딩 아키텍처 변경

## 성공 기준

- 제거 또는 유지 중 하나로 결론을 내린다
- 결론의 근거가 명확하다
