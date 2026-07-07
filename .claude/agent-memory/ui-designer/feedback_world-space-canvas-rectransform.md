---
name: world-space-canvas-rectransform
description: World Space Canvas RectTransform은 m_AnchoredPosition이 실제 위치를 결정 — m_LocalPosition만으로는 씬에 위치가 저장되지 않는다
metadata:
  type: feedback
---

World Space Canvas의 RectTransform은 `m_AnchoredPosition`이 실제 위치를 결정한다. `m_LocalPosition`만 세팅하면 씬 저장 후 y=0으로 리셋된다.

**Why:** Unity의 RectTransform은 anchoredPosition을 우선 사용하기 때문에 Transform.position/localPosition 세팅이 씬 직렬화에 반영되지 않는다. 프리팹 인스턴스 수정도 마찬가지.

**How to apply:** 씬 YAML 파일을 직접 편집할 때 반드시 `m_LocalPosition`과 `m_AnchoredPosition` 모두 올바른 값으로 기록한다. unity_execute_code로 SerializedObject를 써서 anchoredPosition을 세팅하거나, 씬 파일 직접 편집 시 두 필드를 동시에 수정한다.

관련: [[billboard-label-cinemachine-camera]]
