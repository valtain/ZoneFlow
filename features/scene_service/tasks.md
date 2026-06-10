# scene_service — 태스크

| # | 태스크 | 상태 |
| --- | --- | --- |
| 1 | `SceneSo` ScriptableObject 생성 | #21 closed |
| 2 | `SceneType` enum 확장 — `Standalone / Shell / Zone` | #22 closed |
| 3 | `SceneService` API — `internal` 접근자 + `CancellationToken` 적용 | #23 closed |
| 4 | `SceneService.LoadSceneAdditiveAsync / UnloadSceneAsync` — CT 파라미터 추가 | #24 closed |
| 5 | SceneSo 에셋 생성 (CoreServicesSo) | #25 closed |
| 6 | 테스트 씬 파일 생성 — Splash / Intro / World1 / World2 (Zone + SpawnPoint 포함) | #26 closed |
| 7 | Build Settings에 CoreServices / 4개 씬 등록 | #27 closed |
| 8 | Runtime 테스트 — Bootstrap 흐름 및 씬 기반 로딩 검증 | #28 |

> **SceneSo 폐기 결정**: SceneAsset은 Editor-only. CoreServices bootstrap은 static 경로여서
> 인스턴스 SerializeField 사용 불가. const string 유지가 적절.
>
> **SceneType 확장 폐기**: Zone만 의미 있으며 Standalone/Shell 분기 없음.
