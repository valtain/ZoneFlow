---
paths:
  - "Assets/ZoneFlowAssets/Runtime/**"
---

# Rule: Runtime 코드

`Assets/ZoneFlowAssets/Runtime/**`의 파일을 Edit/Write 하기 전에 이 규칙을 적용한다.
원문(canonical): [docs/conventions/coding-style.md](../../docs/conventions/coding-style.md), [docs/architecture/constraints.md](../../docs/architecture/constraints.md).

## 필수

- **비동기는 UniTask 전용** — `IEnumerator`/`StartCoroutine`/`WaitForSeconds` 금지. fire-and-forget은 `.Forget()`. 취소는 `destroyCancellationToken`(구형 `GetCancellationTokenOnDestroy()` 사용 안 함). 딜레이는 `UniTask.Delay()`.
- **MonoBehaviour에 public 필드 금지** — `[SerializeField]` private 또는 `[field: SerializeField] public T Foo { get; private set; }`.
- **예외 처리는 `Debug.Assert`** — 게임 로직 단언에 `throw` 사용 안 함.
- **서비스 생성은 씬 책임** — 코드가 GameObject를 만들어 서비스를 생성하지 않는다. MonoService는 참조(ServiceLocator)만. `DontDestroyOnLoad` 가급적 사용 안 함.
- **씬 로딩은 SceneService 경유** — `SceneManager.LoadSceneAsync` 직접 호출 금지.
- **실행 순서** — 서비스 계층은 `[DefaultExecutionOrder(-1000)]`, Bootstrap 계층(`ColdStartup`, `GamePlayBootstrap`)은 `-2000`.
- `GamePlayBootstrap`은 `DevBootstrap.unity` 전용 — auto-managed 씬에 배치 금지.
- **약어는 한 단어로 casing** — `HudView`, `_uiPanel`. Interface는 `I` prefix(`IDamageable`).
- public/protected 멤버에 **한국어 XML doc**(`/// <summary>`), 교차 참조는 `<see cref=""/>`.

## 모호하면

아키텍처 경계(Zone 생명주기·Mode 스택·서비스 생성 책임)가 불분명하면 멈추고 `architecture-director` 검토를 권한다.
