# Zone-Entry-Guard — 탐색 질문

> 콘텐츠 레벨 "입장 조건/예외처리"(키 필요, 보스 처치 전 잠김, 쿨다운 등)를 어디에, 어떤 구조로 표현하고 평가할 것인가? 현재 Portal.cs에 하드코딩된 `IsSpawnCooldown` 같은 단발 가드를 일반화할 메커니즘을 찾는다.

## 컨텍스트

상호작용→전환 흐름은 이미 구현돼 있다:

```
Portal.OnTriggerEnter
  → (Player 태그 / Director 준비 / IsSpawnCooldown) 체크   ← 가드가 여기 하드코딩됨
  → OnInteractAsync(director, ct)
  → director.NavigateAsync(uri).Forget()
```

- `IInteractable` = `InteractableId` + `OnInteractAsync(director, ct)` ([IInteractable.cs](../../Assets/ZoneFlowAssets/Runtime/GamePlay/Interactable/IInteractable.cs))
- `InteractableCatalog` = Zone 씬 미로드 상태에서도 `InteractableId→(ZoneId, NavigationUri)` 조회 ([InteractableCatalog.cs](../../Assets/ZoneFlowAssets/Runtime/GamePlay/Interactable/InteractableCatalog.cs))
- 현재 유일한 콘텐츠 가드 사례: [Portal.cs:25](../../Assets/ZoneFlowAssets/Runtime/GamePlay/Interactable/Portal.cs#L25)의 `IsSpawnCooldown` — Portal에 박혀 있고 일반화돼 있지 않음.

**층위 구분 (중요)**: 이 탐색은 **게임플레이 레벨 가드**(콘텐츠마다 다른 가변 규칙)다.
시스템 레벨 **전환 재진입 가드**(한 번에 하나의 전환만, 기계적 불변식)는 별개이며
이미 이슈 **#54**로 분리·구현됨. 두 층위를 섞지 않는다.

## 탐색 범위

- **규칙의 위치**: 컴포넌트(Interactable에 부착) vs 데이터(Catalog/SO) vs 중앙(Director/Navigation 파이프라인)
- **규칙의 평가 대상**: 키 보유·플래그·진행도 등 게임 상태를 어디서 읽는가 (GameState/Flags 서비스 필요성)
- **합성(composability)**: 한 포털에 복수 조건(키 AND 쿨다운)을 어떻게 누적/평가하는가
- **거부 피드백**: 입장 거부 시 사유를 UI로 전달하는 횡단 관심사 처리
- **Zone 미로드 평가 가능성**: Catalog 기반 조회처럼, 대상 Zone이 로드되지 않은 상태에서도 평가 가능해야 하는가
- **IsSpawnCooldown 재배치**: 이것이 콘텐츠 가드인지 기계적 디바운스인지 — 같은 층에 둘지 재검토

Out of scope: 전환 재진입(#54), Save/Load 연동(AQ-5), 구체적 인벤토리/키 아이템 시스템 설계

## 성공 기준

- 규칙 위치·평가 대상·합성·피드백 4개 축에 대한 후보들이 트레이드오프와 함께 정리됨
- `IsSpawnCooldown`을 새 메커니즘으로 흡수하는 경로가 후보로 제시됨
- 게임 상태(키/플래그) 의존성을 어디에 둘지 방향이 잡힘
- 후속 `/feature` 승격이 가능한 상태 (실제 콘텐츠 규칙 2~3개가 생겼을 때 적용 가능한 최소 설계)
