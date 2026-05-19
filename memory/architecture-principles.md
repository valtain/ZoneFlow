# 아키텍처 원칙

불일치 시 구현 반복 발생. 코드 작성 전 반드시 확인할 것.

| 원칙 | 결정 | 이유 |
| --- | --- | --- |
| 서비스 생성 책임 | **씬** (GameObject 배치) | 코드가 생성하면 씬 구조와 충돌 |
| DontDestroyOnLoad | **가급적 사용 안 함** | 씬 상주가 생명주기를 보장 |
| MonoService 자동 GameObject 생성 | **사용 안 함** | MonoService는 참조(ServiceLocator)만 |
| SO 씬 이름 | `so.name` (에셋 파일명) | 별도 SerializeField 불필요 |
| 레지스트리 접근 | Inspector 직렬화 우선 | Resources.Load는 CoreServices.asset + PrefabZone 전용 Resources 폴더만 허용 |
| 예외 처리 | `Debug.Assert` | `throw` 사용 안 함 |
| 비동기 | UniTask 전용 | Coroutine 혼용 금지 |
| ColdStartup 씬 재로드 | **unload → reload 유지** | 정상 Play 흐름과 동일한 초기화 순서 보장 목적 |
| 개발용 Bootstrap | **auto-managed 씬에 배치 금지** | `GamePlayBootstrap`은 `DevBootstrap.unity` 전용 |
| CoreServices·GamePlayServices 직접 로드 | **SceneService 경유 원칙** | 직접 `SceneManager.LoadSceneAsync` 호출 금지 |

## 관련 문서

- 씬 계층 상세: [docs/architecture/scene-hierarchy.md](../docs/architecture/scene-hierarchy.md)
- 시스템 계층 상세: [docs/architecture/system-layers.md](../docs/architecture/system-layers.md)
- 설계 결정 근거: [docs/decisions/](../docs/decisions/)
