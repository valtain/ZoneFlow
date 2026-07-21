# tmp-multilingual-font-engine — 탐색 질문

> TextMeshPro로 한/영/일/중(CJK) 다국어 글리프를 올바로 렌더하고, 언어·스타일별 폰트를 지정하며, 에디터에서 locale별 프리뷰가 가능하되 폰트 자산 오염이 없는 **폰트 엔진**을 어떻게 설계하는가?

## 컨텍스트

프로젝트에 존재하는 TMP 폰트는 스톡 `LiberationSans SDF`(라틴 전용) 하나뿐이고 CJK fallback 체인이 없어, 한글/일문/중문을 넣으면 tofu가 난다. 이 때문에 두 rule이 **런타임 텍스트를 영문으로 강제**하고 있다(`.claude/rules/ui-design.md`, `.claude/rules/level-content.md`). 이 탐색의 대상 엔진은 그 회피책을 걷어내는 것이 목적이다.

- TMP는 Unity 6 uGUI 2.0 내장본(`com.unity.ugui: 2.0.0`, `Unity.TextMeshPro::TMPro.*`).
- `com.unity.localization`·`com.unity.addressables` 모두 미설치.
- 18개 프리팹이 폰트 guid(LiberationSans)를 하드코딩. 런타임에 `.font`를 바꾸는 코드는 없음.
- 서비스는 `MonoService<T>`를 CoreServices 씬에 배치(ADR-0001). 비동기는 UniTask 전용.

## 탐색 범위

- **In scope**: 폰트 렌더링 엔진 — locale별 기본 폰트 + (스타일 × 언어) 폰트 지정, 부팅 시 1회 locale 결정(**Intro 씬 피커 1회 선택·불변·영구 저장**), 전역 fallback 커버리지, Addressables 확장 seam, 에디터 locale 프리뷰, 동적 SDF 폰트 오염 방지·저작 제어.
- **Out of scope**: 런타임 문자열 소싱/문자열 테이블/locale 런타임 전환(추후 Unity Localization으로 별도). 실제 CJK 폰트 파일 저작(미확보 — Noto Sans CJK 전제로 seam만).

## 성공 기준

- 세 후보 중 하나를 근거 있게 채택/기각.
- 채택 시: 후속 feature가 구현할 컴포넌트·seam·저작 규칙·검증 기준 도출.
- CLAUDE.md/rule 반영 필요 항목 식별.
