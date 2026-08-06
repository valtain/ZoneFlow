---
paths:
  - "Assets/ZoneFlowAssets/Tests/**"
  - "Assets/PolyglotAssets/Tests/**"
---

# Rule: 테스트

`Assets/ZoneFlowAssets/Tests/**`·`Assets/PolyglotAssets/Tests/**`의 파일을 다룰 때 적용한다.

## 필수

- **배치**: Editor 테스트는 `Tests/Editor`, Runtime(Play Mode) 테스트는 `Tests/Runtime`.
- 실행: Unity Editor → Window > General > Test Runner.
- 비동기 테스트도 **UniTask** 기반으로 작성한다(Runtime 코드 규칙과 동일, 코루틴 금지).
- 단언은 NUnit `Assert`. 프로덕션 코드의 `Debug.Assert` 불변식과 혼동하지 않는다.
- 테스트는 검증 대상 시스템의 공개 표면(서비스 인터페이스)을 통해 작성하고, 내부 구현 세부에 결합하지 않는다.
