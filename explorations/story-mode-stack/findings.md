# 탐색 결과

**결론**: 5개 이슈 중 2개는 의도된 설계, 1개는 기능 설계 필요, 1개는 간단한 버그 수정, 1개는 보류.

**채택된 방향**:
- Candidate 1 (임의 위치 Spawn) → `features/flexible-spawn/`으로 승격. 설계 확정 필요.
- Candidate 4 (Parse 실패 로그) → `/quick`으로 `Debug.LogWarning` 추가.

**폐기된 방향**:
- Candidate 2 (ShellMode 배치) — 이유: 의도된 설계. Shell은 커스텀 모드로 기본 Spawn 없음.
- Candidate 3 (ReplaceAll 상태 Skip) — 이유: 의도된 설계. 전체 ModeOut은 과도한 복잡도.

**생성된 Feature**: features/flexible-spawn/

**CLAUDE.md 반영 필요**: 없음
