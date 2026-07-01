# 탐색 로그

- [2026-07-01 | start] persona5-slice 탐색 시작. P5형 이중 루프 수직 슬라이스가
  Zone-Mode 분리 위에서 성립하는지, P5 시스템을 Zone/Mode/Service에 어떻게 매핑할지 검증.
  플랜 승인으로 방향 확정: MVP=수직 슬라이스, 전투=기본 턴제, 신설=combat-specialist·systems-designer.

- [2026-07-01 | brainstorm] architecture-director 매핑 검증 완료. 핵심 발견:
  - **골격은 이미 이중 루프를 지지한다.** StackAsync(GamePlayDirector.cs 151-166)로 BattleMode를
    push하면 IsOverlay=false이므로 직전 Exploration Zone이 SetActive(false)로 잠들고 battle Zone이
    스왑된다. PopAsync(196-215)가 battle Zone Release + 직전 모드 ResumedAsync로 위치 재스폰까지
    수행 — 던전→전투→복귀 루프는 코드 변경 없이 성립. 결손은 오직 "전투 결과 전달 채널"뿐.
  - **시간·파티·세이브 = Service 판단 확정.** GamePlayMode는 8단계 생명주기의 전환 가능 객체
    (Created→Destroyed)라 전역 상태를 담으면 ReplaceAll/Pop에서 파괴된다. constraints.md의
    "서비스=씬 배치, 참조만" 원칙과 정합. AdvanceDay 결합 우려는 "호출 주체(일과-선택 패널 액션)
    ↔ 상태 소유자(TimeService)" 분리로 해소 — Mode 훅에서 AdvanceDay 호출 금지가 불변식.
  - **Save 복원 정책**: Mode 스택 전체 재구성은 전환 중간상태(`_isNavigating` 구간)를 재현 못 해
    깨지기 쉽다 → "안정 상태에서만 세이브 + 현재 Zone/진입 Mode만 ReplaceAll 복원"이 JRPG
    세이브포인트 계약과 동일하며 골격 마찰 최소. Director는 진입 URI만 ISaveable로 노출, `_stack`은
    세이브 대상 아님.
  - **Battle 결과 채널**: URI 파라미터(pop?result=) 기각 — NavigationRequest pop 분기가 쿼리를
    버리고(NavigationRequest.cs 63-67) 결과는 승/패 bool을 넘어 구조화 페이로드로 자란다. Navigation
    URI는 "어디로"지 "무슨 일"이 아님(관심사 분리). 대신 모드 간 결과 채널(BattleService 보관 →
    Resume된 모드가 pull) 채택. OnResumedAsync가 파라미터를 안 받으므로(GamePlayMode.cs 143) pull
    모델이 훅 시그니처 무변경 최소안.
  - **미해결**: 패배 시 Pop(팰리스 복귀) vs ReplaceAll(아지트 게임오버 복귀) 분기를 누가 결정하나
    → 결정 포인트로 승격. SceneType enum은 Zone 하나뿐(SceneType.cs) — battle Zone도 SceneType.Zone
    으로 충분, 신규 타입 불필요.
  - **신규 AQ 3건 제안**: AQ-7(시간 진행), AQ-8(전투 복귀 계약). AQ-5(Save/Load)는 재활성만.
    AQ-5·AQ-7·AQ-8은 서로 참조 관계(세이브가 시간·전투결과·스택을 스냅샷).

- [2026-07-01 | decision] 사용자 승인 — 5개 결정 포인트 확정: 패배=아지트 ReplaceAll(게임오버),
  전투=별도 아레나 Zone, 세이브 지점=아지트+일과선택, 기록=AQ 등록+ADR. 매핑 검증 결론과 AQ 권고
  전부 채택.

- [2026-07-01 | close] 탐색 완료. ADR-0001(시뮬=Service)·0002(전투 결과 채널)·0003(Save ISaveable)
  작성, AQ-5 Answered·AQ-7·AQ-8 신규 등록. feature 후보 5건(combat 2 / systems 3) 도출.
  다음: Phase 1 에이전트 셋업 → /feature new.
