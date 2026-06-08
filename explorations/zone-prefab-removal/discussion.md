# 탐색 로그

- [2026-06-08 | start] ZonePrefab 제거 탐색 시작. 현재 모든 Zone이 씬 기반이고 CatalogBaker가 ZonePrefab을 null로 고정 초기화.

- [2026-06-08 | explore] 탐색 결과: ZonePrefab은 ZoneAsset.cs L17 정의, ZoneRegistry.cs L41-55(Acquire), L76-79(Release)에서 분기 처리, GamePlayDirector.cs L23(ZonePrefabRoot), L39(ZoneRegistry 생성)에서 참조. 모두 dead code.

- [2026-06-08 | decision] 사용자 의도 확인: Addressables 도입 시 Zone 로딩 구조 전면 재설계가 필요하므로 지금 프리팹 경로를 보존해도 미래 재사용 가치 없음. YAGNI 원칙에 따라 전면 제거(Candidate A) 채택.

- [2026-06-08 | promote] Candidate A → TASK 이슈로 승격 예정.
