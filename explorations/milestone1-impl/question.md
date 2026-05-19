# Milestone1-Impl — 탐색 질문

> Milestone 1의 세 핵심 시스템(ServiceLocator·SceneService·ColdStartup+DevBootstrap)을 어떤 구조와 순서로 설계할 것인가?

## 컨텍스트

ZoneFlow는 현재 Runtime C# 코드가 전혀 없는 초기 상태다.
레퍼런스(`C:\nexus_frame\main`)는 `MonoService<T>.Instance` 패턴(정적 per-type accessor)을 사용하지만,
ZoneFlow는 신규 설계이므로 패턴 선택부터 결정이 필요하다.

CLAUDE.md 아키텍처 원칙 중 핵심 제약:
- 서비스 생성 책임 → **씬 배치** (코드가 생성하지 않음)
- MonoService 자동 GameObject 생성 → **사용 안 함**
- 레지스트리 접근 → **Inspector 직렬화 우선**
- 비동기 → **UniTask 전용**
- ColdStartup 씬 재로드 → **unload → reload 유지**
- 개발용 Bootstrap → **DevBootstrap.unity 전용**
- CoreServices·GamePlayServices → **SceneService 경유 원칙**

## 탐색 범위

- **ServiceLocator 패턴**: 정적 per-type accessor vs. 컨테이너형 vs. Inspector 직렬화 우선
- **SceneService 설계**: SceneType 분류, Prerequisites 로드 오케스트레이션, 씬 계층 구조
- **ColdStartup + DevBootstrap**: 에디터 어느 씬에서 Play해도 동작하는 부트스트랩 흐름
- **구현 순서**: 세 시스템 간 의존 관계 및 단계적 구축 순서

Out of scope: Zone/Mode 시스템, UI 시스템, 게임플레이 로직

## 성공 기준

- ServiceLocator API가 결정되어 `features/` 설계 스펙으로 승격 가능한 상태
- SceneService의 씬 로드 흐름이 시퀀스 다이어그램 수준으로 정의됨
- ColdStartup → SceneService → CoreServices 로드 순서가 명확히 정의됨
- 세 시스템의 구현 순서와 각 시스템의 public API 초안이 확정됨
