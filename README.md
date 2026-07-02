# ZoneFlow

![Unity](https://img.shields.io/badge/Unity-2022.2%2B-black?logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)

Unity 게임플레이 아키텍처 탐구 프로젝트.

---

마을에서는 대화 UI, 던전에서는 전투 UI.
구역이 늘수록 조건문도 늘고, 어느 순간 구역과 행동이 통째로 얽힌다.

이 프로젝트는 그 얽힘을 풀기 위한 실험이다. **어디(Zone)와 무엇(Mode)을 분리**하면: 어떤 구역에서도 원하는 행동을 자유롭게 조합하고, 하나를 바꿔도 나머지가 흔들리지 않는 구조가 된다. 시스템 구현과 함께 실제 콘텐츠로 그 가능성을 검증한다. → [로드맵](#로드맵)

개발 전반에서 AI를 파트너로 활용한다. 코드를 대신 써주는 도구가 아니라, 흐름을 함께 추적하면서 다음 아키텍처 질문을 먼저 꺼내는 역할이다. → [개발 방식](#개발-방식)

→ 현재 탐색 중인 아키텍처 질문: [docs/project-goals.md](docs/project-goals.md)

---

## 게임 구조

### Zone — 공간 단위

게임 세계를 구성하는 구역이다. Unity 씬 단위로 필요할 때 로드하고, 쓰지 않을 때 자동으로 해제한다. SpawnPoint(등장 위치)와 Portal(구역 연결)을 포함한다.

### Mode — 행동 단위

플레이어가 지금 무엇을 하고 있는가를 정의한다. Zone과 독립적으로 존재하며, URI 형식(`gameplay://story/zone_id`)으로 전환하거나 스택으로 쌓을 수 있다.

| Mode | 역할 |
| --- | --- |
| **ExplorationMode** | 월드 탐색, NPC 상호작용 |
| **StoryMode** | 스토리 연출 (Yarn Spinner 대사) |
| **BattleMode** | 턴제 전투 진행 |
| **ShellMode** | 로비·허브 공간 |
| **PanelMode** | UI 오버레이 (Zone 로드 없음) |

---

## 아키텍처

런타임은 4계층으로 구성된다. 의존 방향은 위 → 아래 단방향이다.

| 계층 | 역할 |
| --- | --- |
| **Service** | 영속적 시스템. 게임 전체에서 하나만 존재하며 다른 계층에서 참조한다. 시뮬 백본(TimeService·PartyService·SaveService)이 여기 상주한다 ([ADR-0001](docs/decisions/0001-sim-state-in-service-layer.md)) |
| **Scene** | Unity 씬의 로드·언로드를 조율한다 |
| **Zone** | 게임플레이 공간 단위. 씬 위에서 생명주기를 관리한다 |
| **Mode** | 플레이어의 현재 행동 상태. Zone 위에서 동작하며 스택으로 관리된다 |

상세 → [docs/architecture/](docs/architecture/)

---

## 지금까지 만든 것

| 항목 | 설명 |
| --- | --- |
| Navigation | `gameplay://` URI로 Mode와 Zone을 전환하는 내비게이션 시스템 |
| Zone 생명주기 | Zone이 여러 곳에서 참조될 때 중복 로드 없이 자동으로 관리 (`ZoneRegistry`) |
| Mode 스택 | 교체·쌓기·전체 초기화 방식의 Mode 전환 (`GamePlayDirector`) |
| HUD | Mode마다 전용 UI 패널 (탐색용·스토리용 분리) |
| Bootstrap | 게임 시작 시 씬을 순서대로 초기화하는 흐름 |
| Dialogue | Yarn Spinner 기반 대사·내러티브. `DialogueService`로 Zone 전환 간 진행 상태 보존 |
| 멀티존 던전 체인 | 단일 씬 안에서 dungeon_0~4를 선형으로 오가는 인-씬 이동 시연 |
| Interaction Prompt | 포털·상호작용 대상에 근접 시 라벨을 띄우는 프롬프트 패널 |
| 전투 수직 *(진행 중)* | `BattleMode`·`BattleService` 턴제 골격 — 턴 순서·스킬 실행·HP/데미지 |

---

## 로드맵

Persona5형 수직 슬라이스(`캘린더 1일 → 던전 → 턴제 전투 → 귀가 → 날짜 진행`)를 향한 6단계 진행. 아키텍처 결정 근거는 [docs/decisions/](docs/decisions/), 탐색 산출물은 [explorations/persona5-slice/findings.md](explorations/persona5-slice/findings.md) 참조.

| Phase | 내용 | 상태 |
| --- | --- | --- |
| 0 | 아키텍처 매핑 | ✅ 완료 |
| 1 | 역할 기반 에이전트 셋업 | ✅ 완료 |
| 2 | 백본 서비스 (TimeService·SaveService·PartyService) | 예정 |
| 3 | 전투 수직 (BattleMode·BattleService) | 진행 중 |
| 4 | 시뮬 루프 배선 (UI·Zone) | 예정 |
| 5 | 통합·저장 검증 | 예정 |

---

## 개발 방식

구현하기 전에 **탐색(Explore)**한다. 아이디어나 구조적 의문이 생기면 먼저 질문을 정의하고 후보 방향을 비교한다. 결론이 나오면 **Feature**로 전환해 설계 문서를 작성하고, 이슈 단위로 나눠 구현한다.

```text
질문 정의 → /explore new   후보 비교, 결론을 findings.md에 기록
                  ↓
         /feature new   spec·decisions 설계 문서 작성
                  ↓
          /issue do     이슈 단위 구현
```

전 과정을 AI와 협업해 진행한다. 탐색·설계·구현의 흐름을 AI가 함께 추적하고, 다음 아키텍처 질문을 먼저 제안하기도 한다.

### 역할 기반 서브에이전트

[claude-code-game-studios](https://github.com/donchitos/claude-code-game-studios)의 영향으로 **역할 기반 서브에이전트 라우팅**을 도입했다. complexity-routing이 작업 난이도에 따라 *tier(모델)* 를 고르고, 아래 에이전트가 *role(정체성)* 을 담당한다 — 두 축은 직교한다. 정의: [.claude/agents/](.claude/agents/).

| 에이전트 | 역할 | 위임 커맨드 |
| --- | --- | --- |
| **architecture-director** | Zone-Mode 분리 검토, 아키텍처 질문(AQ) 발견·제안 | `/explore`, `/issue review` |
| **unity-specialist** | Unity API·구현 권위자, 단일 시스템 기능 구현 | `/issue do` |
| **level-designer** | 존/레벨 콘텐츠 설계·저작 (레이아웃·연결성·페이싱) | `/level` |
| **ui-designer** | UI/HUD/패널 설계·저작 (PanelCatalog 등록) | `/ui` |
| **combat-specialist** | 턴제 전투 설계·구현 (BattleMode·스킬·페르소나) | `/battle` |
| **systems-designer** | 시뮬 시스템·데이터 모델 (시간·파티·세이브·인벤) | `/systems` |

작업 복잡도에 따라 모델 티어(haiku/sonnet/opus)를 자동 라우팅하고, 특정 경로의 파일을 수정하기 전에는 대응하는 **path-scoped rules**([.claude/rules/](.claude/rules/))를 먼저 적용한다. 상세 기준은 [CLAUDE.md](CLAUDE.md)·[.claude/docs/complexity.md](.claude/docs/complexity.md) 참조.

### 커스텀 커맨드

| 커맨드 | 용도 |
| --- | --- |
| `/explore` `/feature` `/issue` | 탐색 → 설계 → 구현 코어 흐름 |
| `/level` `/ui` | 존/레벨·UI 콘텐츠 저작 |
| `/battle` `/systems` | 전투·시뮬 시스템 설계·구현 |
| `/next` `/quick` `/bridge` | 흐름 오케스트레이션·소규모 작업·세션 인수인계 |

탐색 및 Feature 인덱스 → [BACKLOG.md](BACKLOG.md)

---

## 문서

| | |
| --- | --- |
| [docs/project-goals.md](docs/project-goals.md) | 프로젝트 목표 + 탐색 중인 아키텍처 질문 |
| [docs/architecture/](docs/architecture/) | 씬 계층·시스템 계층·제약 원칙 |
| [docs/decisions/](docs/decisions/) | 아키텍처 결정 기록 (ADR) — 시뮬 상태·전투 결과 채널·Save/Load |
| [docs/conventions/coding-style.md](docs/conventions/coding-style.md) | 코딩 규칙 |
| [.claude/agents/](.claude/agents/) · [.claude/rules/](.claude/rules/) | 역할 기반 에이전트 정의·경로별 규칙 |
| [BACKLOG.md](BACKLOG.md) | Feature·Exploration 인덱스 + Architectural Questions 추적 |
