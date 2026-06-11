# Design Decisions: Story HUD

## 배너 단일 요소 선택 (현재)

**결정**: 최초 구현에서 배너 하나로 최소화.

**이유**: StoryMode는 탐색보다 연출 중심. 체력바 등 전투 정보가 즉시 필요하지 않으므로 진입 안내 배너만으로 충분하다고 판단. 향후 StoryMode 게임플레이 설계가 확정되면 필요한 요소를 추가한다.

## HUD 생명주기 소유권

**결정**: StoryMode가 HUD 생명주기를 완전히 소유 (ExplorationMode 패턴 동일).

**이유**: Mode가 Zone 컨텍스트를 소유하고 있으므로 HUD 초기화(`Initialize(zone)`) 타이밍을 Mode가 제어하는 것이 자연스럽다. UiService는 레이어 API만 제공.

## UiMainViewLayer 재사용

**결정**: ExplorationHudPanel과 동일하게 `UiMainViewLayer` 사용.

**이유**: StoryMode HUD는 Exploration과 동일한 "모드 진행 중 주 HUD" 역할. 레이어 분리가 필요한 이유 없음.
