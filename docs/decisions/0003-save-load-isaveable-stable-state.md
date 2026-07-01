# ADR-0003: Save/Load는 ISaveable 순회 + 안정 상태 세이브 + 부분 복원

- **상태**: 채택
- **날짜**: 2026-07-01
- **관련 AQ**: AQ-5 (Save/Load과 Zone-Mode 상태 상호작용)

## 맥락

세이브는 Mode 스택 + 현재 Zone + Yarn 변수 + 파티/시간을 하나로 묶어야 한다. 어느 계층이
스냅샷을 조립·복원하며, Mode 스택을 얼마나 복원하는가. Zone-Mode 전환의 중간 상태
(`_isNavigating` 구간)를 재현할 수 있는가가 쟁점이다.

## 결정

**조립 = `ISaveable` 순회.** 각 Service(Time·Party·Dialogue)와 `GamePlayDirector`가
`ISaveable`을 구현하고 SaveService는 등록된 saveable을 순회한다. 이유 — aggregator는 SaveService가
모든 내부 상태 형태를 알아야 해 역방향 의존이 폭발하지만, 순회는 신규 시뮬 시스템이 `ISaveable`만
구현하면 자동 편입돼 개방-폐쇄를 지킨다.

**세이브 시점 = 안정 상태에서만** (아지트 + 일과-선택 패널). 전투/패널 전환 중간 세이브 금지.
**복원 = "현재 Zone + 진입 Mode만 `NavigateAsync(진입 URI, ReplaceAll)`로 복원, `_stack` 깊이는
버림".** 이유 — Mode 스택 전체 재구성은 전환 중간 상태를 재현할 수 없어 깨지기 쉽다. 안정 지점
세이브면 스택 깊이 정보가 애초에 없으므로 손실이 없다(콘솔 JRPG 세이브포인트 계약과 동일).

## 고려한 대안

| 대안 | 장점 | 단점 / 탈락 이유 |
| --- | --- | --- |
| A (채택) ISaveable 순회 + 안정상태 세이브 + 부분복원 | 개방-폐쇄, 전환 중간상태 회피, JRPG 계약 | 임의 시점 세이브 불가(의도된 제약) |
| B aggregator가 각 상태 수집 | 중앙 집중 | 신규 시스템마다 SaveService 수정, 역방향 의존 폭발 |
| C Mode 스택 전체 재구성 복원 | 어떤 시점이든 복원 | `_isNavigating` 중간상태 재현 불가, 깨지기 쉬움 |

## 결과

- **강제**: `ISaveable` 인터페이스 신설. `GamePlayDirector`는 "진입 URI(현 Zone + Active Mode
  host + SpawnPointId)"만 노출, `_stack`은 세이브 대상 아님. `DialogueService`는 자기
  VariableStorage 직렬화(TryGet/Set float·string 접근자 이미 보유, DialogueService.cs 55-69).
- **금지**: 전환 중(비-Active Mode)에 세이브. Mode 스택 깊이 직렬화.
- **MVP 안정 지점**: 아지트, 일과-선택 패널.
- 관련 결정: [ADR-0001](0001-sim-state-in-service-layer.md), [ADR-0002](0002-battle-return-result-channel.md).
