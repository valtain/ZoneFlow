# 탐색 결과

**결론**: 조건부 가능 — ZoneRegistry·CatalogBaker 4곳 수정으로 단일 씬 내 멀티 Zone 지원 가능.

**채택된 방향**: Candidate B — ZoneRegistry + ZoneAsset 확장 (씬:Zone = 1:N)

- `ZoneRegistry.FindZoneInScene()` → ZoneId 파라미터 추가, 특정 Zone 검색
- `ZoneRegistry.ReleaseAsync()` → 씬 내 모든 Zone RefCount가 0일 때만 씬 언로드
- `CatalogBaker.ExtractZoneIdFromScene()` → 첫 번째 break 제거, 씬 내 모든 Zone 추출
- `CatalogBaker.BakeSpawnPointCatalog()` → 씬 루트 전체 순회하여 모든 Zone 처리

**폐기된 방향**:

- Candidate A (현재 구조 유지) — 씬 파일 수 증가, 연결된 구역 분리 불가
- Candidate C (테스트 한정 대안) — 런타임 구조 문제 미해결

**생성된 Feature**: features/multi-zone-scene/

**CLAUDE.md 반영 필요**: 없음
