# 후보 방향들

---

## 축 1 — 구현 범위 (MVP vs 풀스펙)

### Candidate A — 시나리오 풀스펙 (Zone 3 + Mode 3 전부)

**상태**: deferred — 후속 feature로 분리. 한 번에 다 묶지 않고 MVP(B) 동작 확인 후 Story/Battle/Boss를 단계적으로 추가

Village/Dungeon/BossRoom + Exploration/Story/Battle을 시나리오 흐름 그대로 구현.
Push/Pop/Replace/ReplaceAll을 모두 데모에 노출하여 스택 제어를 충실히 증명.

**장점**: 두 명제 + 스택 제어 전체를 한 번에 보여줌
**위험**: Story 대화 + Battle 로직 + Boss 엔딩까지 미구현 볼륨이 큼

### Candidate B — MVP 우선, Story/Boss 후순위

**상태**: promoted — 1차 feature = **MVP(명제 A: Village→Dungeon + ExplorationMode)**. Story(S1)/Battle(Bt1)/Boss/연출은 후속 feature로 단계 분리. (사용자 확정)

1순위 Village+Dungeon+Exploration(명제 A) → 2순위 StoryMode 최소(NPC 대화로
명제 B의 직관적 증명) → 3순위 Battle 더미(접촉→자동승리→Pop) → 4순위 Boss+엔딩.

**근거**: MVP 3요소(Village→Dungeon 전환 / 전투→복귀 / WebGL 빌드)만으로
ZoneFlow 핵심 증명 가능. StoryMode를 시나리오의 3순위가 아닌 2순위로 앞당겨
명제 B의 가장 직관적인 증명 포인트(NPC_Elder 대화)를 조기 확보.

---

## 축 2 — StoryMode 대화 시스템 (현재 미구현)

### Candidate S1 — ScriptableObject DialogueData 최소 구현

**상태**: active (권장)

`DialogueData : ScriptableObject { string[] lines }` 하나 + StoryHudPanel에
텍스트 순차 출력 로직만 추가. 분기·조건 대화 없음. 대화 종료 콜백 → Mode Pop.

**근거**: 명제 증명에 필요한 건 "대화창이 뜨고 Mode가 Push/Pop된다"는 사실뿐.

### Candidate S2 — 외부 대화 프레임워크(Yarn/Ink) 도입

**상태**: eliminated — 데모 범위 대비 과설계, 스타일/의존성 부담

---

## 축 3 — BattleMode 게임 로직 (현재 스타터만)

### Candidate Bt1 — 더미 전투 (접촉 → 자동 승리 → Pop)

**상태**: active (권장)

Enemy AI·밸런싱 없이 "적 접촉 → BattleMode Push → 공격 입력 1회 또는 N초 후
HP 0 → Pop"으로 극단 단순화. 시나리오에서 전투 밸런싱은 이미 제외됨.

**근거**: 증명 대상은 스택 Push/Pop 동작과 Zone 상태 보존이지 전투 자체가 아님.

### Candidate Bt2 — 클릭 공격 + HP 판정

**상태**: active (선택적 상향)

Bt1 + 클릭 1회 = 데미지 1, EnemyController.TakeDamage로 HP 차감. 약간의
인터랙션을 더해 "전투처럼 보이는" 최소선. 시간 여유 시 Bt1에서 승격.

---

## 축 4 — Enemy/NPC → Navigation 연결 패턴 (결정 필요)

### Candidate N1 — IInteractable 통일 패턴 (권장)

**상태**: active (권장)

기존 `Portal.cs`가 `NavigationUri` 필드 + `OnInteractAsync()` 호출 방식이듯,
EnemyController·NpcInteractable도 `IInteractable`을 구현해 NavigationUri로 이동.
Enemy가 GamePlayDirector를 직접 참조하지 않음.

**근거**: 입력 문서의 EnemyController는 OnTriggerEnter에서 직접 BattleMode를
Push하는데, 이는 의존 방향 역전(Mode→Director 직접 참조). 기존 Portal 규약과
통일하면 의존 단방향 유지 + Primitive→실모델 교체 시 로직 컴포넌트 그대로 보존.

### Candidate N2 — Enemy가 GamePlayDirector 직접 호출

**상태**: eliminated — 의존 방향 역전, 기존 Portal 패턴과 불일치

---

## 축 5 — 작업 분담 (Claude Code vs 개발자)

### 결정 사항 (제약에서 도출)

**상태**: decided

| 담당 | 작업 |
| --- | --- |
| Claude Code | C# 스크립트(EnemyController, NpcInteractable, DialogueData, 대화 로직), `.prefab` 기본 생성, ZoneAssetCatalog/SpawnPointCatalog 등록 |
| 개발자(Unity Editor) | Low Poly Dungeon Lite 임포트, 씬 오브젝트 배치(SpawnPoint/NPC/Enemy/Portal), 머티리얼·조명 설정 |

**근거**: Unity 씬(.unity)·프리팹 바이너리/YAML 직접 편집은 .meta GUID 충돌
위험으로 비현실적. 입력 문서의 Task 2~4(씬 배치)는 "배치 지침서"로 재포맷하고
Claude는 스크립트·프리팹 제공으로 한정.

---

## 경로 정정 (입력 문서 오류 → 실제 코드베이스)

| 입력 문서 기재 | 실제 |
| --- | --- |
| `Assets/ZoneFlow/Prefabs/Enemies/` | `Assets/ZoneFlowAssets/Runtime/Prefabs/` |
| `PortalController` 신규 생성 | `Portal.cs` 이미 완성 (NavigationUri 보유) |
| `NpcInteractable` 신규 | `IInteractable` 인터페이스만 존재 → 구현체 신규 작성 |
| 씬명 `Zone_Village/Dungeon/BossRoom` | 기존 `World1/World2` 재활용 가능, ZoneId 매핑만 정의 |
