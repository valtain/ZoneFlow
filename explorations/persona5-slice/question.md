# persona5-slice — 탐색 질문

> 페르소나 5형 이중 루프(사회 시뮬 ↔ 던전 전투)의 **수직 슬라이스**가
> 기존 Zone-Mode 분리 아키텍처 위에서 성립하는가? P5 시스템을
> Zone/Mode/Service 중 어디에 매핑해야 골격을 깨지 않는가?

## 컨텍스트

프로젝트를 "페르소나 5 형태의 간단한 게임"으로 피벗한다. 골격(Mode 스택·Navigation
URI·Zone/Scene 로딩·Exploration/Story/Panel 모드·Yarn 대화·레이어드 UI·Interactable/
Portal·Player 이동)은 이미 프로덕션급이며, 비어있는 것은 그 위의 RPG 시뮬 레이어다:
BattleMode(스켈레톤), 파티/페르소나, 스탯·성장, 인벤·장비, 시간/캘린더, 세이브/로드,
사회링크(코옵).

**확정 방향**: MVP = 수직 슬라이스(`캘린더 1일 → 던전 입장 → 턴제 전투 → 귀가 → 날짜
진행`), 전투 = 기본 턴제(약점/원모어/총공격 제외), 신설 에이전트 = combat-specialist +
systems-designer.

## 탐색 범위

이 탐색은 **아키텍처 매핑 검증**이 목적이다 (코드 변경 없음, Analysis).

1. P5 시스템 → Zone / Mode / Service 매핑의 정합성 검증 (특히 시간·저장·파티를
   Service로 두는 판단).
2. 신규 Architectural Question 등록:
   - **AQ-신규 (시간 진행)**: 날짜/타임슬롯 진행은 Service tick(전역 상태)인가,
     Mode 전환 훅인가? Mode 스택과 어떻게 직교를 유지하나?
   - **AQ-5 (재활성)**: Save/Load가 Mode 스택 + Zone 상태 + Yarn 변수 + 파티/시간을
     어떻게 스냅샷·복원하나? 어느 계층이 스냅샷의 권위인가?
   - **AQ-신규 (전투 복귀 계약)**: BattleMode 종료 후 직전 Exploration/Story 스택으로
     복귀하는 계약(승/패/이탈 각각)은 무엇인가?
3. 수직 슬라이스 MVP 기능 목록을 in/out-of-scope로 확정.

**Out of scope**: 시그니처 P5 전투(약점→원모어→총공격, 속성 상성), 코옵 아크·랭크,
페르소나 합체, 콘텐츠 스케일업 — 모두 MVP 이후 심화 백로그.

## 성공 기준

- P5→Zone-Mode-Service 매핑표가 constraints.md 원칙과 충돌 없음을 확인.
- 신규 AQ 3건이 명확한 질문·후보와 함께 BACKLOG에 등록됨.
- 수직 슬라이스 MVP 기능 목록 확정 → 두 축(combat / systems)의 feature 승격 후보 도출.
