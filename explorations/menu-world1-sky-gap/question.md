# menu-world1-sky-gap — 탐색 질문

> Menu→World1 전환 시 하늘(빈 공간)이 잠깐 보이는 원인은 무엇이며, 어떻게 수정해야 하는가?

## 컨텍스트

Splash→Intro→Menu 흐름에서 사용자가 New Game을 누르면 `gameplay://exploration/world1?switch=replaceall`로 전환된다.
이때 전환 도중 하늘(빈 공간)이 약 0.3초간 노출되는 현상이 발생한다.

## 탐색 범위

- FadeScreen 타이밍과 카메라 렌더링 순서
- ReplaceAllAsync 전환 시퀀스
- PanelMode vs ZoneMode 전환 차이
- 향후 TransitionFX 확장 방향 (효과 지정, 우선순위)
- Mode-Zone 수명 분리 필요성

Out of scope: 개별 씬의 카메라 배치, 씬 아트 디렉션

## 성공 기준

- 하늘 노출의 정확한 원인 코드 지점 특정
- 즉시 수정 방향 확정 (이슈화)
- 향후 TransitionFX 확장 및 Mode-Zone 분리에 대한 설계 방향 이슈화
