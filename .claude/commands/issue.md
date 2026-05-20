# /issue

GitHub Issue 생명주기 커맨드. Task 추적의 단일 소스는 GitHub Issues다.

## 사용법

```text
/issue new <title> --feature <name> [--milestone <label>] [--label <label>]
/issue new <title> --bug [--label <label>]
/issue list [--milestone <name>] [--state open|closed]
/issue show <#>
/issue do <#>       구현 시작
/issue review <#>   코드 리뷰
/issue close <#>    커밋 + GitHub Issue 닫기
```

---

## 서브커맨드

### new — GitHub Issue 생성

**Feature 태스크**:

```text
/issue new "ServiceLocator 구현" --feature core_foundation --milestone "M1 · Core Foundation"
```

1. `features/<name>/spec.md`를 읽어 issue body 초안 작성:

   ```text
   Feature: <name>

   <spec.md 목표 섹션>

   관련 설계 문서: features/<name>/
   ```

2. `gh issue create --title <title> --body <body> --milestone <milestone> --label <label>` 실행
3. 생성된 issue 번호(`#N`)를 출력

**버그 리포트**:

```text
/issue new "SceneService 씬 언로드 누락" --bug
```

1. 재현 조건·기대 동작·실제 동작을 사용자에게 확인한다.
2. `gh issue create --title <title> --body <body> --label bug` 실행
3. 생성된 issue 번호(`#N`)를 출력

---

### list — Issue 목록 조회

```text
/issue list
/issue list --milestone "M1 · Core Foundation"
/issue list --state closed
```

**동작**: `gh issue list --repo valtain/ZoneFlow` + 필터 적용 후 출력

---

### show — Issue 상세 조회

```text
/issue show 3
```

**동작**: `gh issue view <#>` 출력

---

### do — 구현 (구 /work-on)

```text
/issue do 3
```

**동작**:

1. `gh issue view <#> --json title,body,milestone,labels` 로 컨텍스트 수집
2. body에서 `Feature: <name>` 파싱 → `features/<name>/spec.md`, `decisions.md` 읽기
3. 아래 프롬프트로 **Agent tool (`model='haiku'`)** 에 위임:

   ```text
   당신은 ZoneFlow 프로젝트의 개발 에이전트입니다.
   아래 컨텍스트를 바탕으로 Issue를 구현하세요.

   [Issue]
   #<number>: <title>
   <body>

   [설계 스펙]
   <spec.md 전체>

   [아키텍처 결정 — 구현 전 체크리스트로 참조]
   <decisions.md 전체>

   구현 규칙:
   - CLAUDE.md의 Architectural Principles와 Coding Style 준수
   - decisions.md의 결정을 뒤집을 필요가 생기면 먼저 보고하고 중단
   - testcases.md가 있으면 구현 후 체크리스트 검토

   완료 후 반환:
   1. 구현 완료 여부 (success / blocked)
   2. 핵심 변경사항 1-2줄 요약
   3. blocked인 경우: 중단 이유와 필요한 결정 사항
   ```

4. **success** → `/issue close <#>` 자동 호출
5. **blocked** → 중단 이유를 사용자에게 보고 후 STOP

---

### review — 코드 리뷰 (구 /review-issue)

```text
/issue review 3
```

**동작**:

1. `gh issue view <#> --json title,body` 로 Issue 컨텍스트 수집
2. body에서 `Feature: <name>` 파싱 → `features/<name>/decisions.md` 읽기
3. `.claude/docs/style/coding-style.md` 읽기
4. `git diff HEAD~1` 실행 결과 수집
5. 아래 프롬프트로 **Agent tool (`model='opus'`)** 에 위임:

   ```text
   당신은 ZoneFlow 프로젝트의 코드 리뷰어입니다.
   구현자의 작업 과정을 모르는 상태에서 결과물만 검토합니다.

   [Issue]
   #<number>: <title>
   <body>

   [설계 결정 배경]
   <decisions.md 전체>

   [코딩 스타일 기준]
   <coding-style.md 핵심 규칙>

   [검토할 코드]
   <git diff>

   검토 기준:
   1. Issue 요구사항이 모두 구현되었는가?
   2. CLAUDE.md Architectural Principles를 준수하는가?
   3. 코딩 스타일 기준을 준수하는가? (XML 문서, SerializeField 패턴, UniTask 등)
   4. decisions.md의 설계 결정과 충돌하는 구현이 있는가?

   출력 형식:
   - 통과 여부: PASS / FAIL
   - 발견된 문제 목록 (있을 경우)
   - 권장 수정 사항 (있을 경우)

   주의: 스타일 경고는 FAIL 사유가 아님. 아키텍처 원칙 위반과 미구현 요구사항만 FAIL 처리.
   ```

6. **PASS** → `/issue close <#>` 자동 호출
7. **FAIL** → 새 issue 생성 (`gh issue create`) 후 재작업 안내:
   ```text
   gh issue create --title "재작업: <원본 title>" \
     --body "Review FAIL.\n\n문제점:\n<목록>\n\n원본 Issue: #<number>" \
     --milestone <원본 milestone> --label "rework"
   ```

---

### close — 커밋 + Issue 닫기

```text
/issue close 3
```

**동작**:

1. `git status --short` 출력
2. stage할 파일 번호를 선택받음 (예: `1 2` / `all` / `skip`)
3. `Assets/` 하위 파일 stage 시 대응 `.meta` 자동 포함. 단 아래 제외:
   - `.`으로 시작하는 파일/폴더, `~`로 끝나는 파일
   - `ProjectSettings/`, `Packages/manifest.json`, `Packages/packages-lock.json`
4. 확정 목록을 사용자에게 보여줌
5. `git add` 실행
6. staged diff 분석 → 커밋 메시지 초안 작성:
   - 형식: `[prefix] 한국어 메시지 (Closes #<number>)`
   - prefix: feat / fix / refactor / style / test / docs / chore
   - 명사형 종결, 마침표 금지
   - **`Closes #<number>`는 반드시 포함** (GitHub Issue 자동 닫힘)
7. 사용자 확인 후 `git commit -m "<message>"` 실행
8. `gh issue close <#>` 실행
9. 결과 출력: 커밋 해시 + 닫힌 issue 번호

---

## 주의사항

- `gh` CLI가 인증되어 있어야 함 (`gh auth status`)
- issue body의 `Feature: <name>` 줄은 `/issue new`가 자동 삽입하는 파싱 키
- `close`는 `do` / `review` 에서 자동 호출되지만 수동으로도 실행 가능
