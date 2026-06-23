# zone-entry-camera — 탐색 질문

> 존 입장(텔레포트)으로 플레이어 위치가 바뀔 때 자동 적용되는 카메라 거동이 부적절하다.
> 어떻게 간단한 카메라 진입 연출로 대체할 것인가?

## 컨텍스트

- `CinemachineCamera`(vcam)는 `Assets/ZoneFlowAssets/Runtime/Prefabs/Player.prefab`의 Camera 자식,
  TrackingTarget=Model, OrbitalFollow PositionDamping=1.
- CinemachineBrain(CoreServices) DefaultBlend=EaseInOut **2초**.
- 존 진입 흐름: `GamePlayMode.PlayedAsync` → `SpawnPlayer()` → `PlayerService.SpawnAt()`
  → `PlayerController.Teleport()`(재진입) 또는 `Instantiate`(최초). Director가 `FadeScreen`(0.3s+0.3s)으로 감쌈.
- 문제: Cinemachine이 텔레포트를 "워프"로 인지하지 못해 카메라가 새 위치로 댐핑·블렌드로 서서히 미끄러진다.
  페이드(0.6s)가 끝난 뒤에도 슬라이드가 노출됨.

## 탐색 범위

- 텔레포트 시 카메라 컷/워프 처리 방식
- 진입 시 의도된 가벼운 카메라 연출(리빌) 추가
- 카메라 책임 소유 위치(PlayerController vs CameraService vs Director)

Out of scope: 카메라 외 연출(타이틀 카드/전환 효과 다양화/오디오), 일반 플레이 중 카메라 추적 튜닝.

## 성공 기준

- 텔레포트 직후 카메라가 슬라이드 없이 새 위치에 프레이밍(클린 컷).
- 진입 시 짧고 가벼운 리빌(FOV 줌 정착) 연출.
- 일반 플레이 중 추적 동작은 변경하지 않음(텔레포트와 일반 이동을 구분).
