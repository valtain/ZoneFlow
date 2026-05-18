# Complexity Guide

작업 복잡도를 평가해 적합한 모델을 선택하기 위한 기준이다.

## Complexity Hint란?

커맨드나 작업 유형에 정의된 **Complexity Hint**는 일반적인 복잡도 기준점이다.
실제 작업 범위나 맥락에 따라 Claude가 등급을 조정할 수 있다.

---

## 복잡도 등급

### Low → Haiku 4.5

- 파일 읽기, 검색, 내용 확인
- 단순 텍스트/포맷 편집
- 커밋 메시지 작성, 세션 요약
- 정해진 규칙을 그대로 적용하는 작업

### Medium → Sonnet 4.6

- 새 기능 구현 (단일 시스템 범위)
- 버그 수정 및 원인 분석
- 리팩터링, 명명 규칙 정비
- 코드 리뷰, PR 검토

### High → Opus 4.7

- 아키텍처 설계 및 시스템 간 연동
- 복잡한 Plan 모드 전체 흐름
- 여러 패키지에 걸친 구조 변경
- 새로운 패턴/컨벤션 도입 결정

---

## 커맨드별 Complexity Hint

| 커맨드 | Hint | 비고 |
| --- | --- | --- |
| `/bridge` | Low | 맥락 압축, 파일 읽기 중심 |
| `/feature` | Low | 설계 폴더 생성·목록·요약 출력 |
| `/git-commit` | Low | diff 읽기 + 메시지 작성 |
| `/issue new / list / show / close` | Low | gh 명령 실행 + 파일 읽기 중심 |
| `/work-log` | Low | diff 해석 + 설계 의도 기반 보고서 작성 |
| `/init` | Medium | 코드베이스 전체 탐색 포함 |
| `/issue do` | Medium | spec/decisions 읽기 + Haiku 에이전트 구현 위임 |
| `/review` | Medium | 변경사항 분석 + 의견 제시 |
| `/simplify` | Medium | 변경 코드 품질·효율성 검토 및 수정 |
| `/explore` | High | 아키텍처 탐색, 다중 candidate 비교, 트레이드오프 추론 |
| `/issue review` | High | decisions/coding-style 읽기 + Opus 에이전트 리뷰 위임 |
| `/security-review` | High | 취약점 분석, 판단 요구 높음 |
