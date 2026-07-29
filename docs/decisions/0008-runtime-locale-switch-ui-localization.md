# ADR-0008: locale 런타임 변경 + 인게임 UI 지역화

- **상태**: 채택
- **날짜**: 2026-07-29
- **관련**: [ADR-0006](0006-polyglot-font-loading-localization-asset-table.md)(폰트 로딩), [ADR-0007](0007-polyglot-async-font-load-reentrancy.md)(async 재진입 해소), 이슈 #107

## 맥락

폴리글롯 초기 설계는 **"locale 부팅 시 1회 결정, 런타임 swap 없음"**([features/polyglot/spec.md](../../features/polyglot/spec.md))을 전제로 TMP Settings를 부팅 시 1회만 세팅했다. 또한 UI 규칙은 **"인게임 UI 텍스트는 영문"**([.claude/rules/ui-design.md](../../.claude/rules/ui-design.md)) — 근거는 한글/CJK 폰트 글리프 누락 회피였다.

사용자가 **메뉴에서 언어를 자유롭게 바꾸며 UI(라벨 + 폰트)가 실시간으로 변하는 것**을 요구했다. 두 원칙 모두 이 요구와 충돌한다. 그러나 전환 메커니즘은 이미 존재한다: `FontService.SelectLocaleAsync` = `SelectedLocale` 갱신(→ `LocalizeStringEvent` 자동 반영) + FontService 재부팅(→ 폰트·StyleSheet 재적용). 언어 피커가 이미 이걸 쓰며, [ADR-0007](0007-polyglot-async-font-load-reentrancy.md)의 async 전환으로 **반복 전환의 재진입 위험이 제거**되어 안전해졌다.

## 결정

두 원칙을 개정한다.

1. **locale은 런타임에 변경 가능하다.** 변경은 `FontService.SelectLocaleAsync`로만 수행한다 — "swap 없음"이 아니라 **"변경 시 전체 재부팅으로 재적용"**. per-frame 컴포넌트 폰트 직렬화·swap 금지 불변식은 유지된다(컴포넌트는 폰트 미직렬화, 재부팅이 TMP Settings를 일괄 재적용).
2. **인게임 UI 텍스트는 지역화한다**(하드코딩 영문 폐기). `LocalizeStringEvent` + String Table 경유로 바인딩한다.

이유: 전환 메커니즘·안전성(ADR-0007)이 이미 갖춰졌고, "영문 강제"의 유일한 근거였던 글리프 누락 문제가 폴리글롯 per-locale 폰트로 사라졌다 — 두 원칙을 유지할 근거가 소멸했다.

## 고려한 대안

| 대안 | 장점 | 단점 / 탈락 이유 |
| --- | --- | --- |
| A (채택) 런타임 변경 + UI 지역화 | 요구 충족, 실제 다국어 UI 성립 | 원칙 2개 개정, UI 지역화 저작 필요 |
| B 데모에서만 시연·프로덕션 영문 유지 | 원칙 무변경 | 실제 제품에선 언어가 안 바뀜 = 요구 미충족 |

## 결과

- **강제**: locale 변경은 `SelectLocaleAsync` 경유(게임 코드가 `SelectedLocale`·폰트를 직접 조작 금지 — Localization 격리 유지, LocaleSwitcher는 `FontService.CurrentLocaleCode`로 조회). UI 텍스트는 `LocalizeStringEvent` + String Table로 지역화. 언어 선택 UI(메뉴 `TMP_Dropdown`)가 `SelectLocaleAsync`를 호출.
- **불변식 유지**: 컴포넌트 폰트 미직렬화(재부팅 일괄 재적용) — 오염 방지 근간은 그대로.
- **연타 가드**: `SelectLocaleAsync` 중복 재부팅 방지 플래그(#107).
- **개정 문서**: `features/polyglot/spec.md`("swap 없음" 전제 주석), `Assets/PolyglotAssets/Runtime/FontEngine.cs` doc, `.claude/rules/ui-design.md`("인게임 UI 영문" → 지역화).
- **점진 적용(후속)**: 이 ADR은 원칙을 개정하고 메뉴를 지역화한다. HUD·다이얼로그 등 나머지 인게임 UI의 전면 지역화는 별도 후속이다.
