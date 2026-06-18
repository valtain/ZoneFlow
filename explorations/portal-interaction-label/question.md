# portal-interaction-label — 탐색 질문

## 핵심 질문

플레이어에게 IInteractable(Portal 등)의 Label을 **보기 편하게** 보여주려면 어떻게 해야 하는가?
정적 월드 라벨을 대체해, 플레이어가 다가가거나(혹은 조준하면) 친화 명칭 + 행동 힌트가
읽기 좋게 표시되는 **근접/조준 상호작용 프롬프트 시스템**을 설계한다.

## 컨텍스트 (현재 상태와 문제)

현재 Portal의 "Label"은 에디터 툴이 생성하는 **월드 공간 `TextMeshPro` 자식 오브젝트**다
(`Assets/ZoneFlowAssets/Editor/SceneSetupTool.cs:159-168`). 문제점:

- `tmp.text = portalId` — 기술 ID 원문 노출(예: `world1_main`), 플레이어용 친화 명칭 아님
- **빌보드 없음** — 카메라를 향하지 않아 각도에 따라 좌우 반전·모서리로 보여 읽기 어려움
- fontSize 8 고정, 흰색 단색 — 배경 대비/거리 가독성 고려 없음
- 런타임 감지 시스템 부재 — Portal은 `OnTriggerEnter` 시 **자동으로** `OnInteractAsync`를 호출
  (`Assets/ZoneFlowAssets/Runtime/GamePlay/Interactable/Portal.cs:21-28`). "다가가서 누른다" 흐름 없음

사용자 확정: **대상 = 플레이어(인게임)**, **범위 = 근접/조준 상호작용 프롬프트 시스템**.

## 코드베이스 제약 (탐색 입력)

- `IInteractable`은 `InteractableId` + `OnInteractAsync` 둘 뿐 — **표시 명칭 멤버 없음** → 신규 추가 필요(공통 전제)
- UI 인프라: `UiService`에 독립 Canvas 레이어 보유, **`Floating` 레이어가 프롬프트에 최적**
  (`UiService.cs:19`, `UiFloatingLayer`는 빈 컨테이너). `UiPanel` 베이스 + PrimeTween + TextMeshPro.
  참고 패턴: `ExplorationHudPanel`
- 입력: `PlayerInputHandler`에 **Move/Sprint만** 존재 — Interact/Look 액션 없음
- 카메라: **TPS(Cinemachine)** — 근접 감지가 조준 레이캐스트보다 관행적
- Zone이 자식 `IInteractable`를 수집(`Zone.Interactables`), `InteractableCatalog` 레지스트리 존재
- 로컬라이제이션 패키지 없음 (전부 하드코딩)

## 탐색 범위

- 포함: Label 표시 방식(스크린/월드/하이브리드), 감지 방식(근접/조준), 표시 명칭 데이터 모델
- 제외: 실제 코드 구현(feature 승격 후), 로컬라이제이션 도입, 상호작용 모델 전면 개편

## 성공 기준

- 플레이어가 어떤 각도·거리에서도 대상의 친화 명칭을 읽기 쉽게 인지
- 기존 UI 인프라(`Floating` 레이어·`UiPanel`·PrimeTween)와 일관
- IInteractable 추상화를 깨지 않고 표시 명칭을 결합
- 후속 feature로 바로 승격 가능한 단일 추천 방향 도출
