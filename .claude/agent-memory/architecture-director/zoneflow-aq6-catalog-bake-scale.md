---
name: zoneflow-aq6-catalog-bake-scale
description: 제안한 AQ-6 — 콘텐츠 풍부화 시 CatalogBaker 전량 재스캔이 저작 루프 병목/충돌점이 되는가 (AQ-4 인접)
metadata:
  type: project
---

level-designer 에이전트 도입 검토(2026-06-29) 중 제기한 새 아키텍처 질문.

**AQ-6 (제안):** 단일 씬 multi-zone이 늘고 zone당 interactable·spawn·narrative가 풍부해질 때, CatalogBaker의 전량 재스캔(`BakeAll`)이 콘텐츠 저작 루프의 병목/충돌점이 되는가? build settings에 enabled된 모든 Zone 씬을 additive로 열어 재스캔하므로 콘텐츠가 늘수록 느려지고, 두 디자이너 동시 베이크가 카탈로그를 오염시킬 수 있다.

**Why:** 기존 AQ-1~5는 Zone-Mode/Story/Save-Load 런타임 경계 질문이다. AQ-6은 *콘텐츠 저작 도구*가 스케일에서 견디는지의 질문으로, level-designer 도입이 처음 노출시켰다. AQ-4(Addressable 전환)와 인접 — 베이크 방식이 콘텐츠 스케일에서 견디는지가 Addressable 논의의 선행 질문.

**How to apply:** BACKLOG.md Architectural Questions 테이블 등록 여부 아직 미확정(검토 시점엔 미등록). 향후 콘텐츠 저작이 본격화되면 탐색 대상으로 추적. [[catalog-baker-serialization]] 참조.
