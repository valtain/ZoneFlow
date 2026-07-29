---
name: zoneflow-remote-asset-floor-invariant
description: Remote Addressables 도입 시 반복될 아키텍처 질문 — 원격 에셋 부재 시 Zone/세션이 기능적 바닥(floor)으로 저하하는 규약
metadata:
  type: project
---

원격 Addressables 에셋(폰트 content 티어, 향후 원격 콘텐츠)이 오프라인/CORS/실패로 부재할 때, Zone/세션이 **깨진 상태가 아니라 저하 모드로 진입**하는 규약이 ZoneFlow에 아직 없다.

**Why:** tiered-font-loading 탐색(AQ-11)에서 발견 — content 티어(Remote) 실패 시 boot 서브셋엔 콘텐츠 글리프가 없어 tofu. 현 폰트 로드는 `Debug.Assert(fontRef != null)`로 실패 경로를 모델링하지 않음. 이는 폰트만의 문제가 아니라 AQ-4("Addressable 전환 시 Zone 생명주기 인터페이스 변화")가 원격을 실제 도입할 때마다 재발할 **횡단 질문**이다.

**How to apply:** Remote Addressables를 새로 얹는 설계를 검토할 때, "로컬 티어(항상 존재)가 원격 부재 시 기능적 바닥이 되는가 + 실패를 어떻게 표면화하는가"를 필수 체크항목으로 제기하라. 폰트에선 boot 서브셋=floor. 이 규약이 서면화되면 ADR 후보. [[zoneflow-aq9-polyglot-provider-loading]](폰트 로딩 seam)·AQ-4와 인접.
