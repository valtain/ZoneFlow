---
name: zoneflow-aq9-polyglot-provider-loading
description: Polyglot 폰트 로딩 = Localization AssetTable(X). AQ-9 해소·ADR-0006 채택 — provider도 지정 seam이라 "접점 격리" 유지
metadata:
  type: project
---

**결론(2026-07-24, ADR-0006 채택):** X(Localization AssetTable)로 확정·구현됨. `AddressablesFontProvider`가 `LocalizationSettings.AssetDatabase.GetLocalizedAsset<FontRef>("Fonts","font",locale)`로 로드. 부팅 provider를 DirectRef→Addressables로 통째 교체(FontService·FontPreviewHook), FontEngine/facade/FontSet 무변경(교체비용 0 확인). ADR-0005 문구는 "facade 1곳"→"facade+provider 두 지정 seam"으로 리프레임. `DirectRefFontProvider`(#105 테스트)·`FontCatalog`(피커 fallback)는 존치 — 이중화 통일은 후속. 아래는 결정 배경(보존).

---

Polyglot 폰트 엔진의 로딩 전략 결정이 미해결로 남았다 — AQ-9 제안(AQ-4 "Addressable 전환 시 Zone 생명주기" 인접, 별건).

**Why:** 사용자가 "자체 FontCatalog가 Localization AssetTable을 손으로 재구현한 것 아니냐"고 제기. 핵심은 미래 `AddressablesFontProvider`가 (X) Localization `AssetTable`/`LocalizedAsset<FontRef>`를 쓸지 — "TMP/Localization 최대 활용" 원칙 부합, refcount·preload·per-locale 로딩 공짜 — vs (Y) raw Addressables + AssetReference 카탈로그 — "Localization 접점 facade 1곳" 원칙(ADR-0005) 유지. 두 사용자-지정 원칙이 여기서 충돌한다.

**판단(현재):** 지금은 조정 불필요. `FontCatalog`는 `DirectRefFontProvider` 한 곳에서만 참조되고 seam(`IFontProvider`: localeCode→FontSet)을 넘지 않으므로, 미래 마이그레이션은 provider 통째 교체지 FontCatalog 개조가 아니다. `FontRef`(응집 SO)는 그대로 addressable 단위로 forward-compatible(활성 locale FontRef만 로드하면 eager-load 해소). #97 되돌림 불필요. 지금 AssetReference/AssetTable로 당기는 건 CLAUDE.md "추측성 추상화·1회용 유연성 금지" 위반.

**How to apply:** Addressables task 착수(AQ-4와 함께) 시 X/Y 결정 후 ADR로 승격. 나는 X(AssetTable)로 기운다 — "최대활용"이 명시 원칙이고 refcount 재구현이 곧 회피 대상인 "중복 재구현"이며, "1 facade"의 실질 의도(호출부가 TMP/Localization 미접촉)는 provider도 Polyglot 내부 seam이라 유지되기 때문. 채택 시 ADR-0005 문구를 "facade 1곳"→"facade+provider 두 지정 seam"으로 미세 리프레임 필요. 남은 task 가드레일: FontEngine(#100)·facade(#99)는 `IFontProvider`/`FontSet`만 의존, `FontCatalog`/`FontRef` 직접 참조 금지 — seam 청결이 교체비용을 0으로 유지. 관련 [[zoneflow-persona5-pivot]].
