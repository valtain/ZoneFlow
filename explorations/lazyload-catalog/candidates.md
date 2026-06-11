# 후보 방향들

## Candidate A: LazyLoadReference<UiPanel>
**상태**: promoted

PanelCatalog.Entry.Prefab을 `LazyLoadReference<UiPanel>`로 교체한다.
SO 역직렬화 시 prefab을 즉시 올리지 않고, `.asset` 접근 시점에 동기 로드한다.

**변경 범위:**
- `PanelCatalog.Entry.Prefab` → `LazyLoadReference<UiPanel> PrefabRef`
- `_lookup` 타입 → `Dictionary<string, LazyLoadReference<UiPanel>>`
- `TryGetPanel` 반환 타입 → `out LazyLoadReference<UiPanel>`
- 호출부 (`PanelMode`, `ExplorationMode`, `StoryMode`) → `prefabRef.asset` 추가, 타입 캐스트는 동일 유지
- `CatalogBaker` → 암묵적 변환 활용: `LazyLoadReference<UiPanel> ref = menuPanel;`

**장점:**
- 신규 패키지 의존성 없음 (빌트인 API)
- 호출부 변경 최소 (동기 유지, `.asset` 추가만 필요)
- Addressables 없이 메모리 이연 확보
- CatalogBaker 변경 없이 암묵적 변환만으로 할당 가능

**단점:**
- 첫 `.asset` 접근은 동기 IO (메인 스레드 블록)
- 패널이 2~3개인 현재 규모에서 실측 메모리 효과 미미할 수 있음
- `LazyLoadReference<T>`가 MonoBehaviour 컴포넌트에 정상 작동하는지 Editor에서 검증 필요

---

## Candidate B: Addressables AssetReference
**상태**: eliminated — 이유: 패키지 미설치, 현 규모 과다 복잡도. 향후 패널 수 증가 시 A에서 마이그레이션 예정.

Panel prefab들을 Addressable 그룹으로 이동하고, `AssetReference` 필드로 교체한다.

**변경 범위:**
- `com.unity.addressables` 패키지 추가 (현재 미설치 확인됨)
- Panel prefab 에셋을 Addressable 마킹
- `PanelCatalog.Entry.Prefab` → `AssetReference PrefabRef`
- `TryGetPanel` → async (`UniTask<UiPanel?>`)
- 호출부 3곳 전면 async 전환
- `CatalogBaker` → `AddressableAssetSettings` API로 Entry 생성

**장점:**
- 진정한 비동기 로드 (메인 스레드 블록 없음)
- 런타임 메모리 완전 제어 (Release 가능)
- 규모 확장 시 가장 유연

**단점:**
- 패키지 추가 필요 (현재 미설치)
- CatalogBaker 대규모 수정
- PanelCatalog 인터페이스 파괴적 변경
- Addressables 번들 빌드 워크플로우 추가

---

## Candidate C: Status Quo 유지
**상태**: eliminated — 이유: 패턴 확립 가치 및 Addressables 이전 단계로 A 채택.

현행 직접 레퍼런스 유지. 패널 수 기준 재검토 시점을 명시한다.

**장점:**
- 변경 없음, 이미 O(1) 조회 최적화
- 현재 패널 2~3개 — prefab 크기 합산 메모리 이슈 없음

**단점:**
- 패널 수 증가 시 스타트업 메모리 증가
- 패턴 확립 없이 후속 개발 시 동일 문제 반복 가능

**재검토 기준**: 패널 수 10개 이상, 또는 스타트업 메모리 프로파일에서 PanelCatalog 관련 수치가 총 UI 메모리의 10% 이상 시
