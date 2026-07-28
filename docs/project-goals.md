# Project Goals

## 학습 목표

ZoneFlow는 제품 개발이 아닌 **아키텍처 학습 프로젝트**다.  
목표: Zone-Mode 분리 아키텍처가 실제 게임 시나리오에서 어떻게 동작하는지 직접 부딪히며 증명한다.

AI-native 개발 방식 자체도 학습 대상이다 — 어떻게 하면 AI가 다음 아키텍처 질문을 먼저 발견하고 제안할 수 있는가.

## 현재 집중 영역

**Persona5형 이중 루프(사회 시뮬 ↔ 던전 턴제 전투)를 stress test vehicle로 사용한다.**

Zone-Mode 골격이 프로덕션급에 도달해, 이제 그 위에 RPG 시뮬 레이어를 얹어 실전에서 검증하는 단계다. 초기에는 Story 모드가 주 검증 도구였으나(선형 내러티브 ↔ Zone 탐색), 이제는 이중 루프 전체가 더 강한 마찰을 만든다:

- **사회 시뮬 루프** — 시간/캘린더·파티·세이브 등 시뮬 전역 상태가 Service 계층에서 어떻게 사는가 ([ADR-0001](decisions/0001-sim-state-in-service-layer.md))
- **던전 전투 루프** — BattleMode 종료 결과를 직전 스택에 전달하는 모드 간 계약 ([ADR-0002](decisions/0002-battle-return-result-channel.md))
- **두 루프의 접합** — Save/Load가 Zone-Mode 상태와 어떻게 상호작용하는가 ([ADR-0003](decisions/0003-save-load-isaveable-stable-state.md))

Story 모드는 여전히 내러티브 진행 상태 보존(Yarn `DialogueService`)의 검증 도구로 남는다.

수직 슬라이스 목표: `캘린더 1일 → 던전 → 턴제 전투 → 귀가 → 날짜 진행`. 탐색 근거는 [../explorations/persona5-slice/findings.md](../explorations/persona5-slice/findings.md) 참조.

## Open Architectural Questions

AI는 아래 질문들을 참조해 아직 탐색되지 않은 항목을 발견하고 다음 작업을 제안한다.  
답변 상태는 [BACKLOG.md](../BACKLOG.md)의 `Architectural Questions` 테이블에서 추적한다.

| # | 질문 | 상태 |
| --- | --- | --- |
| AQ-1 | Story 진행 중 Zone 강제 전환 시 Mode 스택 상태는 어떻게 되는가? | ❓ Open |
| AQ-2 | Story 진행 상태(어느 챕터까지 봤는가)는 Zone 전환 후에도 유지되는가? | ❓ Open |
| AQ-3 | Mode 스택(stack switch)과 Story 내러티브 흐름이 어떻게 공존하는가? | ❓ Open |
| AQ-4 | 현재 씬 로딩 방식이 Addressable로 바뀌면 Zone 생명주기 인터페이스가 바뀌는가? | ❓ Open |
| AQ-5 | Save/Load가 Zone-Mode 상태와 어떻게 상호작용해야 하는가? | ✅ ADR-0003 |
| AQ-6 | 콘텐츠 풍부화 시 CatalogBaker `BakeAll` 전량 재스캔이 저작 루프 병목/충돌점이 되는가? | ❓ Open |
| AQ-7 | 시간/날짜 진행은 Service 상태 + 명시적 커밋 액션인가, Mode 전환 훅인가? | ✅ ADR-0001 |
| AQ-8 | BattleMode 종료 후 승/패/이탈 결과를 직전 스택에 전달하는 계약은 무엇인가? | ✅ ADR-0002 |
| AQ-10 | Polyglot 부팅 폰트 로드의 동기 `WaitForCompletion`이 다른 Localization/Addressables 비동기 작업과 겹칠 때 재진입(`Reentering the Update method`)한다 — 부팅 폰트 로드를 async로 전환할지, 콜드부팅 Localization 선행 완료를 보장할지? | ❓ Open |

## 성공 기준

Persona5형 수직 슬라이스가 두 루프를 오가며 시뮬 상태(시간·파티·세이브)와 내러티브 진행 상태를 유지한 채 실제로 동작할 때 "Zone-Mode 분리 검증 완료"로 본다.

그 과정에서 아키텍처의 실제 한계와 강점이 드러나야 한다 — 이론적 완성이 아니라 실전 마찰을 통한 학습이 목표다.
