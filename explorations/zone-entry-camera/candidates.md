# 후보 방향들

## Candidate A: 텔레포트 워프 컷 + 가벼운 FOV 리빌

**상태**: promoted

`PlayerController.Teleport()`에서 Cinemachine에 워프를 통지하여 댐핑 슬라이드 없이 컷하고,
그 위에 짧은 FOV 줌 정착을 얹는다.

- 클린 컷: 이동 전 Follow 타깃 월드 위치를 캡처 → 이동 → `_vcam.OnTargetObjectWarped(follow, delta)`.
  (`OnTargetObjectWarped`는 `target == Follow`일 때만 동작 → 정확한 Follow 타깃 delta 필요.)
- 리빌: `_vcam.Lens.FieldOfView`를 rest×1.08 → rest로 PrimeTween ~0.7s, Ease.OutCubic. 단일 float만 조작 → 리그 비의존.
- 카메라 책임을 PlayerController에 응집(vcam이 Player 프리팹 자식이므로 소유자로 자연스러움). Director/Mode/CameraService 무변경.

**장점**: Cinemachine 표준 워프 API 사용, 변경 범위 최소, 일반 추적 동작 보존.
**단점**: 카메라 진입 연출 책임이 PlayerController에 추가됨.

---

## Candidate B: 댐핑/블렌드 설정만 데이터 튜닝

**상태**: eliminated — 이유: DefaultBlend을 Cut으로, OrbitalFollow Damping을 낮추면 텔레포트뿐 아니라
**일반 플레이 중 카메라 추적까지 딱딱해진다**. 텔레포트와 일반 이동을 구분할 수 없어 부적합.

---

## Candidate C: 클린 컷만 (리빌 없음)

**상태**: eliminated — 이유: 사용자가 "간단한 카메라 연출 추가"를 원함. 컷만으로는 진입감 부재.
Candidate A에 리빌 포함하여 흡수.
