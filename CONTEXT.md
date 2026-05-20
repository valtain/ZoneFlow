# ZoneFlow — LLM Context Brief

외부 LLM(ChatGPT, Claude.ai 등)과 협업할 때 이 문서 전체를 프롬프트 앞에 붙여넣는다.  
최신 상태는 `docs/`, `memory/`, `features/` 폴더를 참조할 것.

---

## 1. Project Overview

**ZoneFlow**는 Unity 위에 올라가는 게임 아키텍처 프레임워크다.  
ServiceLocator, 씬 오케스트레이션, 부트스트랩 흐름을 계층화하여 게임플레이 확장을 단순하게 만드는 것이 목적이다.

| 항목 | 내용 |
| --- | --- |
| 언어 | C# (Unity 2022.2+) |
| 비동기 | UniTask (git UPM) — Coroutine 완전 대체 |
| 렌더 | URP 17.3.0 |
| 테스트 | NUnit (Unity Test Runner) |
| 네임스페이스 루트 | `ZoneFlow` |

### 현재 상태 (Milestone 1 — Core Foundation)

| 컴포넌트 | 상태 | 경로 |
| --- | --- | --- |
| `MonoService<T>` | ✅ 구현 완료 | `Assets/ZoneFlowAssets/Runtime/Services/MonoService.cs` |
| `SceneService` | 🔄 설계 완료, 구현 예정 | `features/scene_service/spec.md` |
| `Bootstrap` (ColdStartup, DevBootstrap) | 🔄 설계 완료, 구현 예정 | `features/bootstrap/spec.md` |

---

## 2. System Layers

런타임은 4계층으로 구성된다. 계층 간 의존 방향은 위에서 아래로만 흐른다.

| 계층 | 역할 | 예시 |
| --- | --- | --- |
| **Service** | 영속적 시스템; `MonoService<T>.Instance`로 접근 | `GameManager`, `AudioManager`, `SceneService` |
| **Scene** | additive 씬 로드·언로드 오케스트레이션 | `CoreServices`, `GamePlayServices` (auto-managed 씬) |
| **Zone** | 게임플레이 컨텍스트 단위; 씬 그룹 소유 | `Zone0`, `Zone1` (Content 씬) |
| **Mode** | Zone 내부 게임플레이 행동 단위 | `MenuMode`, `PlayMode`, `CinematicMode` |

---

## 3. Scene Hierarchy

씬은 세 종류로 분류되며, 종류마다 로드 책임과 배치 규칙이 다르다.

| 종류 | 예시 | 로드 방식 | 배치 규칙 |
| --- | --- | --- | --- |
| **Auto-managed** | `CoreServices`, `GamePlayServices` | SceneService가 SceneType 기반으로 자동 Additive 로드 | `GamePlayBootstrap` 배치 금지 |
| **Content** | `Zone0`, `Zone1` | `ColdStartup` 배치; SceneType=Zone이면 Prerequisites 로드 후 unload→reload | 게임플레이 오브젝트 배치 |
| **Dev** | `DevBootstrap` | 에디터에서 수동 실행; 빌드 포함 금지 | `GamePlayBootstrap` 배치 위치 |

### 핵심 씬 흐름

```
[DevBootstrap.unity 실행]
  → ColdStartup.Awake()
  → SceneService.BootstrapAsync(SceneType.Zone)
    → CoreServices 씬 Additive 로드 → SceneService.Instance 등록
    → GamePlayServices 씬 Additive 로드
    → 원본 Content 씬 Unload
    → 원본 Content 씬 Reload (초기화 순서 보장)
```

---

## 4. Critical Constraints — MUST NOT VIOLATE

이 원칙을 어기면 구현을 재작업해야 한다. 코드 제안 전 반드시 확인할 것.

| # | 원칙 | 금지 | 허용 |
| --- | --- | --- | --- |
| 1 | 서비스 생성 책임 | 코드에서 `new` 또는 `Instantiate`로 서비스 생성 | 씬에 `GameObject`로 배치 |
| 2 | `DontDestroyOnLoad` | 서비스에 `DontDestroyOnLoad` 적용 | 씬 상주로 생명주기 보장 |
| 3 | `MonoService` 자동 생성 | `MonoService<T>`가 스스로 GameObject 생성 | ServiceLocator 참조 역할만 |
| 4 | SO 씬 이름 | `SceneSo`에 별도 `[SerializeField] string sceneName` 추가 | `so.name` (에셋 파일명) 그대로 사용 |
| 5 | 레지스트리 접근 | `Resources.Load`로 서비스 탐색 | Inspector 직렬화 우선; `Resources.Load`는 `CoreServices.asset`과 PrefabZone 전용 폴더만 허용 |
| 6 | 예외 처리 | `throw new Exception(...)` | `Debug.Assert(condition, message)` |
| 7 | 비동기 | `IEnumerator` / `StartCoroutine` | `UniTask` 전용 |
| 8 | ColdStartup 재로드 | `unload → reload` 방식 변경 | `unload → reload` 유지 (정상 Play 흐름과 동일) |
| 9 | `GamePlayBootstrap` 배치 위치 | Auto-managed 씬에 배치 | `DevBootstrap.unity` 전용 |
| 10 | 씬 로드 진입점 | `SceneManager.LoadSceneAsync` 직접 호출 | `SceneService` 경유 |

