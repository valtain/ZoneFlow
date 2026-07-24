# ADR-0005: 첫 asmdef 패키지 경계 도입 — Polyglot 순수 엔진 + 게임 어댑터

- **상태**: 제안
- **날짜**: 2026-07-21
- **관련 AQ**: — (project-structure.md의 "독립 패키지" 규약 구체화)

## 맥락

다국어 TMP 폰트 엔진([features/polyglot](../../features/polyglot/spec.md), Source: [tmp-multilingual-font-engine](../../explorations/tmp-multilingual-font-engine/findings.md))을 **별개 Unity 패키지**로 구현하기로 했다. 그런데 현재 프로젝트에는 **프로젝트 소유 asmdef가 하나도 없다** — 모든 런타임 코드가 `Assembly-CSharp` 단일 어셈블리에 있고, `MonoService`/`CoreServices` 등 서비스 인프라도 여기 있다.

폰트 엔진을 독립 asmdef 어셈블리(`Polyglot`)로 분리하면 어셈블리 참조 규칙이 걸린다: **asmdef 어셈블리는 `Assembly-CSharp`을 역참조할 수 없다.** 따라서 `Polyglot`이 `FontService : MonoService<FontService>` 형태로 서비스 인프라에 직접 의존할 수 없다. 이 첫 경계 도입을 어떻게 설계할지 결정해야 한다.

## 결정

**Polyglot을 순수 엔진 패키지로 만들고(서비스 인프라 무의존), 게임 측(ZoneFlowAssets, Assembly-CSharp)이 얇은 `FontService : MonoService<FontService>` 어댑터로 부팅 시 1회 호출한다** — asmdef→Assembly-CSharp 역참조 제약을 만족하는 유일한 깔끔한 방향이며, 탐색의 "MonoService는 얇은 래퍼" 방침과 일치하기 때문.

- `Polyglot`(`Assets/PolyglotAssets/`)은 부팅 엔진 API·`IFontProvider` seam·TMP/Localization facade·`FontRef`/`FontCatalog` SO만 소유. MonoBehaviour 서비스 무의존.
- 게임 측 어댑터가 부팅 진입점을 소유하고 Polyglot API를 호출. `Assembly-CSharp`은 모든 asmdef를 자동 참조하므로 성립.
- 배치는 project-structure.md의 `{PackageName}Assets/` 형제 규약 준수.

## 고려한 대안

| 대안 | 장점 | 단점 / 탈락 이유 |
| --- | --- | --- |
| A (채택) 순수 엔진 패키지 + 게임 측 MonoService 어댑터 | 역참조 제약 충족, 엔진 재사용·컴파일 격리, 어댑터가 얇음 | 부팅 진입점이 패키지 밖에 있어 배선 1겹 추가 |
| B 서비스 인프라(`MonoService`/`CoreServices`)를 먼저 별도 패키지로 추출 후 Polyglot이 참조 | 엔진이 서비스 패턴을 직접 사용 가능 | 범위 폭증(전 서비스 계층 재구조화), 이번 요청 밖. 향후 별도 결정 |
| C asmdef 없이 `Assets/PolyglotAssets/`만 두고 Assembly-CSharp에 흡수 | 제약 자체가 없음 | "별개 패키지" 요건 미충족 — 어셈블리 경계가 없어 격리·재사용 이득 없음 |

## 결과

- **강제**: `Polyglot` 어셈블리는 게임 코드(`Assembly-CSharp`)를 참조하지 않는다(순수 엔진 불변식). 게임↔엔진 연결은 게임 측 어댑터가 담당한다.
- **강제**: TMP·Localization API 접점은 Polyglot 내부 **지정 seam에 격리**(버전 변경 유연 대응) — features/polyglot 원칙. 접점은 **facade**(TMP Settings 적용·locale 조회) + **provider**(폰트 로딩) 둘이며(→ [ADR-0006](0006-polyglot-font-loading-localization-asset-table.md)로 리프레임), 불변식의 본질은 **게임 코드·호출부가 TMP/Localization 타입을 직접 만지지 않는 것**이다.
- **선례**: 이후 독립 인프라 패키지는 동일 패턴(순수 코어 + 게임 어댑터, `{Name}Assets/` + asmdef)을 따른다. constraints.md가 본 ADR을 참조.
- **새 미해결 질문(AQ 후보)**: 서비스 인프라(`MonoService`/`CoreServices`)를 공용 패키지로 추출할 것인가(대안 B) — 독립 패키지가 늘면 재검토.
