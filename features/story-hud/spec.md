# Feature: Story HUD

## 목표

StoryMode 진입 시 HUD를 표시하고, 퇴장 시 숨긴다.
현재 배너(스토리 모드 진입 안내) 구현을 기반으로, 향후 StoryMode에 필요한 HUD 요소를 확장한다.

## 확정 설계

### 아키텍처

StoryMode가 HUD 생명주기를 완전히 소유한다.

```
OnPlayedAsync  → UiService.SetMainViewAsync(StoryHudPrefab) + _hud.Initialize(zone)
OnModeInAsync  → SpawnPlayer() + UiService.ShowMainViewAsync()
OnModeOutAsync → UiService.HideMainViewAsync()
OnStoppedAsync → UiService.ClearMainViewIfIs(_hud)
```

Sleep은 ModeOut을 거쳐 발생하고 Resume은 ModeIn을 거치므로 별도 훅 불필요.

### 레이어

`UiMainViewLayer` 사용. Floating 레이어는 알림 등 별도 용도로 예약.

### 현재 HUD 구성 (배너)

- `BannerContainer` — 상단 전체폭 배너 (높이 70px)
- `ModeLabel` — "◆ STORY" 고정 텍스트
- `ZoneNameLabel` — "ZoneId@SceneName" 형식

배너 슬라이드인(OutBack 0.35s) / 아웃(InBack) 연출.

### 확장 지점

추가 HUD 요소 결정 시:
- `StoryHudPanel.Initialize()` — 추가 데이터 주입
- `StoryHudPanel.OnShowAsync()` — PlayerStats 구독 추가 가능 (ExplorationHudPanel 패턴)
- `StoryHudPanel.OnHideAsync()` — 구독 해제

## Out of Scope

BattleMode HUD, 미니맵, 인벤토리 UI
