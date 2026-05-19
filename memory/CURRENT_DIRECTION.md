# 현재 개발 방향

## 현재 Milestone

**Milestone 1 — Core Foundation** (설계 완료, 구현 예정)

ServiceLocator, SceneService, Bootstrap 설계 확정.

## 다음 단계

Milestone 1 기능 구현:

1. ServiceLocator (`MonoService<T>`) 구현
2. SceneService (SceneType 기반 로드·언로드) 구현
3. Bootstrap (ColdStartup, DevBootstrap) 구현

## 현재 상태 (2026-05-19)

- workspace 구조 전환 완료 (Unity 서브폴더 분리 + docs/ + memory/ 구성)
- features/service_locator, scene_service, bootstrap 설계 스펙 확정
