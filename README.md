# ZoneFlow

![Unity](https://img.shields.io/badge/Unity-2022.2%2B-black?logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)

오픈월드 Action RPG를 위한 Unity 게임플레이 아키텍처 탐구 프로젝트.

---

게임 세계는 여러 구역으로 구성되고, 하나의 구역에서도 탐색·전투·스토리 같은 서로 다른 행동이 발생할 수 있다. 대부분 구역과 행동이 깊게 연결되어 있다. "A 구역에 들어가면 전투를 해야 하니 전투 UI를 보여준다"와 같은 식으로, 처음에는 직관적이지만 세계가 복잡해질수록, 행동이 다양해질수록 구현이 빠르게 복잡해진다.

이 프로젝트에서는 **어디(Zone)와 무엇(Mode)을 분리**해 세계와 행동을 독립적으로 확장할 수 있는 구조를 만든다. 어떤 구역에서도 원하는 행동을 자유롭게 조합할 수 있고, 하나를 수정해도 나머지에 영향을 주지 않는 것이 목표다. 간략한 게임 형태로 구현해 이 설계의 실제 적용 가능성을 직접 확인한다.

이 탐구 과정 전체에서 AI를 적극 활용한다. 단순한 코드 생성 도구가 아니라, 탐색·설계·구현의 각 단계에서 AI가 맥락을 함께 추적하고 다음으로 확인해야 할 아키텍처 질문을 먼저 제안한다. 개발자는 방향을 결정하는 데 집중하고, AI는 그 흐름이 끊기지 않도록 돕는다.

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
| **StoryMode** | 스토리 연출 |
| **BattleMode** | 전투 진행 |
| **ShellMode** | 로비·허브 공간 |
| **PanelMode** | UI 오버레이 (Zone 로드 없음) |

---

## 아키텍처

런타임은 4계층으로 구성된다. 의존 방향은 위 → 아래 단방향이다.

| 계층 | 역할 |
| --- | --- |
| **Service** | 영속적 시스템. 게임 전체에서 하나만 존재하며 다른 계층에서 참조한다 |
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

탐색 및 Feature 인덱스 → [BACKLOG.md](BACKLOG.md)

---

## 문서

| | |
| --- | --- |
| [docs/project-goals.md](docs/project-goals.md) | 프로젝트 목표 + 탐색 중인 아키텍처 질문 |
| [docs/architecture/](docs/architecture/) | 씬 계층·시스템 계층·제약 원칙 |
| [docs/conventions/coding-style.md](docs/conventions/coding-style.md) | 코딩 규칙 |
| [BACKLOG.md](BACKLOG.md) | Feature·Exploration 인덱스 |
