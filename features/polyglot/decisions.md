# polyglot — 설계 결정

Source: [tmp-multilingual-font-engine](../../explorations/tmp-multilingual-font-engine/findings.md)

## 고유 구현 결정

| 결정 | 이유 |
| --- | --- |
| 폰트 엔진을 별개 패키지 `Polyglot`(`Assets/PolyglotAssets/`, asmdef)로 분리 | "별개 패키지로 진행" 요청. 폰트 엔진은 게임 로직에 비의존한 인프라 → 어셈블리 경계로 재사용성·컴파일 격리 확보. 프로젝트 규약(project-structure.md)의 `{PackageName}Assets/` 형제 배치 답습 |
| 순수 엔진(MonoBehaviour 서비스 무의존) + 게임 측 얇은 `FontService : MonoService<FontService>` 어댑터 | asmdef 어셈블리는 `Assembly-CSharp`(→ `MonoService`)를 **역참조 불가**. 부팅 진입점만 게임 측 어댑터가 소유하면 이 제약을 만족하면서 탐색의 "MonoService는 얇은 래퍼" 방침과 일치 |
| 네임스페이스 `Polyglot`(게임 `ZoneFlow`와 독립) | 재사용 가능한 독립 패키지로서 자기완결적 네임스페이스 유지. 향후 `Packages/`로 추출해도 무변경 |
| TMP·Unity Localization 내장 기능 최대 활용 + API 접점을 패키지 내부 facade 1곳에 격리 | 사용자 지정 원칙. 자체 로직 최소화(중복 재구현 회피), 두 패키지 버전 변경 시 수정 범위를 어댑터 1지점으로 한정(유연 대응) |
| locale 영속화는 Localization 내장 selected-locale(StartupLocaleSelector) 사용 | 위 "최대 활용"의 귀결. 탐색이 미뤄둔 커스텀 PlayerPrefs 스토어를 대체 → 코드·상태 축소. **단 최종 확인은 systems-designer**(세이브 계층과의 관계) |
| 가드레일: Candidate C(컴포넌트별 Localization Property Variants 폰트 배선) 재도입 금지 | "Localization 최대 활용"이 폰트 대량 직렬화로 번지지 않도록 명시. 컴포넌트 폰트 미직렬화 불변식은 오염 방지의 근간 |
| `package.json`은 문서·향후 추출용(Assets/ 하위라 Package Manager 비관리) | 실제 어셈블리 경계는 `Polyglot.asmdef`가 만든다. `Packages/`로 이전 시 그대로 UPM 패키지가 되도록 레이아웃만 미리 정렬 |
| TMP Settings의 **디스크** 기본 스타일시트 = Polyglot locale 시트(`PolyglotStyles_en`), 전 locale 시트는 동일 style 집합 유지 (#118) | `TMP_Text.textStyle` getter는 메시 재생성 때마다 해시를 재해석하고, 실패하면 `m_TextStyleHashCode`를 Normal로 **되쓴다**. 이 필드는 driven이 아니라 저장돼야 하는 저작 정보이므로 되쓰기가 곧 디스크 유실이다. 해석 경로가 컴포넌트 `m_StyleSheet`(저장 시 스트립됨) → `TMP_Settings.defaultStyleSheet` 뿐이라, **부팅 전 에디트 모드**에서도 해석이 성립해야 Style-only 저작이 성립한다. `StyleSheetConfigTests`가 가드 |
| TMP Settings는 `SetDirty`+`SaveAssets`로 저장하지 않는다 (#118) | driven 등록은 씬/프리팹 직렬화만 막고 **ScriptableObject 강제 저장은 막지 못한다** — 부팅이 적용해둔 locale 폰트·fallback이 그대로 애셋에 기록돼 오염된다. TMP Settings 변경이 필요하면 YAML을 직접 수정하고 `ImportAsset`으로 리로드한다 |

## 탐색에서 상속받은 결정

(findings.md에서 이미 정의된 방향·폐기 근거는 중복 제외)

- **Candidate A 채택** — 부팅 1회 TMP Settings 세팅(locale별 기본 폰트 + 전역 fallback + 스타일시트) + Style-only 저작 + `IFontProvider` seam. 런타임 swap 없음(locale 부팅 1회 결정).
- **Candidate B 폐기** — 단일 거대 fallback 체인. TMP 첫 매칭 정지로 한자 지역 자형 혼입, 스타일·언어 지정 불가.
- **Candidate C 폐기(프리뷰만 채택)** — Localization Property Variants를 폰트 배선 주 메커니즘으로. 폰트 대량 직렬화가 "기본 폰트 외 serialize 금지" 불변식과 충돌. 단 Localization은 에디터 프리뷰 도구로는 채택.
- **오염 방지 2단** — 저작 제어(폰트 필드 잠금·Style-only·저장 스트립) + `OnWillSaveAssets` `ClearFontAssetData()` / Clear Dynamic Data on Build.
- **로딩 seam** — `DirectRefFontProvider` 지금, `AddressablesFontProvider` 나중, 호출부 무변경.
