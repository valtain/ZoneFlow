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
  - **Low** (`/git-commit`, `/bridge`, `/work-log`, `/quick`, `/issue new|list|show|close`, `/feature new|list|show`): Agent 도구로 `model='haiku'` 서브에이전트를 생성하여 전체 작업 위임
  - **Medium** (`/init`, `/review`, `/next`, `/issue do`, `/feature plan`): 현재 모델 유지, 알림 없음
  - **High** (`/security-review`, `/explore`, `/issue review`): 작업 시작 전 사용자에게 Opus 모델 전환 여부 확인
- **구현 워크플로우**: Plan 승인 후 아래 기준으로 후속 액션을 결정할 것.
  - **Implementation Plan** (기능 구현·리팩터링·버그 수정 등 코드 변경 수반):
    1. **Plan 모드** 진입 → 설계 정리 및 사용자 승인
    2. 승인 후 `/issue new task`로 TASK 등록
    3. `/issue do <#>`로 구현 시작
    - 단순 버그 수정·1줄 변경은 예외 (이슈 생략 가능)
  - **Analysis Plan** (코드 리뷰·보안 리뷰·설계 검토 등 코드 변경 없음):
    1. **Plan 모드** 진입 → 분석 범위 정리 및 사용자 승인
    2. 승인 후 바로 작업 실행 (이슈 등록·`/issue do` 생략)

## Architectural Principles

→ [memory/architecture-principles.md](memory/architecture-principles.md)

## Architecture & Coding Style

→ 씬 계층·시스템 계층: [docs/architecture/](docs/architecture/)
→ 코딩 스타일·네이밍: [memory/naming-conventions.md](memory/naming-conventions.md) · [docs/conventions/coding-style.md](docs/conventions/coding-style.md)
→ 프로젝트 구조: [docs/architecture/project-structure.md](docs/architecture/project-structure.md)

## Custom Commands

- `/next [feature|#]` — 통합 진입점: 상태 자동 감지 → feature plan·이슈 생성·구현 흐름 오케스트레이션
- `/quick <desc>` — 이슈 없이 소규모 작업 즉시 처리 (설정 변경, 문서 수정 등)
- `/bridge` — 세션 컨텍스트 압축 및 인수인계 (Resume 커맨드 포함)
- `/git-commit` — staged 파일 선택 → .meta 자동 처리 → 커밋 (버튼 확인)
- `/issue` — TASK 등록·조회·상태 변경 (`/issue new`, `do`, `review`, `close`)
- `/explore` — 아키텍처 탐색 및 후보 비교
