# 탐색 로그

- [2026-05-18 | start] exploration 시작. 탐색 대상: ServiceLocator·SceneService·ColdStartup+DevBootstrap 신규 설계.
- [2026-05-18 | decision] SceneType: `Gameplay` 대신 `Zone` 분리 (CLAUDE.md 씬 계층 기준).
- [2026-05-18 | decision] Service 패턴: Candidate A(`MonoService<T>.Instance`) 채택. VContainer는 Milestone 1 이후 재검토.
- [2026-05-18 | explore] SceneType 3종(Standalone/Shell/Zone), ColdStartup 흐름(unload→reload), 구현 순서 5단계 확정. candidates.md에 전체 설계 기록.
- [2026-05-18 | promote] Candidate A → features/service_locator/, features/scene_service/, features/bootstrap/ 생성
- [2026-05-18 | close] 탐색 완료.
