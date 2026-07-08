---
name: multi-monobehaviour-per-file-broken-script-ref
description: 한 .cs 파일에 여러 MonoBehaviour 클래스를 정의하면 AddComponent 시 m_Script가 fileID:0(깨짐)으로 직렬화될 수 있음
metadata:
  type: feedback
---

패널의 런타임 복제용 보조 MonoBehaviour(예: 로우/버튼 참조 홀더)를 패널 본체와 **같은 .cs 파일**에 `internal sealed class`로 정의하고, 스크립트 컴파일 직후(같은 세션 내) `[ContextMenu]` 빌더에서 `AddComponent<T>()`로 즉시 부착하면 저장된 프리팹의 `m_Script`가 `{fileID: 0}`으로 깨져 직렬화되는 경우를 확인했다(BattlePanel의 `BattleCombatantRow`/`BattleActionButton`).

**증상**: `GetComponent<T>()`가 null을 반환(에디터 인스펙터에는 컴포넌트가 붙어 보이지만 스크립트 참조가 없음). `unity_component_get_properties`로 `m_Script` 프로퍼티를 직접 확인하면 잡힌다.

**Why**: Unity가 한 파일 내 보조(비-파일명 매칭) 클래스의 MonoScript GUID 역인덱스를 컴파일 직후 즉시 완전히 채우지 못하는 것으로 보임(도메인 리로드 타이밍 이슈로 추정). 별도 파일로 분리하면 즉시 해결됨.

**How to apply**: 패널이 런타임에 복제해 쓰는 보조 MonoBehaviour(로우/버튼 등 참조 홀더)는 **항상 별도 .cs 파일**로 분리해 작성한다. 같은 세션에서 스크립트 생성 → 컴파일 → 즉시 프리팹 빌더 실행 흐름을 탈 때는 특히 주의. 프리팹 저장 후 `unity_component_get_properties`로 `m_Script`가 실제 MonoScript를 가리키는지(단순히 컴포넌트 존재 여부가 아니라) 검증하는 습관을 들인다.
