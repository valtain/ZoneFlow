# bootstrap — 설계 결정

| 결정 | 선택 | 이유 |
| --- | --- | --- |
| ColdStartup 씬 재로드 방식 | unload → reload | CLAUDE.md 원칙 — 정상 Play 흐름과 동일한 초기화 순서 보장 |
| GamePlayBootstrap 배치 위치 | DevBootstrap.unity 전용 | CLAUDE.md 원칙 — auto-managed 씬 배치 금지 |
| ExecutionOrder | -2000 | 씬 내 다른 모든 컴포넌트보다 먼저 실행 |
| DevBootstrap 빌드 포함 | 금지 | 개발 전용 진입점 |
| SceneType 지정 방식 | Inspector SerializeField | 씬마다 다를 수 있어 하드코딩 대신 Inspector |
