---
name: village-portal-replacement
description: Village 씬 인라인 포털 2개를 Portal_Gateway 프리팹으로 교체한 이력 (2026-07-02)
metadata:
  type: project
---

Village 씬(`Assets/ZoneFlowAssets/Scenes/Village.unity`)의 인라인 실린더 포털 2개를
`Portal_Gateway.prefab`(`Assets/ZoneFlowAssets/Runtime/Prefabs/Portal_Gateway.prefab`) 인스턴스로 교체.

**교체된 포털 값:**
- `portal_village_to_overworld`: NavigationUri=`gameplay://exploration/overworld?id=overworld_from_village`, DisplayLabel="To Overworld", 위치=(6, 1.5, 90)
- `portal_village_to_story`: NavigationUri=`gameplay://story/village`, DisplayLabel="Enter Story Mode", 위치=(2, 1.5, 90)

**추가된 에셋:**
- `Assets/ZoneFlowAssets/Materials/PortalSurface.mat` — URP/Lit, HDR 시안 Emission, Transparent
- `Assets/ZoneFlowAssets/Materials/PortalFrame.mat` — URP/Lit, 어두운 메탈, 서브틀 Emission
- `Assets/ZoneFlowAssets/Settings/Village_VolumeProfile.asset` — Bloom intensity=1.0, threshold=0.9
- `Village_BloomVolume` — Env 아래, isGlobal=true, priority=1
- `ZoneMarker_village` — Zone_village 아래, (-3, 0, 74), 오벨리스크형

**Issue #77 (2026-07-03):** Overworld(3개), Dungeon(9개), BossRoom(1개) 씬에도 동일 패턴 전파 완료.
각 씬에 ZoneMarker 오벨리스크("OVERWORLD","DUNGEON 0~4","BOSS ROOM") + Village_VolumeProfile 공유 BloomVolume 추가.
SerializedObject로 Portal 필드 설정이 가장 안정적 (so.FindProperty("<NavigationUri>k__BackingField") 패턴).

**Why:** 테스트 콘텐츠를 프로다운 포털 프리팹으로 업그레이드 (GitHub Issue #76/#77).
**How to apply:** 새 씬에도 동일 Portal_Gateway 프리팹 인스턴스화 후 NavigationUri/PortalId/DisplayLabel 세팅.
CatalogBaker BakeAll 필요 — 메인 세션이 실행.
