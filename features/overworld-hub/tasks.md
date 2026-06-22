# overworld-hub — Tasks

> 설계 확정(2026-06-19): Replace+허브 reload · 진입점 overworld · 직결 포털 제거(허브 경유) · 원점별 복귀 스폰. decisions.md 참조.

| # | 제목 | 상태 |
| --- | --- | --- |
| 1 | BossRoom 씬 + boss_room Zone 신규 — 무대(지면·기본 스폰·overworld 복귀 포털)만, 전투 로직 제외. EditorBuildSettings 등록 | #68 closed |
| 2 | Overworld 씬 + overworld Zone 신규 — 갈림길 3포털(→village/dungeon/boss_room) + 원점별 복귀 스폰포인트(overworld_from_*) 배치, EditorBuildSettings 등록 | #69 closed |
| 3 | village/dungeon 포털 재편 — C3 직결 포털(portal_to_dungeon/portal_to_village) 제거 → overworld 복귀 포털(Replace, `?id=overworld_from_{zone}`)로 교체 | #70 closed |
| 4 | 진입점 전환 — MenuPanel.NewGameUri를 gameplay://exploration/overworld로 변경 | #71 closed |
| 5 | 카탈로그 re-bake — 신규 2씬·재편 포털 반영, Zone/SpawnPoint/Interactable 3종 동기 | #72 closed |
| 6 | 허브 왕복 회귀 검증 — overworld→village/dungeon/boss_room→overworld PlayMode (씬 reload·원점별 복귀 스폰) | #73 closed |
