# Feature: Exploration HUD

## 목표

ExplorationMode 진입 시 다이나믹한 연출로 HUD를 표시하고, 퇴장 시 숨긴다.
HUD는 플레이어 체력/스탯바와 존/씬 정보를 표시한다.

## 확정 설계

### 아키텍처

ExplorationMode가 HUD 생명주기를 완전히 소유한다.
ModeIn/Out은 UI를 적확한 타이밍에 로딩하기 위해 설계된 상태 — HUD 연결의 자연스러운 지점.

```
OnPlayedAsync   → UiService.MainView.SetAsync(hudPrefab)  // 생성(숨김 상태)
OnModeInAsync   → UiService.MainView.ShowAsync()          // 슬라이드인 + 스태거
OnModeOutAsync  → UiService.MainView.HideAsync()          // 슬라이드아웃
OnStoppedAsync  → UiService.MainView.ClearAsync()         // 파괴
```

Sleep은 ModeOut을 거쳐 발생하고 Resume은 ModeIn을 거치므로 별도 훅 불필요.

### 레이어

`UiMainViewLayer` 사용. Floating 레이어는 알림 등 별도 용도로 예약.

### HUD 연출

단순 fade 아님. 각 HUD 요소(체력바, 존 정보)가 화면 밖에서 스르릉 슬라이드인.
요소별 진입 타이밍 오프셋(스태거). 퇴장은 역방향 슬라이드아웃.

### 데이터 바인딩

- **존 정보**: ExplorationMode가 `OnPlayedAsync`에서 `HudPanel.Initialize(ZoneAsset)` 호출
- **플레이어 스탯**: HudPanel이 `OnShowAsync`에서 `PlayerService.Instance` 이벤트 구독, `OnHideAsync`에서 해제 (self-binding)
- `GamePlayMode.ZoneAsset` protected 유지 — 외부 노출 불필요

## 구현 범위

1. `UiMainViewLayer` — Set / Show / Hide / Clear API 구현
2. `UiService` — MainView 레이어 노출 API 추가
3. `PlayerStats` — HP/MaxHp + C# event 데이터 모델 신규 설계
4. `PlayerService` — PlayerStats 노출
5. `HudPanel` — UiPanel 기반, 슬라이드 연출 + self-binding
6. `ExplorationMode` — OnPlayed/ModeIn/ModeOut/Stopped 훅 연결

## Out of Scope

BattleMode HUD, 미니맵, 인벤토리 UI
