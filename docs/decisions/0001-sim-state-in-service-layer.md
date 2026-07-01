# ADR-0001: 시뮬 상태(시간·파티·세이브·인벤)는 Service 계층에 둔다

- **상태**: 채택
- **날짜**: 2026-07-01
- **관련 AQ**: AQ-7 (시간 진행)

## 맥락

프로젝트를 페르소나 5형 게임으로 피벗하며 RPG 시뮬 레이어(시간/캘린더, 파티/페르소나,
세이브/로드, 인벤/장비)를 추가한다. 이 상태들을 Zone-Mode 어느 계층에 둘지 결정해야 한다.
특히 "날짜 진행(AdvanceDay)을 Mode 종료 콜백이 호출하면 사실상 Mode에 결합되는 것 아닌가"가
쟁점이었다. Zone-Mode 분리와 서비스 계층 경계에 닿는 결정이다.

## 결정

**시간·파티·세이브·인벤은 CoreServices 상주 Service(`MonoService<T>`, 씬 배치)로 둔다.**
이유 — `GamePlayMode`는 8단계 생명주기(Created→…→Destroyed)를 가진 *전환 가능한* 객체라,
전역 상태를 담으면 ReplaceAll/Pop 전환에서 파괴된다. 시뮬 상태는 그 전환을 가로질러 한 번만
존재해야 하므로 Mode와 직교하는 Service가 유일하게 안전한 자리다.

**불변식(시간 진행)**: `TimeService.AdvanceDay()`는 **일과-선택 패널의 사용자 액션 핸들러**만
호출한다. Mode 생명주기 훅(OnStoppedAsync 등)에서는 절대 호출하지 않는다 — 그러면 "던전에서
나올 때마다 하루가 지나는" 암묵 결합이 생긴다. Mode 전환은 시간에 대해 read-only.

## 고려한 대안

| 대안 | 장점 | 단점 / 탈락 이유 |
| --- | --- | --- |
| A (채택) 시뮬 = Service, 시간 진행 = 명시적 커밋 액션 | 전환에 불변, constraints "서비스=씬 배치·참조만" 정합, 직교 유지 | 없음 (호출 주체/소유자 분리로 결합 우려 해소) |
| B 시뮬 상태를 Mode에 보유 | Mode 로컬 접근 간편 | ReplaceAll/Pop에서 전역 상태 파괴, Zone-Mode 경계 흐림 |
| C 시간 진행을 Mode 종료 훅에서 호출 | 배선 단순 | "Pop=하루 경과" 암묵 결합, 시간이 Mode 전환에 종속 |

## 결과

- **강제**: 신규 `TimeService`·`PartyService`·`SaveService`·`InventoryService`는 씬 배치 +
  `MonoService<T>` 등록. DontDestroyOnLoad 사용 금지(CoreServices 상주로 충족).
- **금지**: Mode 생명주기 훅에서 시간 진행 호출.
- **constraints.md 반영 후보**: "시뮬 전역 상태는 Service 계층, 시간 진행은 명시적 커밋
  액션에서만" 원칙 추가.
- 관련 결정: [ADR-0002](0002-battle-return-result-channel.md), [ADR-0003](0003-save-load-isaveable-stable-state.md).
