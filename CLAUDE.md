# CLAUDE.md

## Build & Test

- **Unity**: `6000.3.10f1` (URP). 실행 진입점은 `Assets/ZoneFlowAssets/Scenes/DevBootstrap.unity` → Play. Zone 씬 직접 Play는 ColdStartup 경로다.
- **Build**: Unity Editor → File > Build Settings (no CLI build scripts)
- **Tests**: Unity Editor → Window > General > Test Runner
  - Editor tests: `Assets/ZoneFlowAssets/Tests/Editor`
  - Runtime tests: `Assets/ZoneFlowAssets/Tests/Runtime`
  - Polyglot 패키지: `Assets/PolyglotAssets/Tests/{Editor,Runtime}`
  - Claude 실행: `unity_advanced_tool` → `unity_testing_run_tests` (결과의 `failed` 값이 유효 신호)
  - 실행 전 `unity_get_compilation_errors`로 컴파일 상태를 먼저 확인한다.
- **에디터 메뉴**: `ZoneFlow/Bake Catalogs`(= `CatalogBaker.BakeAll`), `ZoneFlow/Runtime State`, `ZoneFlow/Create Zone...` — Polyglot은 `Tools/Polyglot/*`.
- **카탈로그 베이크**: `Runtime/Data/*.asset` 4종은 CatalogBaker 출력물 — 손편집 금지, 부분 베이크 없음. 서브에이전트는 베이크하지 않고 **메인 세션이 작업 종료 시 `BakeAll`을 1회 직렬 실행**한다.
- **커밋 메시지**: `[feat|fix|refactor|style|test|docs|chore] 한국어 명사형 요약` (마침표 없음, 해당 시 `Closes #N`). `Assets/` 변경은 `.meta` 동반. 상세 흐름은 `/git-commit`.

## Collaboration Protocol

- **이슈 전달**: 결론 + 이유 1가지 세트로 전달
- **맥락 부족 시**: 작업 전에 즉시 피드백 — 유추해서 넘어가지 않음
- **이슈 혼합 시**: "이슈가 두 개 섞인 것 같아"라고 명시하고 분리 제안

## Coding Discipline

- **단순함 우선**: 요청한 문제만 최소 코드로 해결. 요청 없는 기능·추측성 추상화·1회용 유연성·일어나지 않을 예외 처리는 추가하지 않는다.
- **수술적 변경**: 변경한 모든 줄은 요청으로 직접 추적되어야 한다. 기존 스타일 보존, 무관한 코드 리팩터·죽은 코드 정리 금지 — 내 변경이 만든 의존성만 제거한다.
- 나머지 두 원칙은 기존 위치가 담당한다: "사고 우선(유추 금지)" → `Collaboration Protocol`·rules `## 모호하면`, "목표 주도(검증 기준)" → `Operational Rules`·feature-spec `검증 방법`.

## Operational Rules

- **Plan Phase**: Plan 모드 진입 시 아래 기준으로 복잡도를 먼저 평가할 것.
  - **Low → haiku**: 파일 읽기·검색, 단순 편집, 커밋 메시지, 규칙 적용
  - **Medium → sonnet**: 단일 시스템 기능 구현, 버그 수정, 리팩터링, 코드 리뷰
  - **High → opus**: 아키텍처 설계, 시스템 간 연동, 다중 패키지 구조 변경, 새 패턴 도입
  - (모델은 별칭으로 지정 — 별칭은 현재 최신 모델로 자동 해석되므로 버전 번호를 고정하지 않는다. 상세 기준: `.claude/docs/complexity.md`)
- **Command Execution**: `UserPromptSubmit` hook(`complexity-hint.ps1`)이 슬래시 커맨드를 자동 감지하여 복잡도를 주입한다. Hook 출력을 반드시 따를 것.
  - **Low** (`/git-commit`, `/bridge`, `/work-log`, `/quick`, `/issue new|list|show|close`, `/feature new|list|show`): Agent 도구로 `model='haiku'` 서브에이전트를 생성하여 전체 작업 위임
  - **Medium** (`/init`, `/simplify`, `/next`, `/issue do`, `/feature plan`, `/level`·`/ui`·`/battle`·`/systems`의 `new|improve|tune|review`): 현재 모델 유지, 알림 없음. **구현 위임은 `unity-specialist`**(`/issue do`), 역할 커맨드는 각자의 에이전트로 위임.
  - **High** (`/security-review`, `/explore`, `/issue review`): 작업 시작 전 사용자에게 Opus 모델 전환 여부 확인. **설계·검토 위임은 `architecture-director` 에이전트로** (`/explore`, `/issue review`).
