# 탐색 결과

**결론**: 페르소나 5형 이중 루프(사회 시뮬 ↔ 던전 전투)의 수직 슬라이스는 Zone-Mode 분리
골격을 깨지 않고 성립한다. 골격(GamePlayDirector stack push/pop)은 "던전→전투→복귀" 루프를
코드 변경 없이 이미 지지하며, **유일한 결손은 "전투 결과 전달 채널"뿐**이다. P5 시스템은
Zone(공간)·Mode(행동)·Service(전역 시뮬 상태) 3계층에 명확히 매핑된다.

**채택된 방향**:
- **시뮬 상태(시간·파티·세이브·인벤) = Service 계층** — Mode 전환에 직교. 시간 진행은
  일과-선택 패널의 명시적 커밋 액션에서만. → [ADR-0001](../../docs/decisions/0001-sim-state-in-service-layer.md) (AQ-7)
- **전투 복귀 = 모드 간 결과 채널(BattleService pull)** — Navigation URI 파라미터 기각.
  패배=아지트 ReplaceAll(게임오버), 승리=팰리스 Pop. → [ADR-0002](../../docs/decisions/0002-battle-return-result-channel.md) (AQ-8)
- **Save/Load = ISaveable 순회 + 안정 상태(아지트·일과선택) 세이브 + 부분 복원**(현재 Zone +
  진입 Mode만, 스택 깊이 버림). → [ADR-0003](../../docs/decisions/0003-save-load-isaveable-stable-state.md) (AQ-5)
- **전투 = 별도 아레나 Zone**(stack push→pop), 팰리스 인플레이스 전투는 심화 백로그.

**MVP 경계 (확정)**:
- In: 허브 Zone 1(아지트, ShellMode) + 팰리스 Zone 1(ExplorationMode) + 아레나 Zone(BattleMode),
  일과-선택 패널, 기본 턴제 전투 1종(승/패), 날짜 진행 1일, Save/Reload 1회.
- Out: 코옵/사회링크, 페르소나 합체, 약점→원모어→총공격·속성 상성, 다중 팰리스, 인벤/장비 심화.

**폐기된 방향**:
- 시뮬 상태를 Mode에 보유 — ReplaceAll/Pop에서 전역 상태 파괴, 경계 흐림.
- `gameplay://pop?result=` URI 파라미터 — pop 파서가 쿼리 폐기, 관심사 분리 위반.
- Mode 스택 전체 재구성 복원 — 전환 중간 상태(`_isNavigating`) 재현 불가.
- battle 인플레이스 전투 — StackAsync의 Zone SetActive(false)와 특수 케이스 마찰(심화로 이월).

**후속 Feature 후보** (2축, 별개 트랙 — 전투는 시간/세이브에 비의존이라 먼저 독립 검증):
- combat 축 (→ combat-specialist): `combat-battle-service`, `combat-turn-loop-ui`
- systems 축 (→ systems-designer): `systems-time-calendar`, `systems-party-stats`, `systems-save-load`

**CLAUDE.md / constraints 반영 필요**:
- constraints.md에 "시뮬 전역 상태 = Service 계층, 시간 진행은 명시적 커밋 액션에서만"(ADR-0001) 반영 후보.
- Phase 1에서 `combat-specialist`·`systems-designer` 에이전트 + `combat-code.md` rule + `/battle`·`/systems`
  커맨드를 신설 (세션 재시작 후 로드).
