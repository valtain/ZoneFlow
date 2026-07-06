# ZoneFlow Backlog

Task 추적은 [GitHub Issues](https://github.com/valtain/ZoneFlow/issues)에서 관리한다.
이 파일은 로컬 설계 폴더(`features/`, `explorations/`) 인덱스 역할만 담당한다.

## Features

| Feature | 설계 폴더 |
| --- | --- |
| service_locator | features/service_locator/ |
| scene_service | features/scene_service/ |
| bootstrap | features/bootstrap/ |
| exploration-hud | features/exploration-hud/ |
| mode-transitionfx | features/mode-transitionfx/ |
| multi-zone-scene | features/multi-zone-scene/ |
| flexible-spawn | features/flexible-spawn/ |
| demo-mvp | features/demo-mvp/ |
| interaction-prompt | features/interaction-prompt/ |
| demo-mvp-scene | features/demo-mvp-scene/ |
| overworld-hub | features/overworld-hub/ |
| combat-battle-service | features/combat-battle-service/ |

## Explorations

| Name | Status | Promoted To |
| --- | --- | --- |
| milestone1-impl | closed | service_locator, scene_service, bootstrap |
| exploration-hud | closed | exploration-hud |
| mode-transitionfx | closed | mode-transitionfx |
| menu-world1-sky-gap | closed | #18, #19, #20 |
| zone-prefab-removal | closed | TASK 이슈 |
| multi-zone-single-scene | closed | multi-zone-scene |
| story-mode-stack | closed | flexible-spawn |
| story-mode-test-verify | closed | TASK 이슈 |
| lazyload-catalog | closed | TASK 이슈 |
| portfolio-demo | closed | demo-mvp |
| story-yarnspinner | closed | #49 |
| zone-entry-guard | active | — |
| portal-interaction-label | closed | interaction-prompt |
| demo-connected-world | closed | demo-mvp-scene, overworld-hub |
| zone-entry-camera | closed | TASK 이슈 |
| persona5-slice | closed | ADR-0001/0002/0003, AQ-5/7/8, feature 후보 5건, combat-battle-service |

## Architectural Questions

`docs/project-goals.md`의 Open Questions 답변 상태를 추적한다.  
`/next`가 이 테이블을 읽어 미답 항목을 발견하고 다음 탐색을 제안한다.

| ID | Question | Status | Linked Exploration/Feature |
| --- | --- | --- | --- |
| AQ-1 | Story 진행 중 Zone 강제 전환 시 Mode 스택 상태 | ❓ Open | — |
| AQ-2 | Story 진행 상태의 Zone 전환 후 지속성 | ❓ Open | story-yarnspinner → #49 |
| AQ-3 | Mode 스택(stack switch)과 Story 내러티브 공존 | ❓ Open | story-yarnspinner → #49 |
| AQ-4 | Addressable 전환 시 Zone 생명주기 인터페이스 변화 | ❓ Open | — |
| AQ-5 | Save/Load과 Zone-Mode 상태 상호작용 | ✅ Answered | persona5-slice → ADR-0003 |
| AQ-6 | 콘텐츠 풍부화 시 CatalogBaker `BakeAll` 전량 재스캔이 저작 루프 병목/충돌점이 되는가 (AQ-4 인접) | ❓ Open | level-designer·ui-designer 도입 |
| AQ-7 | 시간/날짜 진행은 Service 상태 + 명시적 커밋 액션인가, Mode 전환 훅인가 | ✅ Answered | persona5-slice → ADR-0001 |
| AQ-8 | BattleMode 종료 후 승/패/이탈 결과를 직전 스택에 전달하는 계약 | ✅ Answered | persona5-slice → ADR-0002 |
