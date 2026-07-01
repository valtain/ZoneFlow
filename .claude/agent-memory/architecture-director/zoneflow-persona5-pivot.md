---
name: zoneflow-persona5-pivot
description: ZoneFlow를 P5형 이중루프(사회시뮬↔던전전투) 수직슬라이스로 피벗 중 — 시간/파티/세이브=Service, 전투=BattleMode+BattleService 매핑 확정
metadata:
  type: project
---

2026-07-01, ZoneFlow를 "페르소나 5형 간단한 게임"으로 피벗. MVP = 수직 슬라이스(캘린더 1일 → 던전 입장 → 턴제 전투 → 귀가 → 날짜 진행). 전투 = 기본 턴제(약점/원모어/총공격/속성상성은 MVP 밖). 신설 에이전트: combat-specialist, systems-designer. 탐색 산출: `explorations/persona5-slice/`.

확정 매핑(architecture-director 검증):
- **시간/파티/세이브/인벤 = Service** (Mode 아님). Mode는 8단계 생명주기 전환 객체라 전역 상태를 담으면 ReplaceAll/Pop에서 파괴됨. constraints.md "서비스=씬 배치, 참조만"과 정합.
- **AdvanceDay는 일과-선택 패널의 사용자 액션 핸들러가 `TimeService.AdvanceDay()` 직접 호출.** Mode 생명주기 훅에서 호출 금지(암묵 결합 방지). 불변식: 시간 진행은 명시적 커밋 액션에서만, Mode 전환은 시간에 read-only.
- **전투 = BattleMode(진입/연출) + BattleService(턴 로직·결과 보관).** 별도 battle ZoneAsset 부여(SceneType.Zone으로 충분, 신규 타입 불필요). 상세 골격: [[zoneflow-mode-stack-push-pop-contract]].
- **Save: `ISaveable` 순회(aggregator 아님).** 복원은 "안정 상태에서만 세이브 + 현재 Zone/진입 Mode만 ReplaceAll 복원"(JRPG 세이브포인트 계약). Director는 진입 URI만 ISaveable 노출, `_stack`은 비대상. Yarn 변수는 DialogueService가 자기 VariableStorage 직렬화.

제안한 신규 AQ: AQ-7(시간 진행 = Service tick vs Mode 훅), AQ-8(전투 복귀 계약). AQ-5(Save/Load)는 재활성. BACKLOG 등록은 사용자 승인 대기.

**Why:** 프로젝트의 방향 자체가 Story 스트레스테스트에서 P5 이중루프로 확장됨. 이 매핑이 이후 모든 시뮬 시스템 설계의 기준선.

**How to apply:** P5 시뮬 시스템(전투·시간·파티·세이브·사회링크) 설계 요청 시 이 매핑표를 기준으로 검토. "시간/세이브를 Mode에 두자"는 제안은 위 근거로 반려. 미해결 결정 포인트(패배 시 복귀 경로, 세이브 트리거 지점 등)는 사용자 승인 필요.
