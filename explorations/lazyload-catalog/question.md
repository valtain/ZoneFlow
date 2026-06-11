# lazyload-catalog — 탐색 질문

> PanelCatalog의 직접 prefab 레퍼런스를 지연 로드 방식으로 교체했을 때 실익이 있는가?

## 컨텍스트

`PanelCatalog`는 `Entry.Prefab` 필드에 `UiPanel` 직접 레퍼런스를 저장한다.
ScriptableObject 역직렬화 시 Unity가 모든 직접 참조를 따라가므로, 게임 시작 시 사용하지 않는
패널 prefab들이 메모리에 즉시 올라간다.

현재 패널 수: MenuPanel, ExplorationHudPanel, StoryHudPanel (2~3개)

## 탐색 범위

- `LazyLoadReference<T>` vs Addressables `AssetReference` 트레이드오프
- Unity 6 환경에서 `LazyLoadReference<UiPanel>` 타입 호환성
- `CatalogBaker`에서 프로그래매틱 할당 가능 여부
- 현재 규모(소수 패널)에서 개선 실익

Out of scope: Resources.LoadAsync (Resources 폴더 패턴은 채택 않음), AssetBundle

## 성공 기준

- 세 후보 방향 중 하나를 채택 또는 기각하는 근거 있는 결론
- 채택 시: CatalogBaker 수정 방향 파악
- 기각 시: 언제 재검토할지 기준 명시
