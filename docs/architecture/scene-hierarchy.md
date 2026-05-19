# 씬 계층

ZoneFlow의 씬은 세 종류로 분류된다. 종류에 따라 로드 방식과 배치 책임이 다르다.

| 씬 종류 | 예시 | 특징 |
| --- | --- | --- |
| **Auto-managed** | CoreServices, GamePlayServices | SceneService가 SceneType 기반으로 자동 로드; 수동 배치 금지 |
| **Content** | Zone0, Zone1 | ColdStartup 배치; SceneType=Zone → Prerequisites 자동 로드 후 reload |
| **Dev** | DevBootstrap | 개발 전용; GamePlayBootstrap 배치; 빌드 포함 금지 |

## 핵심 규칙

- Auto-managed 씬에는 GamePlayBootstrap을 배치하지 않는다.
- CoreServices·GamePlayServices는 SceneService를 경유해서만 로드한다 — SceneManager.LoadSceneAsync 직접 호출 금지.
- ColdStartup의 씬 재로드 방식은 unload → reload 를 유지한다 (정상 Play 흐름과 동일한 초기화 순서 보장).
