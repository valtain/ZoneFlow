# 네이밍 컨벤션

## 약어(Acronym) 처리

약어는 하나의 단어로 취급한다. 모두 대문자로 쓰지 않는다.

| 위치 | 규칙 | 예시 |
| --- | --- | --- |
| PascalCase (타입·메서드) | 첫 글자만 대문자 | `HudView`, `UiPanel`, `XmlParser` |
| camelCase (private field) | 모두 소문자 | `_hudView`, `_uiPanel` |
| Interface | `I` prefix + PascalCase | `IUiTransitionEffect`, `IDamageable` |

## Inspector 직렬화

```csharp
[field: SerializeField]
public T Foo { get; private set; } = default;
```

MonoBehaviour에는 public field 사용 안 함. `[SerializeField]` private field 또는 위 패턴 사용.

## 비동기 메서드명

UniTask 반환 메서드에 `Async` suffix 불필요. 의미 있는 이름 사용.

## XML 문서

모든 `public`·`protected` 멤버에 한국어 XML 문서 필수.

```csharp
/// <summary>서비스 초기화 완료 여부를 반환합니다.</summary>
public bool IsInitialized { get; private set; }
```

## ExecutionOrder

- Service 계층: `[DefaultExecutionOrder(-1000)]`
- Bootstrap 계층: `[DefaultExecutionOrder(-2000)]`
