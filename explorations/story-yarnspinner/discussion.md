# 탐색 로그

- [2026-06-15 | start] YarnSpinner × Story 모드 통합 검토를 독립 exploration으로 정식화. 출처: `~/.claude/plans/declarative-sniffing-lake.md`. portfolio-demo 데모와 별개 트랙(데모는 DialogueData 최소구현 S1, 본 건은 AQ-2/AQ-3 아키텍처 검증).

- [2026-06-15 | decision] portfolio-demo와의 관계 확정: 별개 트랙으로 분리. 데모 Story는 S1 유지, YarnSpinner는 AQ 검증 독립 트랙. (사용자 선택)

- [2026-06-15 | explore] 원본 계획의 내부 모순 식별 — 핵심 설계(DialogueRunner를 StoryMode 스코프에 두어 DontDestroyOnLoad 회피) vs 주의사항(MonoService root attach 또는 DontDestroyOnLoad 예외 검토 필요). ZoneFlow 제약 "DontDestroyOnLoad 금지"와 "DialogueRunner가 Zone 전환 중 생존+Update 필요" 요구가 충돌. → DialogueRunner GameObject의 소속 씬/소유권 결정이 1차 산출물.

- [2026-06-15 | brainstorm] 후보 3종 정리. A=StoryMode 스코프 소유(Mode 독립성 강조, 단 GameObject 소속 씬 명시 필요), B=DontDestroyOnLoad 예외(제약 위반, eliminated), C=MonoService root attach(생존·Update 안전, 단 상태 소유권이 Mode 밖). A·C 공통 제약: GameObject가 비-Zone 씬에 상주해야 Zone 전환에서 생존. 핵심 차이는 내러티브 상태 소유권(Mode vs Service) → 이 선택이 곧 AQ-3에 대한 답.

- [2026-06-15 | explore] 미해결 핵심: Candidate A에서 "StoryMode가 생성한 GameObject가 어느 씬에 instantiate되는가"가 원본 계획에 불명확. Zone 씬에 생성되면 Zone unload 시 동반 파괴되어 설계 의도(생존) 자체가 깨짐. CoreServices/GamePlayServices 등 비-Zone 씬을 부모로 지정하는 메커니즘 확인 필요 → 다음 탐색 시 SceneService/MonoService 코드 조사 대상.

- [2026-06-15 | external] 이전 대화 결정 확인 — GitHub 이슈 #49 "ContentServices 씬 + YarnSpinner DialogueService 구현"(OPEN)이 실제 확정 기록. 본 candidates.md는 이전 대화를 모른 채 plan 파일만 보고 라벨링되어 초기 라벨(A=StoryMode 스코프)과 실제 채택(서비스 스코프)이 어긋나 있었음. ground truth=이슈 #49.

- [2026-06-15 | decision] 채택 = Candidate C 발전형(전용 ContentServices 씬 + DialogueService MonoService). Candidate A(StoryMode 직접 소유) 폐기. 미해결이던 "GameObject 소속 씬" 문제는 ContentServices 전용 씬 신설로 해소 — 콘텐츠 세션(진입~메뉴 복귀) 수명에 진행 상태를 종속시켜 Zone 전환 생존 보장. AQ-2/AQ-3에 대한 답 확보.

- [2026-06-15 | close] 탐색 완료. findings.md 작성. 후속 = 이슈 #49.
