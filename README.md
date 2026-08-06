# ZoneFlow

![Unity](https://img.shields.io/badge/Unity-6000.3%2B-black?logo=unity)
![URP](https://img.shields.io/badge/URP-17.3-black?logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)

Unity 게임플레이 아키텍처 탐구 프로젝트.

---

마을에서는 대화 UI, 던전에서는 전투 UI.
구역이 늘수록 조건문도 늘고, 어느 순간 구역과 행동이 통째로 얽힌다.

이 프로젝트는 그 얽힘을 풀기 위한 실험이다. **어디(Zone)와 무엇(Mode)을 분리**하면: 어떤 구역에서도 원하는 행동을 자유롭게 조합하고, 하나를 바꿔도 나머지가 흔들리지 않는 구조가 된다. 시스템 구현과 함께 실제 콘텐츠로 그 가능성을 검증한다. → [로드맵](#로드맵)

개발 전반에서 AI를 파트너로 활용한다. 코드를 대신 써주는 도구가 아니라, 흐름을 함께 추적하면서 다음 아키텍처 질문을 먼저 꺼내는 역할이다. → [개발 방식](#개발-방식)

→ 현재 탐색 중인 아키텍처 질문: [docs/project-goals.md](docs/project-goals.md)

---

## 실행 방법

- Unity `6000.3.10f1`, URP.
- 클론 후 Unity Hub로 열고, `Assets/ZoneFlowAssets/Scenes/DevBootstrap.unity`를 열어 Play.
- 주요 에디터 도구: `ZoneFlow/Bake Catalogs`(Zone·Panel 카탈로그 재생성), `ZoneFlow/Runtime State`(런타임 상태 뷰), `ZoneFlow/Create Zone...`(Zone 신규 생성).
- 주요 의존성(`Packages/manifest.json`): UniTask, Yarn Spinner Unity, Unity Localization 1.5.12(Addressables를 전이 의존으로 끌어들임), Cinemachine 3.1.6, PrimeTween 1.4.0, UniVRM 0.131.1, Input System 1.18.0.
- 빌드는 CLI 스크립트 없이 Unity Editor의 File > Build Settings를 사용한다.

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

> Navigation 호스트(`NavigationHost` enum)에는 위 5종 외에 제어용 `Pop`(이전 모드 복귀)·`Portal`(포털 리다이렉트) 두 값이 더 있다. Mode를 구현하지 않는 내비게이션 전용 값이라 표에서는 제외했다.

---

## 아키텍처

런타임은 4계층으로 구성된다. 의존 방향은 위 → 아래 단방향이다.

| 계층 | 역할 |
| --- | --- |
| **Service** | 영속적 시스템. 게임 전체에서 하나만 존재하며 다른 계층에서 참조한다. 시뮬 백본(TimeService·PartyService·SaveService)이 여기 상주할 예정이다 ([ADR-0001](docs/decisions/0001-sim-state-in-service-layer.md)) |
| **Scene** | Unity 씬의 로드·언로드를 조율한다 |
| **Zone** | 게임플레이 공간 단위. 씬 위에서 생명주기를 관리한다 |
| **Mode** | 플레이어의 현재 행동 상태. Zone 위에서 동작하며 스택으로 관리된다 |

상세 → [docs/architecture/](docs/architecture/)

### 패키지 경계

`Assets/ZoneFlowAssets/`(게임)와 `Assets/PolyglotAssets/`(다국어 폰트 엔진)는 별도 asmdef 패키지로 분리되어 있다. Polyglot은 `com.zoneflow.polyglot`이라는 독립 UPM 패키지로, MonoBehaviour 서비스에 의존하지 않는 순수 엔진 코드다 — 게임 쪽은 얇은 어댑터 `FontService`를 통해서만 접근한다.

이 경계는 설계 선택이 아니라 강제된 결과였다. asmdef는 `Assembly-CSharp`(게임 코드)를 역참조할 수 없기 때문에, Polyglot을 별도 asmdef로 두는 순간 게임 타입 의존을 자체적으로 배제해야 했다. 프로젝트의 첫 패키지 경계 사례이고, 이 결정과 후속 문제(비동기 재진입·런타임 로케일 전환)는 [ADR-0005](docs/decisions/0005-first-asmdef-package-boundary-polyglot.md)~[0008](docs/decisions/0008-runtime-locale-switch-ui-localization.md)에 기록되어 있다.

---

## 지금까지 만든 것

### 게임플레이 기반

| 항목 | 설명 |
| --- | --- |
| Navigation | `gameplay://` URI로 Mode/Zone을 전환하는 내비게이션 시스템 (`NavigationRequest`, `GamePlayDirector`) |
| Zone 생명주기 | 참조 카운팅 기반 자동 로드/해제 (`ZoneRegistry`) |
| Bootstrap | `Bootstrap`/`ColdStartup`/`DevBootstrap` — 어느 Zone 씬에서든 직접 진입 가능 |
| Player | 상태 머신(Idle/Move/Sprint)·입력·애니메이터 (`Runtime/Player/`) |
| Interaction | 근접 감지·프롬프트 패널, Portal/월드 라벨 (`InteractionDetector`, `BillboardLabel`) |
| 멀티존 던전 체인 | `dungeon_0~4` 등 9개 Zone이 5개 씬(Intro/Village/Dungeon/BossRoom/Overworld)을 공유 (`ZoneAssetCatalog`) |

### 전투 (완료)

| 항목 | 설명 |
| --- | --- |
| 결정론적 헤드리스 엔진 | MonoBehaviour 없는 순수 C# 턴 해석 + 시드 주입 RNG (`BattleEngine`, `BattleRng`, [ADR-0004](docs/decisions/0004-deterministic-headless-battle-engine.md)) |
| 인터랙티브 턴 루프 | `BattleMode`(291줄) + `BattlePanel`(490줄) — 대상 선택·스킬 실행 |
| 결과 채널 | URI에 결과를 싣지 않고 `BattleOutcomeChannel`을 pull ([ADR-0002](docs/decisions/0002-battle-return-result-channel.md)) |
| 전투 연출 | VRM 액터 스테이징·데미지 넘버 (`BattleView/`, UniVRM 0.131.1) |
| 데이터 | `SkillAsset`/`PersonaAsset`/`EnemyAsset`/`BattleEncounterAsset` ScriptableObject + 저작 콘텐츠 |

### 다국어·전달 (Polyglot)

| 항목 | 설명 |
| --- | --- |
| 다국어 TMP 폰트 엔진 | `FontRuntime`/`FontEngine`/`PolyglotText`, `IFontProvider` 심(seam)으로 로딩 방식 교체 가능 |
| Localization Asset Table 경유 로딩 | Unity Localization Asset Table을 폰트 소스로 사용 ([ADR-0006](docs/decisions/0006-polyglot-font-loading-localization-asset-table.md)), 로케일 en/ko/ja/zh-Hans |
| 런타임 로케일 전환 | Intro 최초 실행 언어 선택 + 메뉴 드롭다운 ([ADR-0008](docs/decisions/0008-runtime-locale-switch-ui-localization.md)) |
| 티어 폰트 로딩 | 부팅용 서브셋 폰트를 로컬로 굽고, 콘텐츠 폰트는 원격(Remote) 전달 — 로케일당 CJK 폰트 페이로드 문제 대응 (AQ-11) |
| 에디터 안전장치 | 폰트 스트립 저장 왕복 불변식, 아틀라스 오염 가드, driven property 등록 (`FontStripProcessor`, `FontAtlasGuard`, `TextDrivenPropertyRegistrar`) |

### UI·연출

| 항목 | 설명 |
| --- | --- |
| UiLayer 스택 | 7단 레이어(`UiLayer`) + `PanelCatalog`(battle/dialogue/exploration-hud/interaction-prompt/menu/story-hud) |
| 전환 효과 | 페이드·인스턴트 블랙 (`TransitionFx/`) |
| Dialogue | Yarn Spinner 기반 대사·내러티브, `DialogueService`로 Zone 전환 간 진행 상태 보존 |

---

## 검증

| 어셈블리 | 대상 | 파일 |
| --- | --- | --- |
| `ZoneFlow.Tests.Editor` | 전투 엔진·데미지·턴 순서·결과 채널, 부팅 폰트 스타일시트 | `Assets/ZoneFlowAssets/Tests/Editor/` |
| `ZoneFlow.Tests.Runtime` | 내비게이션 왕복, 스토리 진행 상태의 Zone 간 보존(AQ-2), 상호작용 감지, MonoService | `Assets/ZoneFlowAssets/Tests/Runtime/` |
| `Polyglot.Editor.Tests` | 폰트 스트립 왕복, 폰트 엔진, 스타일시트, driven 직렬화 | `Assets/PolyglotAssets/Tests/Editor/` |

17개 파일에 걸쳐 테스트 어트리뷰트(`[Test]`/`[UnityTest]`) 61개. 실행: Unity Editor → Window > General > Test Runner.

---

## 로드맵

Persona5형 수직 슬라이스(`캘린더 1일 → 던전 → 턴제 전투 → 귀가 → 날짜 진행`)를 두 트랙으로 나눠 진행한다. 아키텍처 결정 근거는 [docs/decisions/](docs/decisions/), 탐색 산출물은 [explorations/persona5-slice/findings.md](explorations/persona5-slice/findings.md) 참조.

### 수직 슬라이스 트랙

| 단계 | 상태 |
| --- | --- |
| 아키텍처 매핑 | 완료 |
| 역할 기반 에이전트 셋업 | 완료 |
| 전투 수직 | 완료 — 헤드리스 엔진부터 연출까지 |
| 백본 서비스 (Time·Party·Save) | 다음 — `TimeService`/`PartyService`/`SaveService` 파일은 아직 없고, ADR-0001/0003이 설계만 선행한 상태 |
| 시뮬 루프 배선 (UI·Zone) | 예정 |
| 통합·저장 검증 | 예정 |

### 플랫폼·전달 트랙

| 단계 | 상태 |
| --- | --- |
| 다국어 폰트 엔진 (Polyglot 패키지) | 완료 |
| 런타임 로케일 전환 | 완료 |
| 티어 폰트 로딩 (AQ-11) | 완료 |
| WebGL 빌드 검증 | 예정 |

Phase 번호는 두지 않는다 — 재번호가 필요해질 때마다 기존 이슈·문서 참조가 어긋나기 때문이다.

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
| **technical-writer** | 프로젝트 최상위 문서 작성·개편 (현재 README 한정) | 없음 — 문서 작성/정리 요청 시 자동 위임 |

작업 복잡도에 따라 모델 티어(haiku/sonnet/opus)를 자동 라우팅하고, 특정 경로의 파일을 수정하기 전에는 대응하는 **path-scoped rules**([.claude/rules/](.claude/rules/), 경로별 7개)를 먼저 적용한다. 에이전트별로 프로젝트 메모리([.claude/agent-memory/](.claude/agent-memory/))를 남겨 이전 판단·피드백을 다음 작업에 이어간다. 이 프로젝트에서 AI 협업은 장식이 아니라 hook([.claude/hooks/](.claude/hooks/))으로 강제되는 규약이다. 상세 기준은 [CLAUDE.md](CLAUDE.md)·[.claude/docs/complexity.md](.claude/docs/complexity.md) 참조.

### 커스텀 커맨드

| 커맨드 | 용도 |
| --- | --- |
| `/explore` `/feature` `/issue` | 탐색 → 설계 → 구현 코어 흐름 |
| `/level` `/ui` | 존/레벨·UI 콘텐츠 저작 |
| `/battle` `/systems` | 전투·시뮬 시스템 설계·구현 |
| `/next` `/quick` `/bridge` | 흐름 오케스트레이션·소규모 작업·세션 인수인계 |
| `/git-commit` `/gh-sync` `/work-log` | 커밋·GitHub 이슈 동기화·작업 로그 |

탐색 및 Feature 인덱스 → [BACKLOG.md](BACKLOG.md)

---

## 문서

| | |
| --- | --- |
| [docs/project-goals.md](docs/project-goals.md) | 프로젝트 목표 + 탐색 중인 아키텍처 질문 |
| [docs/architecture/](docs/architecture/) | 씬 계층·시스템 계층·제약 원칙 |
| [docs/decisions/](docs/decisions/) | 아키텍처 결정 기록 (ADR) — 시뮬 상태·전투·Polyglot 패키지 경계·로케일 전환 등 0001~0008 |
| [docs/conventions/coding-style.md](docs/conventions/coding-style.md) | 코딩 규칙 |
| [.claude/agents/](.claude/agents/) · [.claude/rules/](.claude/rules/) | 역할 기반 에이전트 정의·경로별 규칙 |
| [.claude/docs/complexity.md](.claude/docs/complexity.md) | 작업 복잡도 평가·모델 티어 선택 기준 |
| [BACKLOG.md](BACKLOG.md) | Feature·Exploration 인덱스 + Architectural Questions 추적 |
