# ADR-0004: 전투 턴 해결은 결정론적 헤드리스 엔진으로 구현한다

- **상태**: 채택
- **날짜**: 2026-07-03
- **관련 AQ**: AQ-8 (전투 복귀 계약, ADR-0002와 연속)

## 맥락

던전 턴제 전투의 로직 코어를 세워야 한다. 전투는 시간/세이브/파티 시뮬 축과 독립적이라 먼저 검증할 수 있는데, 두 가지가 흔들리면 이후 콘텐츠·UI 저작이 통째로 흔들린다.

- **재현성**: 턴 순서·데미지가 프레임/호출 순서나 숨은 전역 난수에 의존하면 버그를 재현할 수 없고 자동 검증이 불가능하다.
- **검증 위치**: 전투 로직이 MonoBehaviour·씬·코루틴에 묶이면 PlayMode를 띄워야만 돌아가 반복 비용이 크다.

이 결정은 서비스 계층(ADR-0001)·전투 결과 채널(ADR-0002)에 맞닿는다. ADR-0002가 "결과를 **어떻게 되돌리는가**"를 정했다면, 이 ADR은 그 결과를 만들어내는 **엔진의 형태**를 정한다.

## 결정

**전투 턴 해결을 MonoBehaviour 무의존 순수 C# 엔진으로 구현하고, 턴 순서·데미지를 주입 시드 기반 순수 함수로 분리해 EditMode에서 검증한다.**

- **결정론 코어**: 턴 순서(`TurnOrder`)와 데미지(`DamageCalculator`)는 입력만으로 결과가 정해지는 순수 함수다. 난수는 전역 `UnityEngine.Random`이 아니라 `BattleSetup`에 실려 주입되는 시드 RNG(`BattleRng`, 경량 LCG)에서만 나온다. 같은 시드·같은 입력이면 트랜스크립트가 완전히 일치한다.
- **헤드리스 엔진**: `BattleEngine`은 `UnityEngine` 표현 계층을 참조하지 않는다. 공개 표면은 `State`(`Ongoing|PlayerWon|PlayerLost`), `Current`(현재 행동자), `SubmitAction(BattleAction)→ActionResult`. 호출자가 액션을 밀어넣으면 엔진이 결정론적으로 적용·사망 판정·다음 행동자 전진을 수행한다. 이 표면을 후속 전투 UI가 그대로 구동한다.
- **데이터 = ScriptableObject → 개시 시 POCO 변환**: 전투원·스킬은 `SkillAsset`·`PersonaAsset`·`EnemyAsset`으로 저작하고, 전투 개시 시 `CombatantFactory`가 런타임 POCO(`Combatant`)로 한 번 변환한다. SO↔엔진 경계를 팩토리 한 곳으로 좁힌다.
- **엔진은 시뮬 축에 비의존**: `BattleEngine`은 전투원을 **주입받는다**. `PartyService`/`TimeService`/`SaveService`에 하드 의존하지 않으며, 이들 상태는 (연동 시에도) 읽기 전용이다. 시뮬 축이 미구현이어도 전투 축을 독립 검증할 수 있다.

이유 — 결정론+헤드리스라야 전투 로직을 씬 없이 EditMode로 못 박을 수 있고, 버그가 시드로 재현되어 회귀가 값싸진다. 데이터를 SO로 저작하고 엔진을 POCO로 돌리면 저작 편의(인스펙터)와 검증 편의(순수 함수)를 동시에 얻는다.

## 고려한 대안

| 대안 | 장점 | 단점 / 탈락 이유 |
| --- | --- | --- |
| A (채택) 순수 헤드리스 엔진 + 주입 시드 + SO→POCO | EditMode 결정론 검증, 시드 재현, 저작/검증 분리, UI가 동일 표면 구동 | 엔진↔데이터 변환 경계(팩토리) 1개 추가 |
| B MonoBehaviour 전투 매니저 + 코루틴 턴 루프 | 씬에 바로 얹기 쉬움 | PlayMode 필수라 검증 반복 비용 큼, `runtime-code.md`의 UniTask·순수성 원칙 위반, 결정론 확보 난망 |
| C 전역 `UnityEngine.Random` 사용 | 코드 단순 | 시드 재현 불가 → 자동 검증 불가, `combat-code.md` "주입 가능한 시드" 위반 |
| D 데이터도 POCO 하드코딩(SO 없음) | 초기 구현 최소 | 콘텐츠 저작을 코드 변경으로 강요, `scriptable-data.md` 규약 이탈, 확장 시 병목 |

## 결과

- **강제**:
  - 전투 순서·데미지 로직은 순수 함수, 난수는 주입 시드(`BattleRng`)만 사용. 전투 코어는 `UnityEngine` 표현 계층 무참조.
  - 전투 데이터는 SO(`SkillAsset`/`PersonaAsset`/`EnemyAsset`)로 저작, 개시 시 `CombatantFactory`가 POCO로 변환.
  - `BattleEngine`은 전투원을 주입받고 시뮬 서비스에 비의존(연동 시 읽기 전용).
  - 결정론은 EditMode 테스트로 상시 검증(동일 시드 → 동일 트랜스크립트).
- **금지**: 전투 로직에서 전역 `UnityEngine.Random`·`Time`·프레임 순서 의존, MonoBehaviour/코루틴 결합.
- **결과 채널**: 산출된 `BattleOutcome`은 [ADR-0002](0002-battle-return-result-channel.md)의 `BattleService` pull 계약으로 직전 모드에 전달한다.
- **이번 슬라이스 경계**: 최소 메커닉(턴 큐·기본공격·단일 스킬·승패). 약점(원소 affinity)·1 More/추가행동·상태이상·캐릭터 특성은 후속 슬라이스가 이 표면 위에 얹는다 — 새 AQ 후보로 남긴다(원소 affinity 모델·추가행동 규칙).
- 관련 결정: [ADR-0001](0001-sim-state-in-service-layer.md), [ADR-0002](0002-battle-return-result-channel.md).
