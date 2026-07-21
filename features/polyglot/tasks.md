# polyglot — Tasks

Source: [tmp-multilingual-font-engine](../../explorations/tmp-multilingual-font-engine/findings.md) · 설계: [spec.md](spec.md) · [decisions.md](decisions.md)

> **선행 완료(커밋됨)**: 패키지 스켈레톤 — `Assets/PolyglotAssets/`(package.json + asmdef 4종) + `com.unity.localization` 1.5.12 의존(커밋 `b8d477e`). 아래는 엔진 구현 task.

| # | 태스크 | 상태 |
| --- | --- | --- |
| 1 | seam 계약 정의(순수): `IFontProvider`(locale→폰트 세트 로드) + TMP/Localization을 감싸는 facade 인터페이스(TMP Settings 적용·활성 locale 조회). 구현 없이 계약만 | #96 closed |
| 2 | 데이터 SO: `FontRef`(폰트+메타) · `FontCatalog`(locale→FontRef 매핑) 스켈레톤 + `CreateAssetMenu`. 식별자는 `so.name` | #97 closed |
| 3 | `DirectRefFontProvider` — `IFontProvider` 직접참조 구현. `FontCatalog`에서 활성 locale 폰트 세트 로드(Addressables는 후속 seam) | #98 closed |
| 4 | TMP/Localization facade 구현 — `TMP_Settings` 기본 폰트·전역 fallback·활성 스타일시트 적용 + Localization selected-locale 조회를 **패키지 내부 1곳에 격리** (`TMPro.*`/`UnityEngine.Localization.*` 직접 호출 여기만) | #99 closed |
| 5 | 부팅 엔진 API `FontEngine` — 활성 locale → `IFontProvider` 로드 → facade로 TMP Settings 적용(부팅 1회, 런타임 swap 없음) | #100 closed |
| 6 | 게임 측 어댑터 `FontService : MonoService<FontService>`(ZoneFlowAssets) + CoreServices 씬 배치 + 부팅 훅 1회 호출 배선(UniTask) | #101 |
| 7 | 저작 제어(Editor): TMP 컴포넌트 Font Asset 필드 잠금·Style-only Inspector + 저장 가드(폰트 자동 스트립·검증), 기존 프리팹 1회 diff | #102 |
| 8 | 오염 방지 가드(Editor): `AssetModificationProcessor.OnWillSaveAssets` `ClearFontAssetData()` + 폰트별 Clear Dynamic Data on Build | #103 |
| 9 | 에디터 프리뷰: Unity Localization `LocalizedTmpFont` + Game View locale 스위처 도입(프리뷰 전용, 컴포넌트 폰트 미직렬화 유지) | #104 |
| 10 | EditMode 테스트: provider(locale→폰트) · facade 적용 · 엔진 부팅 결정론 · 오염 불변식(폰트 미직렬화) 검증 | #105 |

## 의존 순서

타입/계약(1·2) → 핵심 로직(3·4·5) → 씬 배선(6) → 저작·오염·프리뷰 Editor(7·8·9) → 테스트(10).
7·8·9는 6 이후 병렬 가능. 4는 패키지 변경 격리의 핵심 — 이후 TMP/Localization 버전 업 대응 지점.

## 가드레일 (모든 task 공통)

- 컴포넌트/프리팹에 폰트 asset **직렬화 금지**(locale 기본 폰트 상속 + Style만). Candidate C 재도입 금지.
- `Polyglot` 어셈블리는 `Assembly-CSharp` **역참조 금지**(순수 엔진). 게임 연결은 task 6 어댑터만.
- 서비스 생성은 씬 책임 · 비동기 UniTask · 단언 `Debug.Assert`(constraints.md).
- **facade(4)·FontEngine(5)은 seam(`IFontProvider`/`FontSet`)만 의존** — `FontCatalog`/`FontRef`를 직접 참조하지 않는다. 미래 provider 교체(Direct→Addressables)를 국소화하기 위함(AQ-9). 카탈로그↔provider 결선은 game 어댑터(6)가 소유.
