# /feature

로컬 설계 폴더 관리 커맨드. GitHub Issue의 설계 컨텍스트(spec, decisions, testcases)를 담는 폴더를 관리한다.

## 사용법

```text
/feature new <name> [--from <exploration-name>]   로컬 설계 폴더 생성
/feature list                                      로컬 feature 목록 출력
/feature show <name>                               spec.md + decisions.md 요약 출력
/feature plan <name>                               spec.md 분석 → tasks.md task 목록 생성
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
/feature new core_foundation --from milestone1-impl
```

**생성 파일 템플릿**:

`spec.md` — 목표 / 주요 컴포넌트 / 데이터 흐름 섹션 (`# <name> — 설계 스펙` 헤더)
`decisions.md` — `# <name> — 설계 결정` + 결정 테이블. `--from` 지정 시 테이블 위에 `Source:` 링크 삽입.
`testcases.md` — `# <name> — 검증 시나리오` + 빈 체크리스트

**동작**:

1. `features/<name>/` 폴더 생성
2. 위 3개 파일을 템플릿으로 생성 (`--from` 여부에 따라 decisions.md에 Source 링크 포함)
3. `BACKLOG.md` Features 테이블에 `| <name> | features/<name>/ |` 행을 추가한다.
4. `--from <exploration-name>` 이 지정된 경우: `BACKLOG.md` Explorations 테이블에서 해당 exploration 행의 `Promoted To` 열을 `<name>`으로 업데이트한다.

---

### list — feature 목록 출력

`BACKLOG.md` Features 테이블을 읽어 출력한다.

---

### show — feature 상세 출력

`features/<name>/spec.md`와 `decisions.md`를 읽는다.

- `decisions.md`에 `Source:` 링크가 있으면 해당 `findings.md`도 읽어 설계 근거를 포함한 요약을 출력한다.
- `Source:` 링크가 없으면 spec.md + decisions.md만 요약 출력한다.

---

### plan — task 목록 생성

```text
/feature plan service_locator
```

**동작**:

1. `features/<name>/spec.md` + `features/<name>/decisions.md` 읽기
2. `decisions.md`에 `Source:` 링크가 있으면 해당 `findings.md`도 읽어 설계 근거를 파악한다
3. CLAUDE.md Architectural Principles 참조
4. 아래 기준으로 task 목록 생성:
   - 독립적으로 구현·커밋 가능한 단위로 분해
   - 의존 순서가 있으면 번호 순서에 반영
   - 타입 정의 → 핵심 로직 → 씬 연결 → 테스트 순으로 그룹화 권장
5. `features/<name>/tasks.md`의 빈 테이블을 채워 저장:

```markdown
| # | 태스크 | 상태 |
| --- | --- | --- |
| 1 | <task title> | todo |
| 2 | <task title> | todo |
```

- 결과 출력: 생성된 task 목록
- 안내 출력: `이제 /next <name> 으로 이슈 일괄 생성을 시작하세요`

**상태 컬럼 규칙**:

- `todo` — 아직 GitHub Issue 미생성
- `#N` — `/issue new` 실행 후 GitHub Issue 번호로 교체

---

## 주의사항

- feature 이름은 snake_case 사용 (예: `core_foundation`, `zone_system`)
- GitHub Issue와 연결은 `/next <name>` 또는 `/issue new` 커맨드로 별도 수행
- decisions.md는 `/issue do` 또는 `/issue review` 시 참조된다
- `Source:` 링크가 있는 경우 decisions.md에는 exploration이 다루지 않은 feature 고유 구현 선택만 추가한다
