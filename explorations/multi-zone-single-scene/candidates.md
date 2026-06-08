# 후보 방향들

## Candidate A — 현재 구조 유지 (씬 1개 = Zone 1개)

**상태**: eliminated

변경 없음. `ZoneRegistry`가 현재 구조를 완벽하게 지원한다.

- **장점**: 구조적으로 단순하고 명확. ZoneRegistry·CatalogBaker 수정 불필요. 테스트 격리 명확.
- **단점**: Zone 수만큼 씬 파일 증가. 소규모 Zone도 독립 씬을 가져야 함.
- **적합한 경우**: Zone이 물리적으로 분리된 독립 구역인 경우 (현재 World1/World2 구조)

---

## Candidate B — ZoneRegistry + ZoneAsset 확장 (씬:Zone = 1:N)

**상태**: promoted → features/multi-zone-scene/

`ZoneAsset`을 씬당 여러 ZoneId를 지원하도록 확장. ZoneRegistry가 ZoneId로 특정 Zone을 찾도록 수정.

**변경 필요 항목**:

- `ZoneRegistry.FindZoneInScene(sceneName)` → `FindZoneInScene(sceneName, zoneId)` — ZoneId로 특정 Zone 검색
- `ZoneRegistry.ReleaseAsync()`: 씬 내 모든 Zone의 RefCount가 0일 때만 씬 언로드
- `CatalogBaker.ExtractZoneIdFromScene()`: 씬당 첫 번째만 추출하던 것 → 모든 Zone 추출
- `CatalogBaker.BakeSpawnPointCatalog()`: 씬 루트 전체 순회하여 모든 Zone 처리

**장점**: Zone 수 대비 씬 파일 수 감소. 지형이 연결된 구역을 하나의 씬에 묶을 수 있음.

**단점**: ZoneRegistry·CatalogBaker 수정 필요. 기존 테스트 영향 가능성.

---

## Candidate C — 테스트 목적 한정 경량 씬 구성

**상태**: eliminated

런타임 코드 변경 없음. 테스트 전용 씬 파일(예: `TestZoneA.unity`, `TestZoneB.unity`)을 추가하고, 테스트 SetUp 헬퍼로 여러 Zone 씬을 빠르게 Additive 로드하는 방식.

- **장점**: 구조 변경 제로. 테스트 격리 완벽 유지.
- **단점**: "단일 씬 내 멀티 Zone 배치" 자체를 해결하지는 않음. 테스트 씬 파일 관리 필요.
- **적합한 경우**: 목적이 런타임 구조 변경이 아니라 테스트 편의성 향상인 경우
