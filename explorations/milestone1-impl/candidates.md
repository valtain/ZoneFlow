# 후보 방향들

---

## Candidate A — `MonoService<T>.Instance` (채택)

**상태**: promoted

서비스별 정적 accessor를 제공하는 제네릭 베이스 클래스.
레퍼런스(`nexus_frame/main`)와 동일한 패턴. 씬 배치 MonoBehaviour가 Instance를 등록한다.

### `MonoService<T>` API 초안

```csharp
[DefaultExecutionOrder(-1000)]
public abstract class MonoService<T> : MonoBehaviour where T : MonoService<T>
{
    public static T Instance { get; private set; }
    public static bool IsReady => Instance != null;

    protected virtual void Awake()
    {
        Debug.Assert(Instance == null, $"Duplicate {typeof(T).Name} detected.");
        Instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
```

**채택 이유**: 씬 배치 원칙과 일치, 레퍼런스 패턴 재사용, Milestone 1 범위에서 충분

---

## Candidate B — 컨테이너형 ServiceLocator

**상태**: eliminated

`ServiceLocator.Get<ISceneService>()` 방식. 코드에서 동적 조회 — Inspector 직렬화 원칙과 상충하여 제거.

---

## Candidate C — Inspector 직렬화 우선 (static 없음)

**상태**: eliminated

서비스 간 참조를 전부 SerializeField로 연결. 전역 진입점이 필요한 ColdStartup 등에서
SceneService에 접근할 방법이 없어 현실적으로 static accessor 최소화 버전(A)으로 수렴.

---

## Candidate D — VContainer DI

**상태**: eliminated

LifetimeScope 씬 구성 복잡도가 초기 단계에 부담. Milestone 1 이후 규모 확장 시 재검토.

---

## SceneType 분류 (결정)

```csharp
public enum SceneType
{
    Standalone,  // Prerequisites 없음 (타이틀, 로딩 씬 등)
    Shell,       // CoreServices 필요
    Zone,        // CoreServices + GamePlayServices 필요
}
```

CLAUDE.md 씬 계층 기준으로 `Gameplay` 대신 `Zone` 사용.

---

## 씬 계층 구조 (결정)

| 씬 | SceneType | 특징 |
| --- | --- | --- |
| CoreServices | — | Auto-managed; SceneService·기반 서비스 배치 |
| GamePlayServices | — | Auto-managed; 게임플레이 전용 서비스 배치 |
| Zone0, Zone1 | Zone | Content 씬; ColdStartup 배치 |
| DevBootstrap | — | Dev 전용; GamePlayBootstrap 배치 |

---

## ColdStartup 흐름 (결정)

```text
[Zone씬 Play]
    ↓
ColdStartup.Awake()  [DefaultExecutionOrder(-2000)]
    ↓
SceneService.IsReady? → true  → gameObject.SetActive(false)  (이미 부트스트랩됨)
                      → false → 아래 진행
    ↓
씬 내 다른 루트 GameObject 비활성화
    ↓
Resources.Load<SceneSo>("CoreServices")
LoadSceneAdditiveAsync(coreServicesSo)   → SceneService.Instance 등록됨
    ↓
SceneType == Zone?
    → LoadSceneAdditiveAsync(gamePlayServicesSo)
    ↓
UnloadSceneAsync(originalSceneName)      [CLAUDE.md: unload → reload 유지]
    ↓
SceneService.LoadSceneAsync(originalZoneSo)  [정상 흐름으로 Zone 재로드]
```

---

## DevBootstrap 흐름 (결정)

- `DevBootstrap.unity` 전용, 빌드 포함 금지
- `GamePlayBootstrap` MonoBehaviour 배치 (DefaultExecutionOrder(-2000))
- `Start()`에서 ColdStartup과 동일한 Prerequisites 로드 후 지정 Zone으로 이동

---

## 구현 순서 (결정)

| 단계 | 대상 | 이유 |
| --- | --- | --- |
| 1 | `MonoService<T>`, SceneType, SceneSo | 기반 타입, 의존 없음 |
| 2 | SceneService (로드/언로드 API) | ColdStartup이 의존 |
| 3 | CoreServices 씬 구성 | SceneService 배치 |
| 4 | ColdStartup | SceneService.IsReady 의존 |
| 5 | DevBootstrap 씬 + GamePlayBootstrap | ColdStartup 흐름 검증용 |