- **구현 워크플로우**: Plan 승인 후 아래 기준으로 후속 액션을 결정할 것.
  - **Implementation Plan** (기능 구현·리팩터링·버그 수정 등 코드 변경 수반):
    1. **Plan 모드** 진입 → 설계 정리 및 사용자 승인
    2. 승인 후 `/issue new task`로 TASK 등록
    3. `/issue do <#>`로 구현 시작
    - 단순 버그 수정·1줄 변경은 예외 (이슈 생략 가능)
  - **Analysis Plan** (코드 리뷰·보안 리뷰·설계 검토 등 코드 변경 없음):
    1. **Plan 모드** 진입 → 분석 범위 정리 및 사용자 승인
    2. 승인 후 바로 작업 실행 (이슈 등록·`/issue do` 생략)

## Subagents (역할 기반)

complexity-routing이 *tier(모델)*를, 아래 에이전트가 *role(정체성)*을 담당한다 — 추가일 뿐 충돌 없음. 정의: `.claude/agents/`.

- **`architecture-director`** (Opus) — Zone-Mode 분리 검토, AQ 발견·제안, 시스템 간 연동 설계. `/explore`·`/issue review`가 위임. 설계 중심(읽기 위주).
- **`unity-specialist`** (Sonnet) — Unity API·구현 권위자, `unity_*` MCP로 에디터 조작, 경로 rules 강제. `/issue do`가 위임.
- **`level-designer`** (Sonnet) — 존/레벨 콘텐츠 설계·저작(레이아웃·연결성·페이싱·상호작용/내러티브 + 공간에 내재한 아트 디렉션). `unity_*` MCP로 씬 저작. `/level`이 위임. (art-director는 머티리얼/라이팅 파이프라인이 반복되면 분화할 미래 후보 — 현재는 본 에이전트가 흡수.)
- **`ui-designer`** (Sonnet) — UI/HUD/패널 설계·저작(정보설계·레이아웃·프리팹, PanelCatalog 등록). `unity_*` MCP로 패널 저작. `/ui`가 위임. 시스템 코드·Mode↔Panel 매핑은 unity-specialist로 에스컬레이션.
- **`combat-specialist`** (Sonnet) — 턴제 전투 설계·구현(BattleMode·BattleService·스킬/페르소나 데이터). 전투 결과는 모드 간 결과 채널(ADR-0002), 파티/스탯은 읽기만. `/battle`이 위임. 아키텍처는 architecture-director, 시뮬 데이터는 systems-designer로 에스컬레이션.
- **`systems-designer`** (Sonnet) — 시뮬 시스템·데이터 모델(시간·파티·세이브·인벤 Service + SO/POCO). 시뮬 전역 상태는 Service 계층(ADR-0001), Save/Load는 ISaveable 순회·부분 복원(ADR-0003). `/systems`가 위임. 전투 로직은 combat-specialist로 에스컬레이션.
- **`technical-writer`** (Sonnet) — 프로젝트 최상위 문서 작성·개편(현재 README.md 한정, 추후 CONTRIBUTING·docs/ 확장). 코드·git 히스토리로 실제 동작을 검증한 뒤 서술하며, 최종 사용자/기여자 섹션을 분리한다. 전용 커맨드 없음 — "README 작성/정리/문서화" 요청 시 자동 위임.

주의: 서브에이전트는 `AskUserQuestion`·`ExitPlanMode`를 쓸 수 없다 → **사용자 승인 게이트는 메인 세션이 중재**한다. 에이전트 파일을 디스크에서 새로 추가/수정하면 **세션 재시작 후** 로드된다.

## Path-Scoped Rules

특정 경로의 파일을 Edit/Write 하기 **전에** 대응 rule을 먼저 읽고 적용한다. 정의: `.claude/rules/` (frontmatter `paths:` glob). `docs/`가 canonical source이며 rule은 경로별 핵심만 추출·링크.

