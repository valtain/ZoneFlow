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

## Architectural Questions

`docs/project-goals.md`의 Open Questions 답변 상태를 추적한다.  
`/next`가 이 테이블을 읽어 미답 항목을 발견하고 다음 탐색을 제안한다.

| ID | Question | Status | Linked Exploration/Feature |
| --- | --- | --- | --- |
| AQ-1 | Story 진행 중 Zone 강제 전환 시 Mode 스택 상태 | ❓ Open | — |
| AQ-2 | Story 진행 상태의 Zone 전환 후 지속성 | ❓ Open | — |
| AQ-3 | Mode 스택(stack switch)과 Story 내러티브 공존 | ❓ Open | — |
| AQ-4 | Addressable 전환 시 Zone 생명주기 인터페이스 변화 | ❓ Open | — |
| AQ-5 | Save/Load과 Zone-Mode 상태 상호작용 | ❓ Open | — |
