# 탐색 결과

**결론**: PanelCatalog.Entry.Prefab을 `LazyLoadReference<UiPanel>`로 교체한다.

**채택된 방향**: Candidate A — LazyLoadReference\<UiPanel\>
- SO 역직렬화 시 prefab 즉시 로드 제거
- 호출부 변경 최소 (`.asset` 추가만 필요)
- CatalogBaker 암묵적 변환으로 수정 최소화
- 신규 패키지 의존성 없음

**폐기된 방향**:
- Candidate B (Addressables): 패키지 미설치, 현 규모 대비 과도한 변경 — 향후 패널 수 증가 시 A에서 마이그레이션 경로로 유효
- Candidate C (Status Quo): 패턴 확립 가치 및 Addressables 이전 단계로 채택 기각

**생성된 Feature**: 없음 (TASK 이슈로 직행)

**CLAUDE.md 반영 필요**: 없음

**향후 마이그레이션 기준**: 패널 수 10개 초과 또는 스타트업 메모리 프로파일 이슈 확인 시 Addressables로 전환 검토
