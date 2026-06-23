# 탐색 로그

- [2026-06-23 | start] 탐색 시작. 주제: 존 입장 시 카메라 연출 개선.

- [2026-06-23 | brainstorm] 현재 존 입장 연출 전수 조사. 구축됨: FadeScreen 암전(0.3s+0.3s), HUD 배너 슬라이드인.
  미구축: 전용 타이틀 카드, 전환 효과 다양화, 카메라 연출, 오디오. 사용자 확인으로 초점을 **카메라 연출**로 좁힘.

- [2026-06-23 | explore] 카메라 추적 배선 추적. vcam은 Player.prefab Camera 자식(TrackingTarget=Model,
  OrbitalFollow PositionDamping=1), Brain DefaultBlend=EaseInOut 2s. 코드상 Follow 수동 할당 없음(프리팹 직렬화).
  근본 원인 확정: 텔레포트를 워프로 인지 못 해 댐핑·블렌드 슬라이드 발생.

- [2026-06-23 | decision] 사용자 확정: "Player 위치 변경에 따라 자동 적용된 카메라 연출이 부적절 → 간단한 카메라 연출 추가",
  연출 모양 = **클린 컷 + 가벼운 리빌**.

- [2026-06-23 | decision] Candidate A(워프 컷 + FOV 리빌) 채택. B(데이터 튜닝)·C(컷만) 폐기.
  Cinemachine 3.1.6 API 확인: `OnTargetObjectWarped`는 `target == Follow`일 때만 동작 → Follow 타깃 실제 delta 전달.

- [2026-06-23 | explore] 구현 후 플레이 모드 검증. 텔레포트 시 브레인 카메라가 델타(X 0→60, Z 68→128)만큼
  한 스텝에 점프(클린 컷 확인). 리빌은 timeScale=0 동결 측정으로 FOV 64.8(=60×1.08) 펀치 확인.
