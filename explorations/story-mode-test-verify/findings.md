# 탐색 결과

**결론**: `StoryMode`에 대한 Runtime 통합 테스트가 전무하다. `GamePlayNavigationTests.cs`에 최소 2개 케이스를 추가해야 한다.

**채택된 방향**: 기존 `GamePlayNavigationTests.cs` 파일에 StoryMode 케이스 2개 추가

| 케이스 | URI 패턴 | 검증 항목 |
|---|---|---|
| A — 기본 진입 | `gameplay://story/world1?id={spawnPointId}` | StoryMode active, Stack=1, ZoneId 확인 |
| B — 스택+Pop | ExplorationMode → StoryMode(stack) → Pop | Stack 전이(1→2→1), ExplorationMode 복귀 |

**폐기된 방향**:
- C(스폰 위치 검증) — 씬 데이터 의존성 높음, Editor 단위 테스트로 분리 권장. 이 이슈 범위 밖.
- D(null 방어 케이스) — `Zone != null` 체크가 코드에 이미 존재. 별도 케이스 불필요.

**생성된 Feature**: 없음 (기존 테스트 파일 확장이므로 새 feature 불필요)

**다음 액션**: 새 TASK 이슈 등록 → `GamePlayNavigationTests.cs`에 케이스 A + B 추가

**CLAUDE.md 반영 필요**: 없음
