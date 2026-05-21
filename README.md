# ZoneFlow

![Unity](https://img.shields.io/badge/Unity-2022.2%2B-black?logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)

여러 Zone을 탐색하고, NPC와 상호작용하며, 전투를 벌이는 오픈월드 Action RPG를 위한 Unity 아키텍처 프레임워크.  
ServiceLocator, 씬 오케스트레이션, 부트스트랩 흐름을 계층화하여 게임플레이 확장을 단순하게 만드는 것이 목적이다.

---

## 게임 구조

### 게임플레이 Mode

| Mode | 역할 |
| --- | --- |
| **ExploreMode** | 월드 탐색, NPC 상호작용, 아이템 수집 |
| **BattleMode** | 전투 진행 |
| **NarrativeMode** | 스토리 연출·시네마틱 |
| **MenuMode** | 메뉴 조작 |

### Zone 시스템

| Zone 종류 | 특징 |
| --- | --- |
| **SceneZone** | Unity 씬으로 구현된 대형 영역; Additive 로드 |
| **SubZone** | 부모 씬 내 논리적 구간 (보스룸, 특수 구역) |
| **PrefabZone** | 런타임 인스턴스화 (일회용 콘텐츠) |

각 Zone은 SpawnPoint와 Portal로 연결되어 Zone 간 이동을 지원한다.

---

## 아키텍처

런타임은 4계층으로 구성된다. 의존 방향은 위 → 아래 단방향이다.

| 계층 | 역할 |
| --- | --- |
| **Service** | 영속적 시스템; `MonoService<T>.Instance`로 접근 |
| **Scene** | Additive 씬 로드·언로드 오케스트레이션 |
| **Zone** | 게임플레이 컨텍스트 단위 |
| **Mode** | Zone 내부 게임플레이 행동 단위 |

씬 분류·핵심 씬 흐름 → [docs/architecture/](docs/architecture/)  
설계 결정 배경 → [docs/decisions/](docs/decisions/)

---

## 현재 상태 (Milestone 1 — Core Foundation)

| 컴포넌트 | 상태 |
| --- | --- |
| `MonoService<T>` — ServiceLocator 베이스 | ✅ 완료 |
| `ColdStartup` / `GamePlayBootstrap` — 부트스트랩 | ✅ 완료 |
| `SceneSo` / `SceneType` — 씬 데이터 모델 | ✅ 완료 |
| `SceneService` — 씬 로드·언로드 오케스트레이션 | 🔄 설계 완료, 구현 예정 |

---

## 빌드 & 테스트

- **빌드**: Unity Editor → File > Build Settings
- **테스트**: Unity Editor → Window > General > Test Runner
  - Editor: `Assets/ZoneFlowAssets/Tests/Editor/`

---

## 핵심 원칙 (위반 시 재작업)

- 서비스는 씬에 GameObject로 배치 — 코드로 생성 금지
- 비동기는 `UniTask` 전용 — `StartCoroutine` 금지
- 씬 로드는 `SceneService` 경유 — `SceneManager` 직접 호출 금지
- 예외는 `Debug.Assert` — `throw` 금지

전체 원칙 → [memory/architecture-principles.md](memory/architecture-principles.md)

---

## 문서

| | |
| --- | --- |
| [CONTEXT.md](CONTEXT.md) | LLM 협업용 전체 컨텍스트 브리프 |
| [memory/architecture-principles.md](memory/architecture-principles.md) | 10가지 핵심 원칙 |
| [docs/conventions/coding-style.md](docs/conventions/coding-style.md) | 코딩 규칙 |
| [BACKLOG.md](BACKLOG.md) | Feature·Exploration 인덱스 |
