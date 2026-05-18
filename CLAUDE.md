# CLAUDE.md

## Build & Test

- **Build**: Unity Editor → File > Build Settings (no CLI build scripts)
- **Tests**: Unity Editor → Window > General > Test Runner
  - Editor tests: `Assets/ZoneFlowAssets/Tests/Editor`
  - Runtime tests: `Assets/ZoneFlowAssets/Tests/Runtime`

## Collaboration Protocol

- **이슈 전달**: 결론 + 이유 1가지 세트로 전달
- **맥락 부족 시**: 작업 전에 즉시 피드백 — 유추해서 넘어가지 않음
- **이슈 혼합 시**: "이슈가 두 개 섞인 것 같아"라고 명시하고 분리 제안

## Operational Rules

- **Plan Phase**: Plan 모드 진입 시 아래 기준으로 복잡도를 먼저 평가할 것.
  - **Low → Haiku 4.5**: 파일 읽기·검색, 단순 편집, 커밋 메시지, 규칙 적용
  - **Medium → Sonnet 4.6**: 단일 시스템 기능 구현, 버그 수정, 리팩터링, 코드 리뷰
  - **High → Opus 4.7**: 아키텍처 설계, 시스템 간 연동, 다중 패키지 구조 변경, 새 패턴 도입
  - (상세 기준: `.claude/docs/complexity.md`)
- **Command Execution**: `UserPromptSubmit` hook(`complexity-hint.ps1`)이 슬래시 커맨드를 자동 감지하여 복잡도를 주입한다. Hook 출력을 반드시 따를 것.
  - **Low** (`/git-commit`, `/bridge`, `/work-log`): Agent 도구로 `model='haiku'` 서브에이전트를 생성하여 전체 작업 위임
  - **Medium** (`/init`, `/review`): 현재 모델 유지, 알림 없음
  - **High** (`/security-review`, `/explore`): 작업 시작 전 사용자에게 Opus 모델 전환 여부 확인
- **구현 워크플로우**: Plan 승인 후 아래 기준으로 후속 액션을 결정할 것.
  - **Implementation Plan** (기능 구현·리팩터링·버그 수정 등 코드 변경 수반):
    1. **Plan 모드** 진입 → 설계 정리 및 사용자 승인
    2. 승인 후 `/issue new task`로 TASK 등록
    3. 사용자에게 `/work-on` 시작 여부 확인 후 진행
    - 단순 버그 수정·1줄 변경은 예외 (이슈 생략 가능)
  - **Analysis Plan** (코드 리뷰·보안 리뷰·설계 검토 등 코드 변경 없음):
    1. **Plan 모드** 진입 → 분석 범위 정리 및 사용자 승인
    2. 승인 후 바로 작업 실행 (이슈 등록·`/work-on` 생략)

## Architectural Principles

불일치 시 구현 반복 발생.

| 원칙 | 결정 | 이유 |
| --- | --- | --- |
| 서비스 생성 책임 | **씬** (GameObject 배치) | 코드가 생성하면 씬 구조와 충돌 |
| DontDestroyOnLoad | **가급적 사용 안 함** | 씬 상주가 생명주기를 보장 |
| MonoService 자동 GameObject 생성 | **사용 안 함** | MonoService는 참조(ServiceLocator)만 |
| SO 씬 이름 | `so.name` (에셋 파일명) | 별도 SerializeField 불필요 |
| 레지스트리 접근 | Inspector 직렬화 우선 | Resources.Load는 CoreServices.asset + PrefabZone 전용 Resources 폴더만 허용 |
| 예외 처리 | `Debug.Assert` | `throw` 사용 안 함 |
| 비동기 | UniTask 전용 | Coroutine 혼용 금지 |
| ColdStartup 씬 재로드 | **unload → reload 유지** | 정상 Play 흐름과 동일한 초기화 순서 보장 목적 |
| 개발용 Bootstrap | **auto-managed 씬에 배치 금지** | `GamePlayBootstrap`은 `DevBootstrap.unity` 전용 |
| CoreServices·GamePlayServices 직접 로드 | **SceneService 경유 원칙** | 직접 `SceneManager.LoadSceneAsync` 호출 금지 |

### 씬 계층

| 씬 종류 | 예시 | 특징 |
| --- | --- | --- |
| **Auto-managed** | CoreServices, GamePlayServices | SceneService가 SceneType 기반으로 자동 로드; 수동 배치 금지 |
| **Content** | Zone0, Zone1 | ColdStartup 배치; SceneType=Zone → Prerequisites 자동 로드 후 reload |
| **Dev** | DevBootstrap | 개발 전용; GamePlayBootstrap 배치; 빌드 포함 금지 |

### 시스템 계층

| 계층 | 역할 |
| --- | --- |
| **Service** | 영속적 시스템 (ServiceLocator로 등록) |
| **Scene** | additive 씬 로드·언로드 오케스트레이션 |
| **Zone** | 게임플레이 컨텍스트 단위; 씬 그룹 소유 |
| **Mode** | Zone 내부의 게임플레이 행동 단위 |

## Architecture

### Package Layout

- `Assets/ZoneFlowAssets/` — main package (`Runtime/`, `Editor/`, `Tests/`, `.asmdef`)
- 독립 패키지: `{PackageName}Assets/` 동일 레벨

### Key Dependencies

| Package | Notes |
| --- | --- |
| UniTask | Async/await; git UPM |
| URP | Render pipeline |

## Coding Style

See [/.claude/docs/style/coding-style.md](/.claude/docs/style/coding-style.md) — key rules:

- **Acronyms as single words**: `HudView`, `_hudView` — not `HUDView`, `_UIPanel`
- **Inspector properties**: `[field: SerializeField]` with `public T Foo { get; private set; } = default;`
- **XML docs in Korean** on all `public` and `protected` members
- **Early init**: `[DefaultExecutionOrder(-1000)]`
- **No `#if UNITY_EDITOR`** in `Editor/` folder scripts

## Project Structure Conventions

See [/.claude/docs/style/project-structure.md](/.claude/docs/style/project-structure.md)

## Custom Commands

- `/bridge` — compresses session context into a handoff prompt for new conversations
- `/git-commit` — `.claude/commands/git-commit.md` 기반 워크플로우 수행
- `/issue` — TASK 등록·조회·상태 변경
- `/work-on` — Task 전체 생명주기 자동화 (구현 → 커밋 → 리뷰)
- `/explore` — 아키텍처 탐색 및 후보 비교
