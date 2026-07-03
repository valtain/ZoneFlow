---
name: portal-prefab-ring-layout
description: Portal_Gateway 링 캡슐 배치 규칙 — XY평면 (cos, sin, 0) 배치가 수직 링
metadata:
  type: feedback
---

링 세그먼트 캡슐을 (cos*semiA, sin*semiB, 0) 로 배치하면 XY평면 링이 되어 수직으로 선다.
(cos*semiA, 0, sin*semiB)로 배치하면 XZ평면 링이 되어 바닥에 눕는다.
Frame 부모 오브젝트를 회전시켜 수정하려다 실수하기 쉬우므로, 세그먼트 localPosition 자체를 올바르게 지정하는 것이 더 명확하다.

**Why:** 처음에 잘못 배치해서 링이 눕혀진 채로 프리팹이 생성됐고, Frame 90도 회전 방향을 잘못 잡아 한 번 더 수정했음.
**How to apply:** 새 링 형태 프리미티브 오브젝트를 배치할 때 반드시 어느 평면에 놓이는지 먼저 확인하고 localPosition을 선택한다.
