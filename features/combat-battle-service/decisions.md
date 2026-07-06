# combat-battle-service — 설계 결정

Source: [persona5-slice](../../explorations/persona5-slice/findings.md)

## 고유 구현 결정

| 결정 | 이유 |
| --- | --- |
| 엔진을 MonoBehaviour 무의존 순수 C#로 구현 (`Runtime/GamePlay/Battle/`) | EditMode에서 씬 없이 결정론을 단언하기 위함. combat-code.md의 "순서 큐·데미지 = 순수 함수" 강제를 충족 |
| RNG는 주입식 경량 LCG(`BattleRng`), 시드는 `BattleSetup`에 실어 주입 | 결정론 재현. 테스트가 시드를 고정하면 트랜스크립트가 완전 일치 |
| 결과 채널을 순수 `BattleOutcomeChannel`로 분리하고 `BattleService`가 위임 | ADR-0002 계약을 씬 없이 EditMode로 검증 가능하게 함. MonoService는 얇은 래퍼 |
| 엔진은 전투원(`Combatant`)을 주입받고 `PartyService`에 하드 의존하지 않음 | 시뮬 축(파티/시간/세이브) 미구현 상태에서 전투 축을 독립 검증. BattleMode가 임시 직렬화 조우(`BattleEncounterAsset`)를 먹임 |
| SO→POCO 변환을 `CombatantFactory` 한 곳으로 국한 | 데이터 계층과 엔진 계층 경계를 1지점으로 좁혀 결합도 최소화 |
| 이번 슬라이스 메커닉은 최소(턴 큐·기본공격·단일 스킬·승패)로 한정 | 약점/1More/상태이상/트레이트는 각각 독립 검증 대상. 엔진 골격을 먼저 못 박고 후속 슬라이스에서 얹음 |
| BattleMode는 헤드리스 auto-policy로 종료까지 구동해 결과만 산출 | 이번 슬라이스는 UI가 없으므로 명령 입력 대신 결정론 정책으로 종결. 실제 명령 UI는 `combat-turn-loop-ui`가 `SubmitAction` 표면을 구동 |
| 패배 복귀 목적지는 현재 허브 `gameplay://exploration/village?switch=replaceall` | ADR-0002의 "아지트"는 아직 미구현 — 현행 허브가 village(overworld-hub)라 이를 게임오버 복귀지로 사용. P5 아지트 도입 시 그 진입 URI로 교체 |

## persona5-slice에서 상속받은 결정

(findings.md / ADR에서 이미 정의된 범위·아키텍처 결정은 중복 제외)
- **ADR-0001** — 시뮬 전역 상태는 Service 계층. 전투 결과 채널도 Mode가 아닌 `BattleService`가 보유.
- **ADR-0002** — 결과는 `SetOutcome`→`NavigateAsync(pop)`, 직전 모드 `OnResumedAsync`가 `ConsumeOutcome` 1회 pull. Navigation URI에 결과 미탑재.
- **ADR-0003** — 세이브는 안정 상태에서만. 전투 중 세이브 없음(이번 슬라이스는 세이브 비연동).
- 전투 축을 `combat-battle-service`(엔진) / `combat-turn-loop-ui`(UI) 2개로 분할, 엔진 우선.
