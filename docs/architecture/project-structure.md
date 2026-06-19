# Project structure

- `Assets/ZoneFlowAssets/` — main framework package, follows UPM package layout
  - Internal layout: `Runtime/`, `Editor/`, `Tests/`, `.asmdef`
- Additional independent packages live at the same level as `ZoneFlowAssets/`
  - Naming: `{PackageName}Assets/` with its own UPM layout

## 에셋 배치 기준

원칙: **코드와 한 몸인 에셋은 `Runtime/` 안, 순수 오서링 콘텐츠는 패키지 최상위.**

| 분류 | 위치 | 예 |
| --- | --- | --- |
| **코드 결합 에셋** — Runtime 스크립트가 직접 정의/소유하거나 그 런타임 표현인 것 | `ZoneFlowAssets/Runtime/{Data,Prefabs}/` | ScriptableObject 카탈로그(`Runtime/Data`), Player·UI 패널 프리팹(`Runtime/Prefabs`) |
| **순수 오서링 콘텐츠** — 컴파일되지 않고 에디터에서 구성하거나, 여러 시스템·프리팹을 조합하는 합성 루트 | `ZoneFlowAssets/` 최상위 | `.unity` 씬(`Scenes`), 머티리얼(`Materials`), 스프라이트(`Sprites`) |

- **프리팹은 `Runtime/Prefabs/` 단일 폴더로 통합** — 모듈 폴더에 분산 co-locate하지 않는다.
- **씬은 최상위 `Scenes/`에 둔다** — 여러 시스템·프리팹을 조합하는 합성 루트라 특정 모듈 소유가 아니다.
- 에셋 이동 시 `.prefab`/`.asset`과 `.meta`(GUID)를 항상 함께 옮긴다. 참조는 GUID 기반이라 `.meta`가 보존되면 깨지지 않는다.
- Key Dependencies
  - **UniTask** — async/await; used for scene loading, transitions, UI sequences
  - **URP 17.3.0** — render pipeline
- Claude 관련 md 파일
  - CLAUDE.md 가 참조 하는 파일은 docs 밑에 구조적으로 배치
  - `docs/conventions/` — 코딩 규칙, 프로젝트 구조 등 스타일 가이드
  - `docs/architecture/` — 아키텍처 관련 기능 설명
  - `.claude/docs/complexity.md` — 작업 복잡도 평가 및 모델 선택 기준 (Claude 런타임 전용)
