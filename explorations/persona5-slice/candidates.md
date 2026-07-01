# 후보 방향들

## Candidate 1: 시간/저장/파티 = Service (Mode 아님)

**상태**: active (플랜 기본 가정 — 검증 완료: **채택 권고**)

**주장**: 시간·세이브·파티는 Zone/Mode 전환과 직교하는 게임 전역 상태이므로
CoreServices 상주 Service(ServiceLocator 등록)로 둔다. 날짜 진행은 Mode 전환 이벤트가
아니라 서비스 API 호출로 일으킨다.

**검증 질문**: TimeService.AdvanceDay()를 누가 호출하는가? 루프 배선에서 Mode 종료
콜백이 호출한다면 사실상 Mode에 결합되는 것 아닌가? → architecture-director 판단 필요.

**architecture-director 권고 (2026-07-01)**: **Service로 두는 판단은 옳다.** 이유 —
`GamePlayMode`(GamePlayMode.cs)는 8단계 생명주기(Created→…→Destroyed)를 가진 *전환 가능한*
객체로, Zone 로드/스폰/HUD에만 관심을 둔다. 시간·파티·세이브는 그 전환을 가로질러 **한 번만
존재**해야 하는 상태다. 이를 Mode에 두면 ReplaceAll이나 Pop 시 상태가 파괴되어(StoppedAsync가
Zone을 Release하듯) 전역 상태가 날아간다. constraints.md의 "서비스=씬 배치, 참조만" 원칙과도
정합 — TimeService/PartyService/SaveService는 `CoreServices.unity`(또는 신설
`SimServices.unity`)에 GameObject로 배치하고 `MonoService<T>`로 등록한다.

**"AdvanceDay를 Mode 종료 콜백이 호출하면 결합 아닌가"에 대한 답 — 결합 아님**: 호출 *주체*와
상태 *소유자*를 분리하면 직교가 유지된다. AdvanceDay는 **Mode가 아니라 일과-선택 패널의
사용자 액션 핸들러**(PanelMode 위의 UiPanel 콜백)가 `TimeService.Instance.AdvanceDay()`를
직접 호출한다. Mode 생명주기 훅(OnStoppedAsync 등)에서는 절대 호출하지 않는다 — 그러면
"던전에서 Pop해서 나올 때마다 하루가 지나는" 암묵 결합이 생긴다. **불변식**: 시간 진행은
플레이어의 명시적 커밋 액션에서만 발생하고, Mode 전환은 시간에 대해 read-only다.

→ 상세: [[AQ-time-progression]] (아래 discussion.md 및 최종 응답 참조)

---

## Candidate 2: Save/Load 스냅샷 권위 계층 (AQ-5)

**상태**: active — **후보 B + C-부분복원 권고**

**문제**: Mode 스택 + Zone 상태 + Yarn 변수 + 파티/시간을 하나의 세이브로 묶어야 함.
어느 계층이 스냅샷을 조립·복원하는가?

**후보**:
- A) SaveService가 각 Service(Time·Party·Dialogue)와 GamePlayDirector(Mode 스택·현재
  Zone)에서 상태를 수집하는 aggregator.
- B) 각 Service가 `ISaveable` 구현, SaveService는 등록된 saveable 순회.
- C) 복원 시 Mode 스택을 그대로 재구성 vs "현재 Zone + 진입 Mode"만 복원하고 스택은 초기화.

**architecture-director 권고 (2026-07-01)**:

**조립 계층 = B (`ISaveable` 순회) 채택.** 이유 — A(aggregator)는 SaveService가 Time·Party·
Dialogue·Director의 내부 상태 형태를 모두 알아야 해 역방향 의존이 폭발한다. 새 시뮬 시스템
(인벤·장비·사회링크)이 늘 때마다 SaveService를 수정해야 하는 반면, B는 신규 Service가
`ISaveable`만 구현하면 자동 편입된다 — constraints.md의 "서비스는 참조만, 생성은 씬" 원칙과
정합하며 개방-폐쇄를 지킨다.