---

## 5. Coding Conventions

### 명명 (Naming)

약어는 하나의 단어로 취급한다. 모두 대문자 금지.

| 위치 | 규칙 | 예시 |
| --- | --- | --- |
| PascalCase (타입·메서드) | 첫 글자만 대문자 | `HudView`, `UiPanel`, `XmlParser`, `GameEventSo` |
| camelCase (private field) | 모두 소문자 | `_hudView`, `_uiPanel` |
| Interface | `I` prefix + PascalCase | `IUiTransitionEffect`, `IDamageable` |

UniTask 반환 메서드에 `Async` suffix 불필요. 의미 있는 이름 사용 (`LoadSceneInternal`, `EnsurePrerequisitesLoaded`).

### Inspector 직렬화

```csharp
// ✅ 권장
[field: SerializeField]
public T Foo { get; private set; } = default;

// ❌ 금지
public T Foo;  // public field on MonoBehaviour
```

### 비동기

```csharp
// ✅ 권장
private async UniTask LoadSceneInternal(CancellationToken ct)
{
    await SceneManager.LoadSceneAsync(...).ToUniTask(cancellationToken: ct);
}

// ✅ MonoBehaviour 수명 연결
SomeAsync(destroyCancellationToken).Forget();  // Unity 2022.2+ 내장

// ❌ 금지
this.GetCancellationTokenOnDestroy()  // 구형 extension
StartCoroutine(...)                   // Coroutine 혼용 금지
```

### Unity 규칙

- `Debug.Assert()` 사용 — `throw` 금지
- `this.transform` 반복 접근 금지 → `_cachedTransform` 캐시
- `Assets/**/Editor/` 폴더 내 스크립트에는 `#if UNITY_EDITOR` 불필요 (Unity가 자동 처리)

### ExecutionOrder

| 계층 | Attribute |
| --- | --- |
| Service | `[DefaultExecutionOrder(-1000)]` |
| Bootstrap | `[DefaultExecutionOrder(-2000)]` |

### XML 문서

모든 `public`·`protected` 멤버에 **한국어** XML 문서 필수.

```csharp
/// <summary>서비스 초기화 완료 여부를 반환합니다.</summary>
public bool IsReady => Instance != null;
```

---

## 6. Key Implementations

### MonoService\<T\> (구현 완료)

```csharp
[DefaultExecutionOrder(-1000)]
public abstract class MonoService<T> : MonoBehaviour where T : MonoService<T>
{
    public static T Instance { get; private set; }
    public static bool IsReady => Instance != null;

    protected virtual void Awake()
    {
        Debug.Assert(Instance == null, $"[{typeof(T).Name}] 중복 인스턴스");
        Instance = (T)this;
    }

    protected virtual void OnDestroy() => Instance = null;
}
```

### SceneService (설계 완료, 미구현)

```csharp
// SceneType enum
enum SceneType { Standalone, Shell, Zone }

// SceneSo — so.name이 빌드 씬 이름
class SceneSo : ScriptableObject { /* 씬 메타데이터 */ }

// SceneService — CoreServices 씬에 GameObject로 배치
class SceneService : MonoService<SceneService>
{
    [field: SerializeField] public SceneSo CoreServicesSo { get; private set; }
    [field: SerializeField] public SceneSo GamePlayServicesSo { get; private set; }

    public UniTask BootstrapAsync(SceneType type, CancellationToken ct);
    public UniTask LoadSceneAdditiveAsync(SceneSo scene, CancellationToken ct);
    public UniTask UnloadSceneAsync(SceneSo scene, CancellationToken ct);
}
```

---

## 7. Key File Map

```
Assets/ZoneFlowAssets/
  Runtime/Services/MonoService.cs       ← ServiceLocator 베이스 (구현 완료)
  Tests/Editor/MonoServiceTests.cs      ← NUnit 단위 테스트

docs/
  architecture/scene-hierarchy.md       ← 씬 3종 분류 및 핵심 규칙
  architecture/system-layers.md         ← 4계층 구조
  architecture/project-structure.md     ← 패키지 레이아웃
  conventions/coding-style.md           ← 코딩 규칙 원문
  decisions/ADR-001-service-locator.md  ← MonoService<T> 설계 근거
  decisions/ADR-002-scene-service.md    ← SceneService 설계 근거
  decisions/ADR-003-bootstrap.md        ← Bootstrap 설계 근거

memory/
  architecture-principles.md            ← 10가지 핵심 원칙 (항상 확인)
  naming-conventions.md                 ← 네이밍 규칙 요약
  CURRENT_DIRECTION.md                  ← 현재 Milestone 및 다음 단계

features/
  service_locator/spec.md               ← MonoService<T> 설계 스펙
  scene_service/spec.md                 ← SceneService 설계 스펙
  bootstrap/spec.md                     ← Bootstrap 설계 스펙
```
