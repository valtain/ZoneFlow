# Project structure

- `Assets/ZoneFlowAssets/` — main framework package, follows UPM package layout
  - Internal layout: `Runtime/`, `Editor/`, `Tests/`, `.asmdef`
- Additional independent packages live at the same level as `ZoneFlowAssets/`
  - Naming: `{PackageName}Assets/` with its own UPM layout
- Key Dependencies
  - **UniTask** — async/await; used for scene loading, transitions, UI sequences
  - **URP 17.3.0** — render pipeline
- Claude 관련 md 파일
  - CLAUDE.md 가 참조 하는 파일은 docs 밑에 구조적으로 배치
  - `docs/conventions/` — 코딩 규칙, 프로젝트 구조 등 스타일 가이드
  - `docs/architecture/` — 아키텍처 관련 기능 설명
  - `.claude/docs/complexity.md` — 작업 복잡도 평가 및 모델 선택 기준 (Claude 런타임 전용)
