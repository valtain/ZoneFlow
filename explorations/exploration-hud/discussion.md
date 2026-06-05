# 탐색 로그

- [2026-06-05 | start] exploration-hud 탐색 시작. 핵심 질문: ExplorationMode OnModeIn/Out 기반 HUD 아키텍처.

- [2026-06-05 | brainstorm] 코드베이스 전체 조사 완료. 핵심 발견:
  - "HUD"라는 명칭의 시스템·씬·컴포넌트 없음 — 신규 설계 필요
  - UI 시스템: UiService(7레이어) + UiPanel 베이스 + PanelCatalog 구조
  - ExplorationMode는 OnModeInAsync(플레이어 스폰)만 구현, OnModeOutAsync 미오버라이드
  - UiMainViewLayer: 현재 구현 상태 미확인 — API 확장 필요 여부 조사 필요
  - UiPanel: ShowAsync/HideAsync 훅만 있고 데이터 주입 메커니즘 없음
  - GamePlayMode.ZoneAsset: protected — 외부 접근 불가, 노출 API 추가 필요
  - PlayerService: HP/스탯 데이터 모델 없음 — PlayerStats 신규 설계 필요
  - 기존 MenuPanel이 GamePlayDirector.Instance 직접 접근하는 self-binding 패턴 사용 중

- [2026-06-05 | decision] Candidate A (ExplorationMode 내부 연결) 채택. B(Director 중앙), C(HudService) 폐기. 근거: ModeIn/Out이 이 목적을 위해 설계된 상태이며, 모드가 HUD 생명주기를 소유하는 것이 책임 분리에 명확.

- [2026-06-05 | decision] 레이어: UiMainViewLayer 채택. UiFloatingLayer는 알림 등 별도 용도 예약. Sleep/Resume은 ModeOut/ModeIn을 경유하므로 별도 훅 처리 불필요 — OnModeIn/Out 쌍만으로 전체 생명주기 커버.

- [2026-06-05 | decision] 연출 방향 확정: 단순 fade 아님. 각 HUD 요소가 화면 밖에서 스르릉 슬라이드인 + 스태거(요소별 타이밍 오프셋). 퇴장은 역방향 슬라이드아웃. ModeIn/Out이 연출의 소유자임을 명확히 표현.

- [2026-06-05 | decision] HUD 인스턴스 생명주기 확정: OnPlayedAsync에 생성(초기 숨김), OnModeInAsync에 ShowAsync, OnModeOutAsync에 HideAsync, OnStoppedAsync에 ClearAsync(파괴). Zone이 살아있는 동안 인스턴스 유지 → Sleep/Resume 반복 시 상태 보존.

- [2026-06-05 | decision] ZoneAsset 노출 방식: GamePlayMode.ZoneAsset은 protected 유지. ExplorationMode가 OnPlayedAsync에서 HUD 생성 시 직접 전달(Initialize). 외부 접근 불필요.

- [2026-06-05 | decision] PlayerStats 바인딩: C# event 기반. PlayerStats(신규 설계)가 OnHpChanged 등 이벤트 발행. HUD 패널이 OnShowAsync에서 PlayerService.Instance를 통해 구독, OnHideAsync에서 해제. MenuPanel의 GamePlayDirector.Instance 직접 접근 패턴과 일관.

- [2026-06-05 | decision] UiMainViewLayer API 설계: Set/Show/Hide/Clear 4단계. SetAsync<T>(prefab, ct) → 생성(숨김 상태), ShowAsync(ct) / HideAsync(ct) → 연출, ClearAsync(ct) → 파괴. UiService에 MainView 레이어 노출 API 추가 필요.

- [2026-06-05 | decision] ExplorationMode 최종 구조 확정. 미결 사항 전부 해소 → promote 준비 완료.

- [2026-06-05 | promote] Candidate A → features/exploration-hud/ 생성
