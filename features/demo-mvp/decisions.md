# demo-mvp — 설계 결정

Source: [portfolio-demo](../../explorations/portfolio-demo/findings.md)

## 고유 구현 결정

| 결정 | 이유 |
| --- | --- |
| 기존 World1/World2 씬을 새로 만들지 않고 재활용 + ZoneId 매핑으로 Village/Dungeon 표현 | 씬 신규 생성 비용 제거, 빠른 MVP 검증. 아키텍처는 ZoneAssetCatalog 매핑으로 충분히 증명 |
| 명제 A 증명 방식 = Portal 왕복 시 Mode 스택 불변 관찰 | 코드 수정 없이 플레이 관찰만으로 직관적 증명. GamePlayDirector의 Zone 로드/언로드 로직이 Mode 스택을 건드리지 않음을 시각적 확인 |
| Portal NavigationUri를 명시적 문자열로 설정 (e.g., `gameplay://exploration/dungeon`) | 기존 Portal.cs 규약 준수, IInteractable 패턴 일관성. URI 파싱 경로를 통해 라우팅 메커니즘 동작 검증 |

## 포트폴리오-데모에서 상속받은 결정

(portfolio-demo findings.md에서 이미 정의된 범위/아키텍처 결정은 중복 제외)
- MVP 우선 접근: 명제 A만 증명하고 Story/Battle/Boss는 후속 feature로 분리
- 기존 Portal 패턴(NavigationUri + OnInteractAsync) 준용
- 기존 ExplorationMode·ZoneRegistry 재사용
