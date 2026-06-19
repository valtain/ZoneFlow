# demo-connected-world — 탐색 질문

> village·dungeon(및 후속 Zone)을 **관찰 가능한 Zone 전환을 보존**하면서, "단절"감 없이
> 더 넓고 시나리오에 적합한 환경으로 연결하려면 demo-mvp의 씬 구조를 어떻게 재설계해야 하는가?

## 컨텍스트

demo-mvp는 명제 A(*Zone이 바뀌어도 Mode는 유지*)를 증명하는 1차 데모로 기능상 완료 상태다.
그러나 현재 씬 구조가 데모의 설득력을 떨어뜨린다.

- `SceneSetupTool.SetupDemoMvp()`가 **World1**에 `village` Zone, **World2**에 `dungeon` Zone을
  추가하지만, 각 Zone은 Plane + Cube 몇 개 + Cylinder Portal로 된 **고립된 원시 박스**다.
- village ↔ dungeon 연결은 **분리된 두 씬을 순간이동시키는 Portal** 하나뿐 → 사용자 표현 그대로
  "Zone 연결이 **단절**"되어 있고, 더 넓은·시나리오에 맞는 환경이 아니다.
- 두 씬에 과거 실험의 레거시 Zone(`world1`, `world1_b`, `story_w1`, `world2_b` 등)이 뒤섞여
  `ZoneAssetCatalog`에 ZoneId 8개가 누적됨.

## 핵심 제약 (codebase)

- 명제 A는 **씬 단위 실제 load/unload**(그 사이 Mode 스택 불변)로 증명된다.
- `ZoneRegistry.ReleaseAsync`는 **씬 내 모든 Zone의 RefCount가 0일 때만** `UnloadSceneAsync`를
  호출한다 → 같은 씬 내 Zone 전환은 SetActive 토글뿐(씬 언로드 X). **별도 씬 유지**가 전환 관찰력을
  보장한다.
- 카탈로그(ZoneAsset/SpawnPoint/Interactable)는 `CatalogBaker`가 씬에서 **bake**한다 →
  레거시 정리는 .asset 직접 수정이 아니라 씬에서 Zone 루트 제거 후 re-bake.

## 탐색 범위

- village·dungeon의 "연결" 구조 재설계 방향 비교 (연결감 ↔ 관찰 가능한 Zone 전환 트레이드오프)
- 씬 리네임 가부와 영향 범위 (빌드세팅·코드 문자열·카탈로그)
- 레거시 ZoneId 정리 범위 (`intro` 보존 필요성 포함)
- Claude(스크립트·데이터) vs 개발자(에셋·배치) 작업 분담

Out of scope: 실제 씬 재구성 구현 (close 후 `/feature` 승격), 아트 퀄리티, Battle/Story/Boss 로직

## 성공 기준

- 후보 ≥3개가 명제 A 보존 강도·연결감·작업량 축으로 비교됨
- 채택 방향 1개 + 폐기/보류 근거가 `findings.md`에 기록됨
- 1단계 실행 근거(리네임 대상·전환연출 연결점·레거시 정리 범위)가 실행 가능 수준으로 명시됨
- 후속 `/feature new --from demo-connected-world` 승격 가능 상태
