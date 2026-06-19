# demo-mvp-scene — Tasks

| # | 제목 | 상태 |
| --- | --- | --- |
| 1 | 씬 리네임 World1→Village, World2→Dungeon — .unity 파일명(+.meta 이동, GUID 보존)·EditorBuildSettings path·ZoneAssetCatalog.SceneName·SceneSetupTool 하드코딩 문자열/메뉴명 일괄 | #63 closed |
| 2 | Zone/Portal 내비게이션 전환에 FadeScreen 연결 — GamePlayDirector.SelectTransitionAsync의 InstantBlackScreen을 FadeScreen으로 교체(암전→이동→복귀) | #64 closed |
| 3 | 레거시 Zone 루트 제거 + 카탈로그 re-bake — 씬에서 world1/world1_b/story_w1/world2/world2_b 제거(intro 보존) 후 CatalogBaker re-bake로 3개 카탈로그 동기 | #65 closed |
| 4 | 리네임·전환 회귀 검증 — GamePlayNavigationTests PlayMode 갱신/추가 | #66 closed |
