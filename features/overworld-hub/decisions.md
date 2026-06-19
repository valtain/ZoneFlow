# overworld-hub — 설계 결정

Source: [demo-connected-world](../../explorations/demo-connected-world/findings.md)

> findings의 C2 방향을 따른다. 아래는 exploration이 결론내지 않은 **feature 고유 결정** —
> 2026-06-19 확정.

| 결정 | 선택 | 이유 |
| --- | --- | --- |
| 허브 왕복 방식 | **Replace + 명시 복귀 포털로 허브 reload** (Stack/Pop 아님) | findings 원안. 진입 시 허브 씬 언로드(RefCount=0)로 메모리 해제 명확, 씬 단위 load/unload로 명제 A 직접 시연 |
| 새 게임 진입점 (NewGameUri) | **overworld로 변경** (현 `exploration/village`) | 허브가 연결 축 → 시작점도 허브가 자연스럽고 갈림길 선택을 즉시 노출 |
| village↔dungeon 직결 포털(C3) | **제거 → 허브 경유 일원화** | 모든 이동을 허브 경유로 묶어 허브 중심성·명제 A 반복 시연 확보. 직결 우회 경로 차단 |
| 복귀 스폰 | overworld에 **원점별 복귀 스폰포인트** (`overworld_from_{zone}`), 복귀 포털이 `?id=`로 지정 | Replace+reload는 위치 자동복원이 없으므로, 떠난 게이트 근처로 복귀시켜 동선 자연스럽게 |
| BossRoom 전투 메커닉 | ➡️ 분리 | 본 feature는 무대(씬·Zone·포털·스폰)까지. 전투 방식은 별도 탐색(`/explore`) |
