# 탐색 로그

- [2026-06-16 | start] HUD MissingReferenceException 디버깅 중 파생. 전환 가드를 두 층위로 분리: (1) 시스템 레벨 전환 재진입 가드 → 이슈 #54로 구현 완료, (2) 콘텐츠 레벨 입장 가드 → 본 탐색. 현재 유일 사례는 Portal.cs의 IsSpawnCooldown 하드코딩.

- [2026-06-16 | explore] 상호작용 흐름 코드 확인: Portal.OnTriggerEnter가 (Player 태그 / Director 준비 / IsSpawnCooldown) 3종 체크 후 NavigateAsync.Forget(). IInteractable은 InteractableId + OnInteractAsync(director, ct)만 정의. InteractableCatalog는 Zone 미로드 상태에서도 InteractableId→(ZoneId, NavigationUri) 조회 가능 — "Zone 미로드 평가" 가능성이 후보 차별 축으로 부상.

- [2026-06-16 | brainstorm] 후보 4종 도출:
  - A. IEntryCondition 컴포넌트(Interactable 부착, 합성·디자이너 친화 ◎, 단 Zone 미로드 평가 ✗)
  - B. 데이터 기반 조건(Catalog Entry 확장, Zone 미로드 평가 ◎, 단 표현력 △)
  - C. 중앙 Navigation 가드 파이프라인(단일 통제점, 단 Director 콘텐츠 결합·#54와 층위 혼동 우려)
  - D. Portal 서브클래스/가상 메서드(최소안 베이스라인, 단 조합 폭발·합성 ✗)
  - 4축 평가: 규칙 위치 / Zone 미로드 평가 / 합성 / 디자이너 친화.

- [2026-06-16 | external] WebSearch(Unity locked-door/key gating 패턴). 핵심: 업계 통념은 IInteractable.Interact 내부에서 isLocked 등 조건 체크 + 거부 시 프롬프트("Press E to unlock") 반환 — 본 탐색의 Candidate D(인터랙터블 내 조건) + 거부 사유 피드백과 일치. Meta XR "Gating" 개념은 "요구 충족 전까지 상호작용 차단하되 다른 상호작용은 막지 않음" — 본 탐색의 콘텐츠 가드 층위 정의와 부합. 새 Candidate 없음(기존 A/B/C/D가 외부 통념을 포함·확장). 평가 변경: D는 "업계 베이스라인"으로 자리매김하되 합성 한계는 그대로 — 조합 필요 시 A로 승급이 자연스러움.

- [2026-06-16 | decision] 선결 과제 식별: 네 후보 모두 GameState/Flags 서비스(키 보유·진행 플래그)를 전제. 실제 콘텐츠 규칙(예: "열쇠 필요 던전")이 1~2개 생기기 전엔 일반 메커니즘 확정을 보류(YAGNI). 그 시점에 A/B 또는 A+B 하이브리드 채택 유력.
