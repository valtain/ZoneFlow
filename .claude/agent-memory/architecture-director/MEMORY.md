# Memory Index

- [CatalogBaker 직렬화 병목](project_catalog-baker-serialization.md) — BakeAll 단일 진입점, 부분 베이크 불가, 멀티 에이전트 동시 베이크 금지
- [AQ-6 카탈로그 베이크 스케일](zoneflow-aq6-catalog-bake-scale.md) — 콘텐츠 풍부화 시 전량 재스캔이 병목인가 (AQ-4 인접, 제안한 새 AQ)
- [P5 피벗 매핑](zoneflow-persona5-pivot.md) — 이중루프 수직슬라이스; 시간/파티/세이브=Service, 전투=BattleMode+BattleService, Save=ISaveable 순회
- [Mode 스택 push/pop 계약](zoneflow-mode-stack-push-pop-contract.md) — stack push=Zone스왑, pop=재활성+위치복원 이미 지지, 결과 전달 채널만 결손
- [AQ-9 Polyglot provider 로딩](zoneflow-aq9-polyglot-provider-loading.md) — AddressablesFontProvider: Localization AssetTable(최대활용) vs raw Addressables(1-facade); 지금 조정 불필요, seam이 격리
- [원격 에셋 floor 불변식](zoneflow-remote-asset-floor-invariant.md) — Remote Addressables 부재 시 로컬 티어가 기능적 바닥으로 저하하는 규약 부재 (AQ-11서 발견, AQ-4 재발질문)
