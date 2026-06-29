---
paths:
  - "Assets/ZoneFlowAssets/Runtime/Data/**"
---

# Rule: ScriptableObject·데이터

`Assets/ZoneFlowAssets/Runtime/Data/**`(SO 카탈로그 등 코드 결합 데이터)를 다룰 때 적용한다.
원문(canonical): [docs/architecture/constraints.md](../../docs/architecture/constraints.md), [docs/architecture/project-structure.md](../../docs/architecture/project-structure.md).

## 필수

- **SO 씬 이름은 `so.name`(에셋 파일명)** — 별도 `SerializeField` 필드를 두지 않는다.
- **레지스트리 접근은 Inspector 직렬화 우선.** `Resources.Load`는 `CoreServices.asset` + PrefabZone 전용 Resources 폴더에만 허용.
- **코드 결합 에셋은 `Runtime/Data`·`Runtime/Prefabs`에**, 순수 오서링 콘텐츠(씬·머티리얼·스프라이트)는 패키지 최상위에 둔다.
- 에셋 이동 시 `.asset`/`.prefab`과 `.meta`(GUID)를 **항상 함께** 옮긴다 — 참조는 GUID 기반.
