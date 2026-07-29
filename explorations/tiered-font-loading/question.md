# Tiered-Font-Loading — 탐색 질문

> WebGL 빌드에서 CJK 폰트 용량(소스 TTF ~38MB)이 **전량 초기 다운로드**에 실리는 문제를 어떻게 줄일 것인가? 그리고 **고정 문자열(Intro/부팅 UI)** 과 **가변 문자열(콘텐츠)** 의 폰트 로딩 전략을 어떻게 계층 분리할 것인가?

관련 AQ: **AQ-11** (BACKLOG.md / docs/project-goals.md).

## 컨텍스트 (실측)

- **아틀라스 모드**: `m_AtlasPopulationMode: 1` = Dynamic + `m_ClearDynamicDataOnBuild: 1` → 빌드 시 아틀라스 비고, **런타임에 소스 TTF로 글리프 렌더** → 소스 TTF가 빌드에 포함돼야 함.
- **소스 TTF 크기**: NotoSans(Latin) 2.0M / NotoSansJP 9.2M / NotoSansKR 10M / NotoSansSC 17M = **~38M**.
- **Addressables**: `Localization-Assets-*` 그룹 = `Local.BuildPath`/`Local.LoadPath` (Remote 아님) → 전 locale 폰트가 초기 `.data`에 포함.
- **WebGL 특성**: 로컬 디스크 지연 로딩 없음 → 어떤 언어를 쓰든 ~38MB가 초기 다운로드에 실림.
- **로딩 seam**: `AddressablesFontProvider`(Localization Asset Table, async — [ADR-0007](../../docs/decisions/0007-polyglot-async-font-load-reentrancy.md)) → `FontService.BootAsync`. 재부팅·재적용 경로 존재(`SelectLocaleAsync`, `FontRuntime.Applied`).
- **콘텐츠 세션 경계**: `MenuPanel.OnNewGame` → `SceneService.EnsureContentServicesLoaded`(ContentServices 씬) — "콘텐츠 시작"의 명시적 seam.

## 탐색 범위

- 아틀라스 모드: Dynamic(TTF 필요) vs **Static 서브셋**(TTF 불요, 고정 글리프만).
- Addressables **Local vs Remote per-locale**(WebGL 온디맨드 다운로드).
- 부팅 UI vs 콘텐츠의 폰트 로딩 **계층 분리**(2단).
- 서브셋 글리프 소스: String Table(Intro/Menu) + 피커 라벨(4개 네이티브명)에서 추출·베이크(빌드/CI 스텝).
- 전환 seam: boot→content 폰트 **업그레이드 트리거**(ContentServices 경계?).

Out of scope: 실제 대사/콘텐츠 텍스트 지역화 저작, 구체 CDN 선정, Han-unification 정책 확정(설계 노트로만).

## 성공 기준

- 초기 다운로드에서 CJK 폰트 용량을 줄이는 후보들이 트레이드오프와 함께 정리됨.
- 부팅 UI(고정)·콘텐츠(가변) 폰트 계층 분리 설계 방향이 잡힘.
- Static 서브셋 베이크의 **유지보수 비용**(재베이크 트리거·CI)이 평가됨.
- Remote per-locale의 **호스팅/네트워크 비용**이 평가됨.
- 후속 `/feature` 승격 가능한 최소 설계(빌드 `.data` 크기 diff **측정 프로토타입** 포함).
