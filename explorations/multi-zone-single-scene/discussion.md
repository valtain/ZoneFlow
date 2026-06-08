# 탐색 로그

- [2026-06-08 | start] 탐색 시작. 질문: 단일 씬 내 멀티 Zone 배치 가능 여부.

- [2026-06-08 | brainstorm] 코드 레벨 분석 완료. 핵심 제약 4곳 확인:
  1. `ZoneRegistry.FindZoneInScene()` — `GetComponentInChildren<Zone>()` 루트 순회 중 첫 번째 Zone만 반환. 씬에 Zone이 여러 개 있어도 나머지는 무시됨.
  2. `ZoneAsset` — `{ZoneId, SceneName}` 구조로 씬:ZoneId = 1:1 매핑. 하나의 씬에 복수 ZoneId를 연결할 방법 없음.
  3. `ZoneRegistry.ReleaseAsync()` — `handle.Zone.gameObject.scene.name`으로 씬 이름 추출 후 씬 언로드. Zone 1개 Release 시 씬 전체가 언로드되어 동일 씬 내 다른 Zone도 함께 소멸.
  4. `CatalogBaker.ExtractZoneIdFromScene()` — 루트 순회 중 Zone 발견 즉시 break, 첫 번째 Zone만 ZoneAssetCatalog에 등록. 두 번째 Zone은 카탈로그에서 누락됨.
  결론: 물리적 배치는 가능하지만 ZoneRegistry·CatalogBaker가 인식하지 못하므로 현재 구조에서 의도대로 동작하지 않음.
  Candidate A(현재 유지) / Candidate B(구조 확장) / Candidate C(테스트 한정 대안) 3개 후보 도출.

- [2026-06-08 | promote] Candidate B → features/multi-zone-scene/ 생성