**복원 정책 = C 중 "현재 Zone + 진입 Mode만 복원, 스택은 초기화" 채택.** 이유 — Mode 스택을
그대로 재구성(overlay PanelMode, BattleMode 중첩 등)하려면 각 Mode의 CreatedAsync→PlayedAsync를
순서대로 재생하며 Slept였던 하위 Zone까지 되살려야 하는데, 이는 세이브 시점의 전환 중간
상태(_isNavigating true 구간 등)를 재현할 수 없어 깨지기 쉽다. **세이브는 "안정 상태(Active
Mode)"에서만 허용**하고, 복원은 `NavigateAsync(진입 URI, ReplaceAll)` 한 방으로 현재 Zone +
진입 Mode를 세운다. 스택 깊이는 버린다 — 전투/패널 중간에 세이브 자체를 금지(일과-선택·아지트
같은 안정 지점에서만 세이브)하면 정보 손실이 없다. 이는 콘솔 JRPG의 세이브 포인트 계약과 동일.

**스냅샷 권위 계층**: `GamePlayDirector`는 "현재 진입 URI(현 Zone + Active Mode host +
SpawnPointId)"만 `ISaveable`로 노출한다. Mode 스택 내부 리스트(`_stack`)는 세이브 대상이 아니다.
Yarn 변수는 DialogueService가 `ISaveable`로 자기 VariableStorage를 직렬화(이미 TryGet/Set
float·string 접근자 보유 — DialogueService.cs 55-69행). 시간·파티는 각 Service가 자기 POCO 직렬화.

→ 상세: [[AQ-5-save-restore]]

---

## Candidate 3: BattleMode 종료 → 복귀 계약

**상태**: active — **후보 C(공유 결과 컨텍스트) 권고, A는 기각**

**문제**: BattleMode는 stack switch로 push되어 직전 모드가 Slept됨. 종료 시 Pop으로
Resume하는데, 승/패/이탈 결과를 어떻게 직전 스택에 전달하는가?

**후보**:
- A) Navigation URI `gameplay://pop?result=win` 형태로 결과 파라미터 전달.
- B) BattleService가 결과를 보관, Resume된 모드가 조회.
- C) Mode 간 결과 전달 채널(공유 컨텍스트) 신설.

**architecture-director 권고 (2026-07-01)**:

**현 코드 확인 결과** — StackAsync(GamePlayDirector.cs 151-166)로 push 시 BattleMode의
`IsOverlay=false`이므로 직전 Exploration Zone은 `SetActive(false)`로 잠들고, BattleMode는
자기 battle ZoneAsset을 AcquireAsync한다(Zone 스왑, 오버레이 아님). Pop 시
PopAsync(196-215)가 battle Zone을 Release→직전 모드 ResumedAsync가 Exploration Zone 재활성 +
저장 위치 재스폰. **골격은 이미 승/패 복귀를 지지한다. 유일한 결손은 "결과 전달 채널"이다.**

**A(URI 파라미터) 기각**: 이유 — `NavigationRequest.Parse`의 pop 분기(NavigationRequest.cs
63-67행)는 쿼리를 버리고 `(Pop, default, null, null)`을 만든다. pop에 result를 실으려면
파서·요청 구조체·PopAsync 시그니처를 모두 확장해야 하고, "결과"는 승/패 bool을 넘어 획득
아이템·소모 HP·도주 여부 등 **구조화된 페이로드**로 자란다. URI 문자열에 이걸 싣는 건
타입 안전성을 버리는 것이다. Navigation URI는 "어디로 갈지"를 표현하는 계층이지 "무슨 일이
있었는지"를 나르는 계층이 아니다 — 관심사 분리 위반.

