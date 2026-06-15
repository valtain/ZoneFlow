# 탐색 결과

**결론**: ZoneFlow 포트폴리오 데모를 **MVP 우선, 단계적 feature 분할**로 구현한다. 1차 feature는 명제 A(Zone 전환 중 Mode 유지)를 증명하는 최소 범위 — Village→Dungeon 이동 + ExplorationMode 유지 — 로 한정하고, 동작 확인 후 Story·Battle·Boss·연출을 후속 feature로 단계 추가한다.

**채택된 방향**:

- **범위(축1)**: Candidate B — MVP 우선. 1차 feature = 명제 A. 풀스펙(A)은 deferred.
- **Story 대화(축2)**: Candidate S1 — `DialogueData : ScriptableObject { string[] lines }` 최소 구현 + StoryHudPanel 순차 출력. (YarnSpinner는 별도 트랙 story-yarnspinner → #49, 데모와 무관)
- **Battle 로직(축3)**: Candidate Bt1 — 더미 전투(접촉→BattleMode Push→자동/1클릭 승리→Pop). 여유 시 Bt2(클릭 데미지+HP)로 상향.
- **Enemy/NPC 연결(축4)**: Candidate N1 — `IInteractable` 통일. Enemy·NPC가 `NavigationUri`를 들고 `OnInteractAsync()`로 이동(기존 Portal.cs 규약). GamePlayDirector 직접 참조 금지.
- **작업 분담(축5)**: Claude=C# 스크립트+`.prefab`+카탈로그 등록 / 개발자=에셋 임포트+씬 오브젝트 배치+조명. 입력 문서의 씬 배치 Task는 "배치 지침서"로 재포맷.

**폐기된 방향**:

- Candidate A(풀스펙 일괄) — deferred. MVP 동작 확인 전 전체를 묶으면 미구현 볼륨(Story 대화·Battle 로직·Boss 엔딩) 리스크가 큼.
- Candidate S2(YarnSpinner 등 외부 프레임워크) — 데모 범위 대비 과설계. 단, 아키텍처 검증용으로는 별도 트랙(story-yarnspinner)에서 채택됨.
- Candidate N2(Enemy가 Director 직접 호출) — 의존 방향 역전.

**후속 Feature 후보**:

1. **demo-mvp (1차)** — 명제 A. Zone_Village/Zone_Dungeon(기존 World1/World2 재활용 + ZoneId 매핑) + ExplorationMode(완성) + Portal(완성) 연결. Claude: ZoneAssetCatalog/SpawnPointCatalog 등록, Portal NavigationUri 설정. 개발자: 씬 배치.
2. demo-battle (2차) — BattleMode 더미(Bt1) + EnemyController(IInteractable, N1) + Enemy_Slime 프리팹. 명제 B 증명.
3. demo-story (3차) — DialogueData(S1) + StoryHudPanel 순차 출력 + NpcInteractable. Village에서 명제 B 보강.
4. demo-boss (4차) — Zone_BossRoom + Enemy_Boss + 엔딩(StoryMode Replace).
5. demo-polish (5차) — 페이드/HUD 애니, WebGL 빌드, README 다이어그램.

**CLAUDE.md 반영 필요**: 없음. (씬 배치=개발자 수동, 스크립트·프리팹=Claude라는 작업 분담 원칙이 데모 전반에 반복 적용되나, 일반 규칙화는 데모 종료 후 판단)
