# demo-mvp — 검증 시나리오

## 기본 흐름

- [ ] Village 씬에서 게임 시작 시 ExplorationMode가 Active 상태, 플레이어 조종 가능
- [ ] Portal_ToDungeon 진입 → Dungeon Zone 로드 + Village Zone 언로드 (로그 또는 씬 전환 관찰)
- [ ] Zone 전환 전후 ExplorationMode 스택이 Push/Pop/Replace 없이 그대로 유지 (**명제 A**)
- [ ] 전환 후 플레이어가 SP_Entrance에 배치됨 (Dungeon 입구 위치)
- [ ] 역방향: Portal_ToVillage 진입 → Village 복귀, ExplorationMode 유지 (**명제 A 확인**)

## 추가 검증

- [ ] NavigationUri 파싱 성공 여부 확인 (로그: `Navigation: gameplay://exploration/dungeon`)
- [ ] Zone 언로드 시 Village 오브젝트 정리됨 (Hierarchy 또는 메모리 관찰)
- [ ] 양방향 이동 반복 시 ExplorationMode가 계속 유지 (스택 손상 X)
