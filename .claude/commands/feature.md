# /feature

로컬 설계 폴더 관리 커맨드. GitHub Issue의 설계 컨텍스트(spec, decisions, testcases)를 담는 폴더를 관리한다.

## 사용법

```text
/feature new <name>      로컬 설계 폴더 생성
/feature list            로컬 feature 목록 출력
/feature show <name>     spec.md + decisions.md 요약 출력
/feature plan <name>     spec.md 분석 → tasks.md task 목록 생성
```

## 폴더 구조

```text
features/<name>/
  spec.md          설계 스펙 (목표, 컴포넌트, 데이터 흐름)
  decisions.md     주요 설계 결정과 이유
  testcases.md     검증 시나리오 체크리스트
```

---

## 서브커맨드

### new — 새 feature 폴더 생성

```text
/feature new core_foundation
```

**동작**:

1. `features/<name>/` 폴더 생성
2. 아래 3개 파일을 빈 템플릿으로 생성

**spec.md 템플릿**:

```markdown
# <name> — 설계 스펙

## 목표

## 주요 컴포넌트

## 데이터 흐름
```

**decisions.md 템플릿**:

```markdown
# <name> — 설계 결정

| 결정 | 선택 | 이유 |
| --- | --- | --- |
```

**testcases.md 템플릿**:

```markdown
# <name> — 검증 시나리오

- [ ] 시나리오 1
```

3. `BACKLOG.md` Features 테이블에 행 추가:

```text
| <name> | features/<name>/ |
```

---

### list — feature 목록 출력

`BACKLOG.md` Features 테이블을 읽어 출력한다.

---

### show — feature 상세 출력

`features/<name>/spec.md`와 `decisions.md`를 읽어 요약 출력한다.

---

### plan — task 목록 생성

```text
/feature plan service_locator
```

**동작**:

1. `features/<name>/spec.md` + `features/<name>/decisions.md` 읽기
2. CLAUDE.md Architectural Principles 참조
3. 아래 기준으로 task 목록 생성:
   - 독립적으로 구현·커밋 가능한 단위로 분해
   - 의존 순서가 있으면 번호 순서에 반영
   - 타입 정의 → 핵심 로직 → 씬 연결 → 테스트 순으로 그룹화 권장
4. `features/<name>/tasks.md`의 빈 테이블을 채워 저장:

```markdown
| # | 태스크 | 상태 |
| --- | --- | --- |
| 1 | <task title> | todo |
| 2 | <task title> | todo |
```

- 결과 출력: 생성된 task 목록
- 안내 출력: `이제 /issue new "<title>" --feature <name>으로 GitHub Issue를 등록하세요`

**상태 컬럼 규칙**:

- `todo` — 아직 GitHub Issue 미생성
- `#N` — `/issue new` 실행 후 GitHub Issue 번호로 교체

---

## 주의사항

- feature 이름은 snake_case 사용 (예: `core_foundation`, `zone_system`)
- GitHub Issue와 연결은 `/issue new` 커맨드로 별도 수행
- decisions.md는 `/issue do` 또는 `/issue review` 시 참조된다
