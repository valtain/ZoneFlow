# ADR-003: Bootstrap 설계 — ColdStartup 재로드 및 DevBootstrap 분리

**Status**: Accepted
**Date**: 2026-05-19

## Context

에디터 Play 진입 시 정상 Play 흐름과 동일한 초기화 순서를 보장해야 하고,
개발용 진입점(GamePlayBootstrap)이 프로덕션 씬을 오염시키지 않아야 한다.

## Decision

- ColdStartup 씬 재로드 방식: `unload → reload` 유지 (씬 내 모든 오브젝트 재초기화)
- GamePlayBootstrap은 `DevBootstrap.unity` 전용 — auto-managed 씬 배치 금지
- `[DefaultExecutionOrder(-2000)]` — 씬 내 모든 컴포넌트보다 먼저 실행
- DevBootstrap은 빌드에 포함하지 않음 (개발 전용 진입점)
- SceneType은 Inspector SerializeField로 지정 (씬마다 다르기 때문)

## Consequences

- 에디터와 빌드의 초기화 흐름이 동일하게 유지됨
- DevBootstrap 없이는 개발 중 빠른 Play 진입 불가 — DevBootstrap 씬 항상 유지 필요

## Alternatives Considered

- `reloadScene: false` 방식: 초기화 순서가 정상 Play와 달라져 버그 재현 어려움
