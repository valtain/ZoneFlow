---
name: feedback_mcp_prefab_editing
description: unity_* MCP로 프리팹의 RectTransform/UI 구조를 라이브 편집할 때의 함정 2가지(도메인 리로드 데이터 손실, dirty 씬 전환 모달)
metadata:
  type: feedback
---

## 함정 1 — AssetDatabase.Refresh() 도메인 리로드가 라이브 씬 오브젝트의 미저장 프로퍼티를 되돌린다

커스텀 컴포넌트(예: BattleActorView)에 새 [SerializeField]를 추가한 직후, 씬에 인스턴스화한 프리팹 편집
세션에서 `unity_component_get_properties`가 새 필드를 못 찾는 경우가 있다(리플렉션 캐시 문제로 보임).
`AssetDatabase.Refresh()`(execute_code)로 해결되지만, **그 직후 도메인 리로드가 Refresh 이전에
`unity_component_set_property`로 바꾼 RectTransform anchorMin/anchorMax/sizeDelta·Image color 같은
가치들을 기본값으로 되돌렸다**(반면 TMP의 text/fontSize 같은 일부 속성은 살아남음 — 정확한 경계 불명).

**Why:** 근본 원인은 불명확하지만(라이브 오브젝트가 SetDirty/Undo 기록 없이 필드만 바뀐 상태였을 가능성),
재현 패턴은 명확하다: Refresh 호출 전에 세팅한 RectTransform/Color 값이 저장된 프리팹에 반영되지
않았다(構造·와이어링은 살아남았다 — GameObject 계층과 ObjectReference 필드는 안전).

**How to apply:**
- 새 필드 추가로 `component_get_properties`가 필드를 못 찾으면, **Refresh를 가장 먼저(자식 구조/속성을
  세팅하기 전에) 한 번 실행**하고, 그 다음에 anchor/color/enum 속성을 세팅한다.
- 프리팹으로 저장(`unity_asset_create_prefab`)하기 **직전에 반드시 `get_properties`로 RectTransform/
  Image 값을 재검증**한다 — Success 응답이 와도 실제 값이 반영됐는지 별도로 확인해야 한다.
- 값이 안 맞으면, 저장된 `.prefab` YAML을 `Read`로 열어 정확한 수치를 `Edit`로 직접 고치는 게 가장
  신뢰할 수 있다(구조/fileID/와이어링은 이미 올바르므로 숫자만 교정하면 됨). #93에서 이 방식으로 복구.

## 함정 2 — dirty 씬에서 unity_scene_open으로 전환하면 저장 확인 모달이 브리지를 멈춘다

씬에 임시 GameObject를 만들고 지운 뒤에도 씬은 dirty로 남는다. 이 상태에서 다른 씬으로
`unity_scene_open`을 호출하면 Unity가 "변경사항을 저장하시겠습니까?" 모달을 띄우고, MCP 브리지가
포트는 열려 있지만 요청에 응답하지 않는 상태가 된다(대화창 클릭 도구가 없어 자력 복구 불가).

**Why:** 모달은 Unity 메인 스레드를 블록하고, MCP 요청 처리도 메인 스레드 의존적이라 응답이 전혀
오지 않는다(포트 TCP 연결은 되지만 HTTP 응답이 없음 — `unity_editor_ping`/`unity_list_instances`가
"not reachable"/빈 목록으로 보임).

**How to apply:** 씬을 여러 번 열고 닫는 편집 세션(프리팹 임시 인스턴스화 등)에서는, 다음 씬으로
전환하기 전에 **`unity_scene_save`를 호출해 dirty 상태를 없앤다** — 특히 사람이 자리를 비울 수 있는
자동화 세션에서는 필수. 이미 블록됐다면 사용자에게 Unity Editor 창에서 모달을 직접 닫아달라고
요청해야 한다(원격 도구로는 복구 불가).
