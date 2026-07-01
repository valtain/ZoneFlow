# Complexity Guide

작업 복잡도를 평가해 적합한 모델을 선택하기 위한 기준이다.

## Complexity Hint란?

커맨드나 작업 유형에 정의된 **Complexity Hint**는 일반적인 복잡도 기준점이다.
실제 작업 범위나 맥락에 따라 Claude가 등급을 조정할 수 있다.

> 모델은 **별칭**(`haiku`/`sonnet`/`opus`/`fable`)으로 지정한다. 별칭은 항상 현재 최신 모델로
> 해석되므로(예: `sonnet`→최신 Sonnet), 문서·hook에 버전 번호를 고정하지 않는다.
> `Agent` 도구의 `model` 파라미터도 이 별칭만 받는다(특정 버전 pin 불가).

---

## 복잡도 등급

### Low → haiku

- 파일 읽기, 검색, 내용 확인
- 단순 텍스트/포맷 편집
- 커밋 메시지 작성, 세션 요약
- 정해진 규칙을 그대로 적용하는 작업

### Medium → sonnet

- 새 기능 구현 (단일 시스템 범위)
- 버그 수정 및 원인 분석
- 리팩터링, 명명 규칙 정비
- 코드 리뷰, PR 검토

### High → opus

- 아키텍처 설계 및 시스템 간 연동
- 복잡한 Plan 모드 전체 흐름
- 여러 패키지에 걸친 구조 변경
- 새로운 패턴/컨벤션 도입 결정

---

## 커맨드별 Complexity Hint

| 커맨드 | Hint | 비고 |
| --- | --- | --- |
| `/bridge` | Low | 맥락 압축, 파일 읽기 중심 |
| `/feature new / list / show` | Low | 설계 폴더 생성·목록·요약 출력 |
| `/git-commit` | Low | diff 읽기 + 메시지 작성 |
| `/issue new / list / show / close` | Low | gh 명령 실행 + 파일 읽기 중심 |
| `/quick` | Low | 소규모 설정·문서 변경, 이슈 추적 없음 |
| `/work-log` | Low | diff 해석 + 설계 의도 기반 보고서 작성 |
| `/feature plan` | Medium | spec/decisions 분석 → task 목록 생성 |
| `/init` | Medium | 코드베이스 전체 탐색 포함 |
| `/issue do` | Medium | spec/decisions 읽기 + `unity-specialist` 에이전트 구현 위임 |
| `/next` | Medium | 상태 자동 감지 → feature plan·이슈 생성·구현 흐름 오케스트레이션 |
| `/review` | Medium | 변경사항 분석 + 의견 제시 |
| `/simplify` | Medium | 변경 코드 품질·효율성 검토 및 수정 |
| `/battle new / tune / review` | Medium | 턴제 전투 설계·구현, `combat-specialist` 위임 (`list`는 Low) |
| `/systems new / improve / review` | Medium | 시뮬 서비스·데이터 모델 설계·구현, `systems-designer` 위임 (`list`는 Low) |
| `/explore` | High | 아키텍처 탐색, 다중 candidate 비교, 트레이드오프 추론 |
| `/issue review` | High | decisions/coding-style 읽기 + Opus 에이전트 리뷰 위임 |
| `/security-review` | High | 취약점 분석, 판단 요구 높음 |

---

## 티어 배정 재검토 결과

- **현 매핑은 타당하며 일괄 상향은 불필요.** `sonnet`은 단일 시스템 구현·도메인 콘텐츠 저작에 충분하고,
  아키텍처/시스템 간 연동은 이미 High(`opus`)로 분리되어 있다. High 티어는 `opus` 유지(Fable 5는
  Opus 초과 프리미엄이라 일반 설계 검토엔 과함).
- **모델 중복 지정 주의.** hook은 delegating 커맨드(`issue do`, `level/ui/battle/systems *`)에
  `model=`과 `agent=`를 함께 넘기는데, 해당 에이전트 정의(`.claude/agents/*.md`)도 `model:`을 선언한다.
  `Agent` 도구의 model 파라미터는 에이전트 frontmatter를 **override**하므로, 특정 커맨드를 재-티어링하려면
  **hook과 에이전트 `.md` 양쪽**을 함께 바꿔야 한다(한쪽만 바꾸면 override 값이 이긴다).
