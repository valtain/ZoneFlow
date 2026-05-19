# 탐색 결과

**결론**: Milestone 1의 세 핵심 시스템을 `MonoService<T>.Instance` 패턴 기반으로 설계하고, SceneType을 Zone 타입으로 분리하며, ColdStartup unload→reload 흐름을 확정했다.

**채택된 방향**: Candidate A — `MonoService<T>.Instance` (정적 per-type accessor, 씬 배치 기반)

**폐기된 방향**:

- Candidate B (컨테이너형 ServiceLocator) — Inspector 직렬화 원칙과 상충
- Candidate C (Inspector 직렬화 전용) — 전역 진입점 필요 시 접근 방법 없음
- Candidate D (VContainer) — 초기 단계 복잡도 부담; 규모 확장 시 재검토

**생성된 Feature**:

- `features/service_locator/` — `MonoService<T>` 베이스 클래스
- `features/scene_service/` — SceneType·SceneSo·SceneService
- `features/bootstrap/` — ColdStartup·GamePlayBootstrap·DevBootstrap

**CLAUDE.md 반영 필요**: 없음 (기존 원칙과 모두 일치)