| glob | rule |
| --- | --- |
| `Assets/ZoneFlowAssets/Runtime/**`, `Assets/PolyglotAssets/Runtime/**` | `runtime-code.md` |
| `Assets/**/Editor/**` | `editor-code.md` |
| `Assets/ZoneFlowAssets/Runtime/Data/**` | `scriptable-data.md` |
| `Assets/ZoneFlowAssets/Tests/**`, `Assets/PolyglotAssets/Tests/**` | `tests.md` |
| `Assets/ZoneFlowAssets/Scenes/**`, `Assets/ZoneFlowAssets/Story/**` | `level-content.md` |
| `Assets/ZoneFlowAssets/Runtime/Ui/**`, `Runtime/Prefabs/**`(UI 패널 한정) | `ui-design.md` (`runtime-code.md`와 동시 매칭) |
| `Assets/ZoneFlowAssets/Runtime/GamePlay/Battle/**`, `Runtime/GamePlay/ModeImpl/BattleMode.cs` | `combat-code.md` (`runtime-code.md`와 동시 매칭) |

**제2 패키지**: `Assets/PolyglotAssets/`(폰트·로컬라이제이션, asmdef 4개 — ADR-0005). `runtime-code.md`·`tests.md`·`editor-code.md`가 함께 매칭된다. `ZoneFlow.Runtime`을 참조하지 않는 독립 패키지이므로 씬 로딩·Bootstrap 실행 순서 규칙은 해당 없다.

## Templates

- `.claude/templates/architecture-decision.md` — ADR. 채워서 `docs/decisions/`에 저장 (constraints.md가 참조).
- `.claude/templates/feature-spec.md` — `/feature`·`/issue` 흐름의 설계 입력.
- 산출 위치: `features/<name>/{spec,decisions,tasks,testcases}.md`, 인덱스는 루트 `BACKLOG.md`.

## Architectural Principles

→ [docs/architecture/constraints.md](docs/architecture/constraints.md)
→ 학습 목표·미해결 Architectural Question(AQ): [docs/project-goals.md](docs/project-goals.md)
→ 설계 결정 기록(ADR-0001~0008): [docs/decisions/](docs/decisions/)

## Architecture & Coding Style

→ 프로젝트 개요·실행 방법·로드맵: [README.md](README.md)
→ 씬 계층·시스템 계층: [docs/architecture/](docs/architecture/)
→ 코딩 스타일·네이밍: [docs/conventions/coding-style.md](docs/conventions/coding-style.md)
→ 프로젝트 구조: [docs/architecture/project-structure.md](docs/architecture/project-structure.md)

## Custom Commands

- `/next [feature|#]` — 통합 진입점: 상태 자동 감지 → feature plan·이슈 생성·구현 흐름 오케스트레이션
- `/quick <desc>` — 이슈 없이 소규모 작업 즉시 처리 (설정 변경, 문서 수정 등)
- `/bridge` — 세션 컨텍스트 압축 및 인수인계 (Resume 커맨드 포함)
- `/git-commit` — staged 파일 선택 → .meta 자동 처리 → 커밋 (버튼 확인)
- `/issue` — TASK 등록·조회·상태 변경 (`/issue new`, `do`, `review`, `close`)
- `/explore` — 아키텍처 탐색 및 후보 비교
- `/level` — 존/레벨 콘텐츠 설계·저작 (`/level list|new|improve|review`) → `level-designer` 위임
- `/ui` — UI/HUD/패널 설계·저작 (`/ui list|new|improve|review`) → `ui-designer` 위임
- `/battle` — 턴제 전투 설계·구현 (`/battle list|new|tune|review`) → `combat-specialist` 위임
- `/systems` — 시뮬 시스템·데이터 모델 설계·구현 (`/systems list|new|improve|review`) → `systems-designer` 위임
- `/feature` — 기능 스펙 작성·조회 (`/feature new|list|show|plan`)
- `/work-log` — 마지막 실행 이후 커밋 분석 → 설계 의도 중심 업무 보고서 생성
- `/gh-sync` — GitHub 동기화 (`/gh-sync issues` 이슈 상태, `/gh-sync board` 프로젝트 보드 → `tasks.md` 역방향)
