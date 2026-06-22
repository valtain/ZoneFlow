# multi-zone-scene — Tasks

> 핵심: 멀티존 엔진(ZoneRegistry·CatalogBaker·ReplaceAsync)은 **현재 코드에 이미 구현됨**.
> spec.md가 나열한 변경은 코드가 이미 반영(스펙 구식). 잔여 작업은 이를 **콘텐츠로 시연**하는 것.

| # | 제목 | 상태 |
| --- | --- | --- |
| 1 | Dungeon 씬 5-Zone 선형 체인 시연 — dungeon_0~dungeon_4(0↔1↔2↔3↔4). 진입 Zone `dungeon`→`dungeon_0` rename(Overworld portal·ColdStartup·GamePlayNavigationTests·SceneSetupTool 참조 갱신) + 카탈로그 re-bake. 같은 씬 내 Zone 이동 시 씬 언로드 없음 시연 | #74 |

관련: [overworld-hub](../overworld-hub/) — 시연 무대(허브 동선 위 Dungeon).
계획서: `.claude/plans/overworld-hub-feature-foamy-bee.md`
