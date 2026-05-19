# ADR-002: SceneService 설계 — SceneType 기반 오케스트레이션

**Status**: Accepted
**Date**: 2026-05-19

## Context

씬을 additive로 로드·언로드할 때, 씬 종류별로 다른 처리(자동 로드, 전제조건 로드 등)가 필요하다.

## Decision

- SceneType을 `Standalone / Shell / Zone`으로 분류 (Gameplay 대신 Zone 사용)
- SO(ScriptableObject)의 씬 이름은 `so.name`(에셋 파일명)으로 결정 — 별도 SerializeField 불필요
- CoreServices·GamePlayServices 씬 참조는 Inspector SerializeField로 직렬화
- SceneService를 경유하지 않은 직접 `SceneManager.LoadSceneAsync` 호출 금지
- 비동기 API는 UniTask 전용 (Coroutine 혼용 금지)

## Consequences

- 씬 로드 진입점이 SceneService 하나로 통일되어 흐름 추적이 쉬움
- SO 에셋 파일명이 씬 빌드 이름과 일치해야 함

## Alternatives Considered

- 씬 이름을 별도 SerializeField로 관리: 에셋 이름과 불일치 가능성 있음
