---
paths:
  - "Assets/ZoneFlowAssets/Runtime/GamePlay/Battle/**"
  - "Assets/ZoneFlowAssets/Runtime/GamePlay/ModeImpl/BattleMode.cs"
---

# Rule: 전투 코드

전투 관련 파일을 Edit/Write 하기 전에 이 규칙을 적용한다. `Runtime/**`이므로
[.claude/rules/runtime-code.md](runtime-code.md)와 **동시 매칭**된다 — 그쪽 규칙(UniTask 전용,
public 필드 금지, `Debug.Assert`, 서비스=씬 생성)도 함께 지킨다.
원문(canonical): [docs/decisions/0002-battle-return-result-channel.md](../../docs/decisions/0002-battle-return-result-channel.md).

## 필수

- **전투 결과는 모드 간 결과 채널로만 전달(ADR-0002)** — 전투 종료 시
  `BattleService.SetOutcome(BattleOutcome)` 후 `NavigateAsync(pop)`. 직전 모드의
  `OnResumedAsync`가 `ConsumeOutcome()`로 1회 pull·소비한다. **`gameplay://pop?result=`처럼
  Navigation URI에 결과 페이로드를 싣지 않는다** — URI는 "어디로"지 "무슨 일"이 아니다.
- **복귀 경로 분기는 Resume된 모드가 결정** — 패배=아지트 `ReplaceAll`(게임오버), 승리=팰리스
  `Pop`. BattleMode는 결과만 기록하고 목적지를 정하지 않는다.
- **BattleService는 CoreServices 상주 `MonoService<T>`** — 코드가 GameObject를 만들어
  생성하지 않는다(씬 배치). `DontDestroyOnLoad` 미사용.
- **전투는 상태를 소유하지 않는다** — 파티/스탯/시간/세이브는 `PartyService`·`TimeService`·
  `SaveService`(systems-designer 소유)에서 **읽기만** 한다. 전투 결과(획득·소모)는 채널로 통지.
- **턴 로직은 결정론적·검증 가능** — 턴 순서 큐·데미지 계산은 순수 함수로 분리해 EditMode
  테스트가 가능하게 한다(난수는 주입 가능한 시드).
- **전투 데이터는 ScriptableObject** — `SkillAsset`·`PersonaAsset`·`EnemyAsset`은
  `Runtime/Data`에 두고 [.claude/rules/scriptable-data.md](scriptable-data.md)를 따른다.
- **전투 아레나는 별도 Zone** — `SceneType.Zone`. StackAsync로 push, Pop으로 복귀(팰리스
  인플레이스 전투는 심화 백로그, MVP 아님).

## 모호하면

전투 복귀 계약·Mode 스택 경계·새 서브모드(미니게임 등) 결과 채널 확장이 불분명하면 멈추고
`architecture-director` 검토를 권한다. 파티/스탯/세이브 데이터 스키마가 필요하면
`systems-designer`로 에스컬레이션한다.
