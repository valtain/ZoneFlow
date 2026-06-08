# 탐색 결과

**결론**: ExplorationMode OnModeIn/Out 훅 기반 HUD 연결 설계 완료. 모든 세부 결정 확정.

**채택된 방향**: Candidate A — ExplorationMode 내부 연결

- OnPlayedAsync → MainView.SetAsync(hudPrefab) — 생성(숨김)
- OnModeInAsync → MainView.ShowAsync() — 슬라이드인 + 스태거
- OnModeOutAsync → MainView.HideAsync() — 슬라이드아웃
- OnStoppedAsync → MainView.ClearAsync() — 파괴
- PlayerStats 바인딩: C# event 기반, HUD 패널 self-binding (OnShowAsync 구독 / OnHideAsync 해제)
- ZoneAsset: protected 유지, OnPlayedAsync에서 HUD.Initialize(ZoneAsset) 직접 전달
- UiMainViewLayer API: Set / Show / Hide / Clear 4단계

**폐기된 방향**:

- Candidate B (GamePlayDirector 중앙 관리) — Director 책임 과중, 모드별 분기 로직 집중 우려
- Candidate C (HudService 독립 서비스) — 단일 HUD를 위한 서비스 계층 과도한 설계

**생성된 Feature**: features/exploration-hud/

**CLAUDE.md 반영 필요**: 없음
