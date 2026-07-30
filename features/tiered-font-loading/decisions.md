# tiered-font-loading — 설계 결정

Source: [tiered-font-loading](../../explorations/tiered-font-loading/findings.md)

## 고유 구현 결정 (탐색이 다루지 않은 feature 선택만)

| 결정 | 이유 |
| --- | --- |
| **단계화 — Tier-1 먼저, Tier-2 후속 phase** | Tier-1(Static 서브셋·Local)은 **원격 호스팅 무의존 + 즉시 ~22M 절감 + 리스크 낮음**. Tier-2(Remote·C/E·floor 불변식)는 호스팅·재베이크 결정이 얽혀 분리하는 것이 안전 |
| **첫 태스크 = before/after 측정** | 탐색 중 spend 한도로 보류된 서브셋 베이크 정확값을 구현 초입에 확보해 방향·목표치 확정 |
| feature 이름 하이픈 유지(`tiered-font-loading`) | 탐색명 및 기존 하이픈 feature(`exploration-hud`·`mode-transitionfx` 등)와 일관 |

(탐색 findings의 채택 방향·기구 1a·per-locale Tier-1 구조·소유권 분할·D/A/B 폐기 근거는 중복 기재하지 않음 — Source 참조.)