**C(공유 결과 컨텍스트) 채택**: 신설 `BattleService`(CoreServices/SimServices 상주)가
`BattleOutcome`(win/lose/fled + 페이로드) POCO를 보관한다. BattleMode는 전투 종료 시
`BattleService.Instance.SetOutcome(...)` 후 `NavigateAsync(pop)`. 직전 모드의
`OnResumedAsync`가 `BattleService.Instance.ConsumeOutcome()`로 1회성 조회·소비한다. B와
사실상 같으나, **결과 소유자를 "전투 전용 BattleService"가 아니라 "모드 간 결과 채널"로
일반화**하는 게 핵심 — 미래에 미니게임·심문 등 다른 push/pop 서브모드가 생겨도 같은 채널을
재사용한다. `OnResumedAsync`가 파라미터를 받지 않으므로(GamePlayMode.cs 143행) 조회는 Resume된
모드가 Service에서 pull하는 방식이어야 한다(Director가 push하지 않음) — 이 pull 모델이
GamePlayMode 훅 시그니처를 건드리지 않는 최소 변경.

**미해결 하위질문**: 패배 시 Pop이 아니라 아지트로 ReplaceAll(게임오버 복귀)해야 할 수도 있다
→ 결과에 따라 복귀 *경로*가 갈린다. "누가 이 분기를 결정하나(BattleMode vs Resume된 모드)"는
결정 포인트로 승격.

→ 상세: [[AQ-battle-return-contract]]

---

## Candidate 4: 수직 슬라이스 MVP 경계

**상태**: active — 확정

**In**: 허브 Zone 1(아지트) + 팰리스 Zone 1, 일과-선택 패널, 기본 턴제 전투 1종,
날짜 진행 1일, Save/Reload 1회.

**Out**: 코옵/사회링크, 페르소나 합체, 속성 상성, 다중 팰리스, 인벤/장비 심화.

**architecture-director 확정 매핑표 (2026-07-01)**:

| P5 시스템 | 계층 | 구현체 | 근거 |
| --- | --- | --- | --- |
| 아지트(허브) | Zone + ShellMode | 신규 Zone 씬 + 기존 ShellMode | ShellMode는 Zone=환경 자체(needSpawn=false), 허브에 최적 |
| 팰리스(던전) | Zone + ExplorationMode | 신규 Zone 씬 + 기존 ExplorationMode | 자유 탐색 + Portal로 전투 트리거 |
| 턴제 전투 | BattleMode + BattleService | BattleMode 확장 + 신규 Service | Mode=전투 진입/연출, Service=턴 로직·결과 |
| 시간/캘린더 | Service | 신규 TimeService | Mode 직교 전역 상태 (Candidate 1) |
| 파티/페르소나 | Service | 신규 PartyService | Mode 직교, 전투가 참조 |
| 스탯·성장 | Service(파티에 종속) | PartyService 내부 or PlayerStats 확장 | PlayerStats(POCO) 이미 존재, 파티원으로 확장 |
| 인벤·장비 | Service | 신규 InventoryService (MVP는 스텁) | Mode 직교 |
| 세이브/로드 | Service + ISaveable | 신규 SaveService (Candidate 2) | aggregator 아닌 순회 |
| 사회링크 | (Out of scope) | — | MVP 밖 |
| 일과-선택 UI | PanelMode + UiPanel | 기존 PanelMode | 오버레이, 액션 핸들러가 AdvanceDay 호출 |

**전투는 Zone이 필요한가?**: MVP 권고 — **BattleMode에 별도 battle ZoneAsset 부여**(팰리스와
분리된 전투 아레나 씬). 이유 — StackAsync가 직전 Zone을 SetActive(false)하므로, battle Zone
없이 같은 팰리스 Zone에서 싸우려면 needSpawn·Zone 유지 로직이 특수 케이스가 된다. MVP는
"팰리스에서 Portal 밟음 → battle Zone으로 stack push → Pop으로 팰리스 복귀"가 골격과 마찰
없이 흐른다. (심리스 인카운터는 out-of-scope 심화 백로그.)
