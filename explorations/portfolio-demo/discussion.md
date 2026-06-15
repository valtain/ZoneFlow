# 탐색 로그

- [2026-06-15 | start] 포트폴리오 데모 exploration 시작. 입력 자료 2종(시나리오 스펙, 아트 에셋 셋업 계획)을 코드베이스와 대조하며 검토.

- [2026-06-15 | explore] 코드베이스 현황 파악(Explore agent). 완성: Navigation URI(파싱·빌더·Portal), GamePlayDirector(스택 Launch/Stack/Replace/ReplaceAll/Pop), ZoneRegistry(refcount 생명주기), Mode 베이스(상태머신)+5구현체, UI 7레이어+전환효과, 서비스 4종. 미구현: Dialogue 시스템, NpcInteractable 구현체, Enemy 게임 로직, Zone 씬 3종. ExplorationMode/StoryHudPanel은 완성, BattleMode는 스타터만.

- [2026-06-15 | explore] 시나리오 스펙 피드백. (1) Phase 2(Mode 구현) 볼륨 과소평가 — Story 대화 시스템·Battle 로직이 최대 미구현 구간. (2) StoryMode가 두 명제 모두에 핵심이나 대화 시스템 부재 → DialogueData 최소 구현 필요. (3) 씬명 불일치(World1/2 vs Zone_*) → ZoneAssetCatalog 매핑으로 해소. 작업순서 수정 제안: Story를 3순위→2순위로 앞당겨 명제 B 조기 증명.

- [2026-06-15 | explore] 아트 에셋 계획 피드백. 1팩+Primitive 전략·로직/비주얼 분리 포인트는 타당. 수정 3건: (1) Task 2~4 씬 배치는 Claude 불가(.meta GUID 위험) → 배치 지침서로 재포맷. (2) 경로/컴포넌트 오류 — `ZoneFlow/Prefabs/Enemies` → `ZoneFlowAssets/Runtime/Prefabs`, PortalController 불필요(Portal.cs 존재), NpcInteractable은 IInteractable 구현. (3) EnemyController가 OnTriggerEnter에서 직접 BattleMode Push → 의존 역전. Portal 규약(NavigationUri+OnInteractAsync) 따라 IInteractable로 통일 권장.

- [2026-06-15 | decision] 작업 분담 확정: Claude=스크립트+프리팹+카탈로그 등록 / 개발자=에셋 임포트+씬 배치+조명. Enemy/NPC→Navigation 연결은 IInteractable 통일 패턴(N1) 채택 방향.

- [2026-06-15 | decision] 구현 범위(축1) 확정 = Candidate B(MVP 우선). 1차 feature=명제 A(Village→Dungeon+ExplorationMode), Story(S1)/Battle(Bt1)/Boss/연출은 후속 feature로 단계 분리. 풀스펙(A)은 deferred. (사용자 확정)

- [2026-06-15 | close] 탐색 완료. findings.md 작성. 후속 = demo-mvp(1차) + demo-battle/story/boss/polish 단계 분할.
