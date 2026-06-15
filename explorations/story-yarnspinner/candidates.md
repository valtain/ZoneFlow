# 후보 방향들

> 핵심 결정 축: **DialogueRunner GameObject를 어디에 두는가** (Zone 전환 중 생존 보장 + DontDestroyOnLoad 금지 제약 준수)
>
> **확정 (이슈 #49)**: 아래 Candidate C의 발전형 — **전용 ContentServices 씬 + DialogueService(MonoService)** 방식으로 결정. Candidate A(StoryMode 직접 소유)는 폐기.

---

## Candidate A — StoryMode 스코프 소유

**상태**: eliminated — Mode가 생성한 GameObject의 소속 씬을 비-Zone 씬으로 강제해야 하는 우회가 필요하고, 내러티브 상태를 Mode에 묶으면 ContentServices 단위의 생명주기(콘텐츠 진입~메뉴 복귀)와 어긋남. 서비스 스코프(C)가 더 단순. (이슈 #49에서 불채택)

StoryMode가 `OnPlayedAsync()`에서 DialogueRunner를 담은 GameObject를 직접 생성하고,
`OnStoppedAsync()`에서 파괴. StoryMode가 Mode 스택에 살아있는 동안 Yarn 변수·
대화 흐름이 유지된다.

```
OnPlayedAsync()   → StoryDialogueController GameObject 생성 + DialoguePanel 로드
OnSleptAsync()    → PauseDialogue()  (Zone 전환 전)
OnResumedAsync()  → ResumeDialogue() (Zone 전환 후)
OnStoppedAsync()  → Controller 파괴 + DialoguePanel 정리
```

**장점**: 씬=서비스 생성 책임 원칙과 정합, Mode 생명주기에 자연히 종속,
DontDestroyOnLoad 불필요
**미검증 우려**: 이 GameObject가 어느 씬에 속하는가? Mode가 생성한 GameObject가
Zone 씬에 instantiate되면 Zone unload 시 같이 파괴됨 → **소속 씬을 명시해야 함**
(CoreServices/GamePlayServices 씬 등 비-Zone 씬에 부모를 둬야 생존)

---

## Candidate B — DontDestroyOnLoad 예외

**상태**: eliminated

DialogueRunner GameObject에 DontDestroyOnLoad 적용. ZoneFlow 핵심 제약
("DontDestroyOnLoad 금지")을 정면 위반 → 제거. 원본 계획 주의사항에서 거론되었으나
제약과 충돌.

---

## Candidate C — 서비스 스코프 (DialogueService) — **채택 (이슈 #49)**

**상태**: promoted → 이슈 #49

DialogueRunner를 서비스로 호스팅. 단, 원안의 "GamePlayServices에 Attach"가 아니라
**전용 ContentServices 씬을 신설**하는 형태로 확정됨.

### 확정 설계 (이슈 #49)

- **ContentServices 씬**: 콘텐츠 플레이 시작 직전 로드 → 메뉴 복귀 시 언로드.
  GamePlayServices(상시)와 구분되는, 콘텐츠 세션 수명의 전용 서비스 씬.
- **DialogueService**: `UiService.Instance` 패턴 동일. ContentServices 언로드 시
  Instance = null → 접근하면 NRE 자연 발생 (의도된 생명주기 경계).
- **대화 UI**: `UiService.Instance.Overlay` 패턴으로 표시 → Zone 전환과 무관하게 유지.

**채택 이유**: Update 보장(씬 상주), 비-Zone 씬이라 Zone 전환에 안전, 콘텐츠
세션 단위 생명주기가 명확(로드/언로드 경계 = 진행 상태 수명). DontDestroyOnLoad 불요.

**AQ-3에 대한 답**: 내러티브 상태 소유권을 Mode가 아닌 콘텐츠 서비스 계층에 둔다.
Mode 스택은 표현(Story HUD/Overlay)만 담당하고, 진행 상태(Yarn 변수)는
ContentServices 수명에 종속 → Mode와 내러티브가 책임 분리된 채 공존.

---

## 결정 보조 — A vs C의 본질

```
A: 내러티브 상태 = Mode 스코프  → Mode 독립성 강조, 단 GameObject 소속 씬 주의
C: 내러티브 상태 = Service 스코프 → 생존·Update 안전, 단 상태 위치가 Mode 밖
```

두 후보 모두 "GameObject가 비-Zone 씬에 상주해야 Zone 전환에서 생존"이라는
공통 제약을 공유. 차이는 **상태 소유권을 Mode에 둘지 Service에 둘지**.
이 선택이 곧 AQ-3에 대한 ZoneFlow의 답이 된다 → 본 exploration의 핵심 결정 지점.

---

## 구현 단계 초안 (생명주기 결정 후 적용 — 원본 계획에서 이관)

| 단계 | 내용 |
| --- | --- |
| 1 | YarnSpinner 패키지 설치 (UPM git URL `...#current`, `Packages/manifest.json`) |
| 2 | `DialoguePanel.cs` (UiPanel 상속, LineView/OptionsView 연결, PrimeTween 애니 재사용) — `Runtime/Ui/Panels/` |
| 3 | `StoryDialogueController.cs` (DialogueRunner + InMemoryVariableStorage 보유, StartNode/Pause/Resume, `[YarnCommand("zone_enter")]`) — `Runtime/GamePlay/Story/` |
| 4 | `StoryMode.cs` 수정 (OnPlayed/Slept/Resumed/Stopped 훅 연동) — `Runtime/GamePlay/ModeImpl/` |
| 5 | 검증용 Yarn 스크립트 `intro.yarn` / `zone_a.yarn` (`$story_progress` 지속성) — `Story/Scripts/` |

## 검증 시나리오 (AQ-2/AQ-3)

1. Story 진입 → `intro.yarn` 시작
2. 대화 중 Zone 전환 → Pause → Zone 로드
3. Zone 로드 완료 → Resume 또는 `zone_a.yarn` 노드 진입
4. `$story_progress`가 Zone 전환 전후 동일 → **AQ-2 증명**
5. Mode 스택 Pop → StoryMode Stopped → DialogueRunner 파괴 확인 → **AQ-3 증명**
