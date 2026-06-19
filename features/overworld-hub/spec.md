# overworld-hub — 설계 스펙

## 목표

demo-connected-world 탐색의 **2단계 C2** 방향 구현. demo-boss 단계의 *구조* 기반으로,
**Overworld 허브 씬**을 추가해 village·dungeon·신규 **BossRoom**을 포털 갈림길로 연결한다.
C3(demo-mvp-scene)에서 페이드로 가렸던 "독립된 섬" 구조를 허브로 실제 연결해, 명제 A(씬 단위
Zone load/unload)를 **반복 시연**한다.

**범위 밖(명시적 제외)**: 보스 전투 메커닉(적·HP·공격·승패 판정)은 본 feature가 다루지 않는다.
본 feature는 BossRoom의 *무대*(씬·Zone·포털·스폰)까지만 만든다. 전투 방식은 별도 `/explore` 대상.

## 주요 컴포넌트

- **Overworld 씬 + `overworld` Zone**: 갈림길 허브. 마을문/던전입구/보스문 3개 포털 배치.
- **BossRoom 씬 + `boss_room` Zone**: 신규 씬·Zone (전투 무대, 로직 추후).
- **포털 그래프**:
  - overworld → village / dungeon / boss_room (갈림길)
  - village / dungeon / boss_room → overworld (복귀)
  - 기존 village↔dungeon 직결 포털(C3)을 허브 경유로 재편할지 검토 (decisions.md).
- **허브 생명주기**: 진입 시 허브 씬 언로드 + 대상 Zone 로드(Mode 불변), 복귀 시 허브 reload +
  직전 위치 스폰. **(설계 결정 필요 — decisions.md 참조)**
- **카탈로그/빌드**: 신규 2씬을 EditorBuildSettings 등록 + CatalogBaker re-bake.

## 데이터 흐름

부트(MenuPanel NewGameUri) → **overworld 진입** → 포털 선택 → 대상 Zone 로드 / 허브 씬 언로드
→ 복귀 포털 → 허브 reload + 복귀 지점(`overworld_*_return`) 스폰.

> 진입점 변경: 현재 NewGameUri는 `gameplay://exploration/village`. 허브 도입 시 시작점을
> overworld로 옮길지 여부는 미결(decisions.md).
