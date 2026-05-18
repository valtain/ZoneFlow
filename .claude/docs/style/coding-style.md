# Coding style

Follows [Unity C# coding standards](https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity). Rules below take precedence where they differ.

포맷/네이밍의 기계적 규칙(indent, 줄바꿈, `_camelCase` private fields, `I` prefix interfaces, PascalCase 타입 등)은 [`.editorconfig`](../.editorconfig)에서 IDE가 직접 강제한다.

## Naming

- Public inspector-exposed property: `[field: SerializeField]` with `public T Foo { get; private set; } = default;`
- Async methods that return `UniTask`: descriptive name without mandatory `Async` suffix (e.g., `LoadSceneInternal`, `EnsurePrerequisitesLoaded`)
- **약어(Acronym)는 하나의 단어로 취급한다.** 모두 대문자로 쓰지 않고 위치에 맞는 casing을 적용한다.
  - PascalCase 위치: 첫 글자만 대문자 — `HudView`, `UiPanel`, `XmlParser`, `GameEventSo`
  - camelCase / private field 위치: 모두 소문자 — `_hudView`, `_uiPanel`
  - 예외: Interface `I` prefix는 단독 접두어이므로 그대로 유지 — `IDamageable` (not `Idamageable`)
- **Interface는 반드시 `I` prefix로 시작한다** — `IUiTransitionEffect`, `IDamageable`

## Async

- Use **UniTask** exclusively — no coroutines (`IEnumerator` / `StartCoroutine`).
- Fire-and-forget: call `.Forget()` on the returned `UniTask`.
- Cancellation: `destroyCancellationToken` (Unity 2022.2+ 내장 프로퍼티) 을 MonoBehaviour 수명에 연결. 구형 `this.GetCancellationTokenOnDestroy()` extension은 사용하지 않는다.
- Timing delays: `UniTask.Delay()` instead of `WaitForSeconds`.

## Unity-Specific

- **No public fields** on MonoBehaviours — use `[SerializeField]` private fields or `[field: SerializeField]` properties.
- Cache `this.transform` into a local field (`_cachedTransform`) to avoid repeated property access.
- Use `Debug.Assert()` for defensive invariant checks; do not use `throw` for game-logic assertions.
- **Editor 폴더 스크립트에는 `#if UNITY_EDITOR`를 사용하지 않는다.** `Assets/**/Editor/` 폴더 내 파일은 Unity가 자동으로 에디터 전용으로 처리한다. `#if UNITY_EDITOR`는 Runtime 폴더 내 파일에서 에디터 전용 코드를 부분적으로 포함할 때만 사용한다.
- Scripts that must initialize early use `[DefaultExecutionOrder(-1000)]` (서비스 계층). Prerequisites를 로드하는 Bootstrap 계층(`ColdStartup`, `GamePlayBootstrap`)은 `-2000`을 사용해 서비스보다 먼저 실행되도록 한다.

## Documentation

- XML doc comments (`/// <summary>`) in **Korean** on all public and protected members.
- Use `<see cref=""/>` for cross-references to related types/members.
