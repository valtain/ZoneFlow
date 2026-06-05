# 후보 방향들

## [A] ExplorationMode 내부 연결 — 채택

**상태**: promoted

ExplorationMode가 OnModeIn/Out 훅에서 HUD를 직접 Show/Hide한다.

```
ExplorationMode
  ├─ OnModeInAsync   → HUD.ShowAsync(ct)   [슬라이드인 + 스태거 연출]
  └─ OnModeOutAsync  → HUD.HideAsync(ct)   [슬라이드아웃 연출]
```

근거:
- ModeIn/Out은 이 목적을 위해 설계된 상태 — 의도를 코드로 명확히 표현
- Sleep/Resume은 내부적으로 ModeOut/ModeIn을 경유하므로 별도 처리 불필요
- ExplorationMode가 HUD 생명주기를 완전히 소유 → 책임 명확

연출:
- 각 HUD 요소(체력바, 존 정보)가 화면 밖에서 스르릉 슬라이드인
- 요소별 진입 타이밍 오프셋 (스태거) — 동시 진입 아님
- 퇴장은 역방향 슬라이드아웃

세부 설계 확정:

**HUD 생명주기** — Played에 생성, Stopped에 파괴. ModeIn/Out에서 Show/Hide.
```
OnPlayedAsync   → MainView.SetAsync(hudPrefab)  // 생성(숨김)
OnModeInAsync   → MainView.ShowAsync()          // 슬라이드인 + 스태거
OnModeOutAsync  → MainView.HideAsync()          // 슬라이드아웃
OnStoppedAsync  → MainView.ClearAsync()         // 파괴
```

**ZoneAsset** — GamePlayMode.ZoneAsset protected 유지. ExplorationMode가 OnPlayedAsync에서 HUD.Initialize(ZoneAsset)로 직접 전달.

**PlayerStats 바인딩** — C# event 기반. HUD 패널이 OnShowAsync에서 PlayerService.Instance 구독, OnHideAsync에서 해제 (self-binding, MenuPanel 패턴과 일관).

**UiMainViewLayer API** — Set / Show / Hide / Clear 4단계.
UiService에 MainView 레이어 노출 API 추가.

---

## [B] GamePlayDirector 중앙 관리 — 폐기

**상태**: eliminated

이유: Director가 ActiveMode 타입별 분기 로직을 가지면 책임이 과중해진다. 모드별 HUD 정책이 흩어지고 확장 시 Director가 계속 수정되는 구조가 된다.

---

## [C] HudService 독립 서비스 — 폐기

**상태**: eliminated

이유: ExplorationMode HUD 하나를 위한 서비스 계층 추가는 과도한 설계다. 모드 전환 이벤트 구독 메커니즘도 별도로 만들어야 하며, Mode 내부 연결보다 복잡하고 이점이 없다.
