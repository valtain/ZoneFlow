---
name: billboard-label-cinemachine-camera
description: BillboardLabel은 CameraService.Instance.MainCamera를 사용해야 하며 Camera.main은 Cinemachine 환경에서 null일 수 있다
metadata:
  type: feedback
---

Cinemachine 환경에서 `Camera.main`이 null을 반환할 수 있다. Cinemachine 카메라는 "MainCamera" 태그가 없을 수 있기 때문.

**Why:** ZoneFlow의 카메라는 Cinemachine Brain이 제어하며, 카메라 오브젝트의 태그가 "Untagged"로 설정되어 있다. CameraService가 이를 추상화하므로 BillboardLabel은 반드시 `CameraService.IsReady` + `CameraService.Instance.MainCamera`를 사용해야 한다.

**How to apply:** BillboardLabel.LateUpdate에서 `CameraService.IsReady`를 체크하고 `CameraService.Instance.MainCamera`로 카메라를 참조한다. `Camera.main`은 절대 사용하지 않는다. Play Mode 시각 검증 시 Cinemachine Brain을 임시 비활성화하거나 카메라에 MainCamera 태그를 임시 부여해야 game capture가 가능하다.

관련: [[world-space-canvas-rectransform]]
