# UI Designer Agent Memory

- [World Space Canvas RectTransform](feedback_world-space-canvas-rectransform.md) — m_AnchoredPosition이 실제 위치 결정, m_LocalPosition만으로는 씬 저장 안됨
- [BillboardLabel Cinemachine Camera](feedback_billboard-label-cinemachine-camera.md) — Camera.main null 가능, CameraService.Instance.MainCamera 사용 필수
- [멀티 MonoBehaviour 한 파일](feedback_multi-monobehaviour-per-file-broken-script-ref.md) — 런타임 복제용 보조 컴포넌트는 별도 .cs 파일로, 같은 파일이면 m_Script fileID:0 깨짐
- [screenshot_game은 카메라만](feedback_screenshot-game-camera-only.md) — ScreenSpaceOverlay 패널은 안 찍힘, 구조 검증으로 대체
