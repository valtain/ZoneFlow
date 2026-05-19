# scene_service — 설계 스펙

## 목표

씬 로드·언로드를 UniTask 기반으로 오케스트레이션하는 SceneService와 관련 타입을 구현한다.
CoreServices·GamePlayServices Auto-managed 씬 로드 흐름의 진입점.

## 주요 컴포넌트

- `SceneType` (enum) — `Standalone` / `Shell` / `Zone`
- `SceneSo` (ScriptableObject) — 씬 메타데이터; `so.name` = 빌드 씬 이름
- `SceneService : MonoService<SceneService>` — CoreServices 씬에 배치
  - `BootstrapAsync(SceneType, CancellationToken)` — Prerequisites 로드
  - `LoadSceneAdditiveAsync(SceneSo, CancellationToken)`
  - `UnloadSceneAsync(SceneSo, CancellationToken)`
  - `[field: SerializeField]` — CoreServicesSo, GamePlayServicesSo Inspector 연결

## 데이터 흐름

ColdStartup → `BootstrapAsync(Zone)` → CoreServices 씬 Additive 로드 → SceneService.Instance 등록
→ GamePlayServices 씬 Additive 로드 → 원본 씬 Unload → 원본 Zone 씬 재로드
