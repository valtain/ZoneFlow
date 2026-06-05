# Decisions

- HUD 생명주기: Played 생성 / Stopped 파괴. ModeIn/Out에서 Show/Hide. (Sleep/Resume은 ModeOut/ModeIn 경유로 자동 처리)
- 레이어: UiMainViewLayer (Floating은 알림 예약)
- ZoneAsset: protected 유지, ExplorationMode가 Initialize로 직접 전달
- PlayerStats 바인딩: C# event 기반 self-binding (MenuPanel 패턴 일관)
- HUD 연출: 슬라이드인/아웃 + 스태거 (단순 fade 아님)
