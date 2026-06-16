# 후보 방향들

> 핵심 결정 축 4개:
> 1. **규칙 위치** — 컴포넌트 / 데이터 / 중앙
> 2. **평가 대상** — 게임 상태(키·플래그·진행도)를 어디서 읽는가
> 3. **합성** — 복수 조건 누적 방식
> 4. **거부 피드백** — 사유 전달 경로
>
> 공통 전제: 어떤 후보든 "게임 상태(키 보유/플래그)"를 읽을 **GameState/Flags 서비스**가 전제된다. 이 서비스의 존재 여부가 가장 큰 미해결 의존성.

---

## Candidate A — IEntryCondition 컴포넌트 (Interactable에 부착)

**상태**: active

`Portal`(또는 임의 IInteractable)이 같은 GameObject/자식의 `IEntryCondition`
컴포넌트들을 수집해, 상호작용 전 **전부 통과해야** 전환을 실행한다.

```csharp
public interface IEntryCondition {
    bool CanEnter(out string deniedReason);   // 또는 UniTask<bool>
}
// 예: RequireKeyCondition, RequireFlagCondition, CooldownCondition
```

- **장점**: 디자이너가 포털별로 조건을 조립(여러 컴포넌트 부착)·합성 자연스러움. Portal 코드는 "조건 전부 통과?"만 알면 됨. `IsSpawnCooldown`도 `CooldownCondition`으로 흡수 가능.
- **단점**: 조건이 Zone 씬에 부착되므로 **Zone 미로드 상태에선 평가 불가**(Catalog 조회 경로엔 안 실림). 각 Condition이 GameState 서비스에 접근해야 함.
- **평가 대상**: 각 Condition이 직접 GameState 서비스 조회.
- **거부 피드백**: `deniedReason` out 파라미터 → Portal이 UI(Floating/Toast)로 표시.

---

## Candidate B — 데이터 기반 조건 (Catalog/SO Entry 확장)

**상태**: active

`InteractableCatalog.Entry`에 조건을 **데이터로** 기술(예: `RequiredFlag`,
`RequiredKeyId`), 중앙(Director 또는 EntryGuard)이 GameState와 대조해 평가.

- **장점**: **Zone 미로드 상태에서도 평가 가능**(Catalog는 씬 독립). CatalogBaker로 자동 수집 흐름과 정합. 규칙이 직렬화 데이터라 추적·디버그 쉬움.
- **단점**: 표현력 제한(데이터로 표현 가능한 단순 조건만; "보스 N마리 처치 AND 시간대" 같은 복합 로직은 어려움). 조건 종류가 늘면 Entry 스키마가 비대해짐.
- **평가 대상**: 중앙 평가기가 GameState 서비스 조회.
- **거부 피드백**: 중앙 평가기가 사유 코드 반환 → 호출측이 UI 처리.

---

## Candidate C — 중앙 Navigation 가드 파이프라인

**상태**: active

`GamePlayDirector`(또는 전용 EntryGuard)가 등록된 `INavigationGuard` 목록을
전환 실행 전에 순차 평가. 한 곳(choke point)에서 모든 진입을 통제.

```csharp
public interface INavigationGuard {
    bool Allows(NavigationRequest req, out string deniedReason);
}
```

- **장점**: 단일 통제점 — 포털뿐 아니라 메뉴·스크립트 전환까지 일괄 적용. 규칙 추가가 Director 외부(가드 등록)로 분리됨.
- **단점**: Director가 콘텐츠 규칙과 결합될 위험(추상화로 완화 필요). 규칙이 **전역**이라 "이 특정 포털만"이라는 지역성을 표현하려면 req에 식별자가 충분해야 함. #54 재진입 가드와 같은 클래스에 살면 층위가 섞일 우려.
- **평가 대상**: 각 가드가 GameState 서비스 조회.
- **거부 피드백**: 가드가 사유 반환 → Director가 이벤트 발행 → UI 구독.

---

## Candidate D — Portal 서브클래스 / 가상 메서드 (최소안)

**상태**: active (베이스라인)

`Portal`에 `protected virtual bool CanInteract(out string reason)`를 두고,
특수 포털은 서브클래스(`LockedPortal` 등)로 오버라이드. `IsSpawnCooldown`은
베이스의 기본 구현으로 유지.

- **장점**: 가장 단순. 새 개념(인터페이스·서비스) 도입 최소. 즉시 적용 가능.
- **단점**: 조건 조합마다 서브클래스 폭증(키+쿨다운+플래그 = 조합 폭발). 데이터/디자이너 친화성 낮음. 합성 불가.
- **위치**: 코드(서브클래스). **평가 대상**: 오버라이드 내부에서 GameState 조회.

---

## 결정 보조 — 축별 매핑

```
              위치        Zone미로드평가   합성      디자이너친화   복잡도
A 컴포넌트     Interactable   ✗            ◎(부착)    ◎           중
B 데이터       Catalog/SO     ◎            △(스키마)  ○           중
C 중앙가드     Director       ◎(req기반)    ○(가드목록) △           중상
D 서브클래스   코드           ✗            ✗          ✗           하
```

**미해결 선결 과제**: 네 후보 모두 **GameState/Flags 서비스**(키 보유·진행 플래그
조회)를 전제한다. 이 서비스가 없으면 어느 것도 "쿨다운" 이상을 평가 못 함.
→ 콘텐츠 규칙이 실제로 1~2개 생길 때(예: demo 확장의 "열쇠 필요 던전") 함께 설계하는 게
YAGNI 측면에서 합리적. 그 시점에 A/B 중 하나(또는 A+B 하이브리드: 데이터로 기술하되
컴포넌트로 평가)를 채택.
