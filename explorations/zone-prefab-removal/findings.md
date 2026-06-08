# 탐색 결과

**결론**: ZonePrefab 및 프리팹 기반 Zone 로드 경로를 전면 제거한다.

**채택된 방향**: Candidate A — ZonePrefab, ZonePrefabRoot, 프리팹 분기 로직 전면 제거

**폐기된 방향**: Candidate B (현상 유지) — Addressables 도입 시 어차피 ZoneRegistry 재설계가 필요하므로 forward-compatible 구조 보존의 실익이 없음

**생성된 Feature**: TASK 이슈로 직접 등록 (feature 설계 불필요한 단순 dead code 제거)

**CLAUDE.md 반영 필요**: 없음 (아키텍처 원칙 변경 없음, Zone은 씬 기반만 지원하는 것이 현재도 사실상 규칙)
