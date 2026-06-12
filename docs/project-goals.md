# Project Goals

## 학습 목표

ZoneFlow는 제품 개발이 아닌 **아키텍처 학습 프로젝트**다.  
목표: Zone-Mode 분리 아키텍처가 실제 게임 시나리오에서 어떻게 동작하는지 직접 부딪히며 증명한다.

AI-native 개발 방식 자체도 학습 대상이다 — 어떻게 하면 AI가 다음 아키텍처 질문을 먼저 발견하고 제안할 수 있는가.

## 현재 집중 영역

**Story 모드를 stress test vehicle로 사용한다.**

Story 모드는 Zone-Mode 분리의 가장 좋은 검증 도구다:
- 선형 내러티브와 Zone 탐색이 동시에 필요하다
- Mode 전환(Exploration ↔ Story)이 실제 마찰을 드러낸다
- "분리가 잘 됐다"는 걸 이론이 아니라 동작으로 확인할 수 있다

## Open Architectural Questions

AI는 아래 질문들을 참조해 아직 탐색되지 않은 항목을 발견하고 다음 작업을 제안한다.  
BACKLOG.md의 `Architectural Questions` 테이블에서 현재 답변 상태를 추적한다.

| # | 질문 |
|---|------|
| AQ-1 | Story 진행 중 Zone 강제 전환 시 Mode 스택 상태는 어떻게 되는가? |
| AQ-2 | Story 진행 상태(어느 챕터까지 봤는가)는 Zone 전환 후에도 유지되는가? |
| AQ-3 | Mode 스택(stack switch)과 Story 내러티브 흐름이 어떻게 공존하는가? |
| AQ-4 | 현재 씬 로딩 방식이 Addressable로 바뀌면 Zone 생명주기 인터페이스가 바뀌는가? |
| AQ-5 | Save/Load가 Zone-Mode 상태와 어떻게 상호작용해야 하는가? |

## 성공 기준

Story 모드가 여러 Zone을 오가며 스토리 진행 상태를 유지하는 시나리오가 실제로 동작할 때 "Zone-Mode 분리 검증 완료"로 본다.

그 과정에서 아키텍처의 실제 한계와 강점이 드러나야 한다 — 이론적 완성이 아니라 실전 마찰을 통한 학습이 목표다.
