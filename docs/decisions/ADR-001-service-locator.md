# ADR-001: ServiceLocator 패턴 — MonoService<T>.Instance

**Status**: Accepted
**Date**: 2026-05-19

## Context

씬 배치 원칙(서비스 생성 책임은 씬이 가짐)을 유지하면서 서비스를 전역 접근 가능하게 해야 한다.

## Decision

`MonoService<T>.Instance` 패턴을 사용한다.

- 씬에 GameObject로 배치된 컴포넌트가 스스로를 ServiceLocator에 등록
- 자동 GameObject 생성(AddComponent 등) 사용 안 함
- 중복 인스턴스 감지는 `Debug.Assert`로 처리 (`throw` 금지)
- `[DefaultExecutionOrder(-1000)]` — Bootstrap(-2000)보다 늦게, 일반 컴포넌트보다 먼저

## Consequences

- 씬 구조가 서비스 생명주기를 직접 제어
- 씬 없이 서비스 접근 불가 — 테스트 시 씬 설정 필요

## Alternatives Considered

- DI 컨테이너(Zenject 등): 씬 구조와 독립적이지만 씬 배치 원칙과 충돌
- Resources.Load 기반 자동 생성: CLAUDE.md 원칙 위반
