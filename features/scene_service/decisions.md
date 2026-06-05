# scene_service — 설계 결정

| 결정 | 선택 | 이유 |
| --- | --- | --- |
| SceneType 분류 | Standalone / Shell / Zone | CLAUDE.md 씬 계층 기준; Gameplay 대신 Zone |
| SO 씬 이름 | `so.name` (에셋 파일명) | CLAUDE.md 원칙 — 별도 SerializeField 불필요 |
| CoreServices 참조 | Inspector SerializeField (CoreServicesSo 1개) | GamePlayDirector가 CoreServices 씬에 있어 GamePlayServices 불필요; Resources.Load는 CoreServices.asset 전용 |
| SceneService 직접 로드 금지 | SceneService 경유 원칙 | CLAUDE.md — SceneManager 직접 호출 금지 |
| 비동기 API | UniTask | CLAUDE.md — Coroutine 혼용 금지 |
