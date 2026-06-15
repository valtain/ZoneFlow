# 탐색 결과

**결론**: YarnSpinner를 ZoneFlow Story 모드에 통합하되, DialogueRunner를 **전용 ContentServices 씬에 호스팅되는 DialogueService(MonoService)**로 둔다. 내러티브 진행 상태(Yarn 변수)의 수명을 ContentServices 씬의 로드~언로드 경계에 종속시켜, ZoneFlow 제약(DontDestroyOnLoad 금지 / 씬=서비스 생성 책임)을 위반하지 않고 Zone 전환 중 생존을 보장한다.

**채택된 방향**: Candidate C(서비스 스코프)의 발전형 — ContentServices 씬 + DialogueService

- ContentServices 씬: 콘텐츠 플레이 시작 직전 로드 → 메뉴 복귀 시 언로드 (GamePlayServices와 구분되는 콘텐츠 세션 전용 서비스 씬)
- DialogueService: `UiService.Instance` 패턴. 언로드 시 Instance=null로 생명주기 경계를 명확히 함
- 대화 UI: `UiService.Instance.Overlay`로 표시 → Zone 전환과 무관하게 유지
- **AQ-2 답**: 진행 상태가 ContentServices 수명에 종속되므로 Zone 전환(=Zone 씬 unload/load)에 영향받지 않음 → 지속성 확보
- **AQ-3 답**: 내러티브 상태 소유권을 Mode가 아닌 콘텐츠 서비스 계층에 둠. Mode 스택은 표현만, 진행 상태는 서비스가 보유 → 책임 분리된 공존

**폐기된 방향**:

- Candidate A(StoryMode 직접 소유) — Mode가 생성한 GameObject의 소속 씬을 비-Zone 씬으로 강제하는 우회 필요 + 내러티브 상태를 Mode에 묶으면 콘텐츠 세션 생명주기와 어긋남
- Candidate B(DontDestroyOnLoad 예외) — ZoneFlow 핵심 제약 정면 위반

**후속 Feature 후보**: **이슈 #49 "ContentServices 씬 + YarnSpinner DialogueService 구현"** (이미 생성됨, OPEN). 구현 항목: YarnSpinner 패키지 설치 / ContentServices 씬 / DialogueService.cs / DialoguePanel.cs / StoryMode.cs 연동 / 검증용 Yarn 스크립트(intro·zone_a).

**CLAUDE.md 반영 필요**: 없음. 단, "콘텐츠 세션 전용 서비스 씬(ContentServices)" 개념이 GamePlayServices와 별개 계층으로 자리잡으면, 추후 `docs/architecture/`의 씬 계층 문서에 ContentServices 계층을 추가 검토. (이슈 #49 구현 완료 후)

---

## 비고 — 후보 라벨 정정

본 exploration의 candidates.md는 이전 대화의 실제 결정(이슈 #49)을 모른 채 `~/.claude/plans/declarative-sniffing-lake.md`만 보고 라벨링되어, 초기 라벨(A=StoryMode 스코프)과 실제 채택(C=서비스 스코프)이 어긋나 있었다. ground truth는 이슈 #49이며, 위 결론은 그에 맞춰 정정됨.
