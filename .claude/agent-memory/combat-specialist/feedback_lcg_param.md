---
name: feedback_lcg_param
description: BattleRng LCG 파라미터 선택 — a=1664525, c=1013904223, 모듈러스=2^32(uint 오버플로우)
metadata:
  type: feedback
---

BattleRng LCG 파라미터: a=1664525, c=1013904223, 모듈러스=2^32(uint 자연 오버플로우).

**Why:** Numerical Recipes in C가 제시한 값으로, 짧은 수열에서 분산이 고르다. `_state >> 1`로 부호 비트를 제거한 뒤 range로 모듈러스.

**How to apply:** 파라미터 변경 시 트랜스크립트 재현 테스트가 즉시 깨지므로, 변경하려면 테스트 기댓값도 함께 업데이트해야 한다.
