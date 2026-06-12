# /next
<!-- Complexity Hint: Medium → Sonnet 4.6 -->

현재 상태를 읽고 다음 단계를 자동으로 결정하는 통합 진입점. explore와 feature 양쪽 상태를 넘나들며 멈추지 않고 흘러간다.

## 사용법

```text
/next                     전체 진행 현황 + 추천 다음 액션 출력
/next <name>              exploration 또는 feature → 상태 감지 후 다음 단계 자동 실행
/next <#>                 issue 번호 → 상태 확인 후 구현 시작
```

인자 타입 자동 감지:

- 숫자 → issue 번호로 처리
- `explorations/<name>/` 존재 → exploration 상태 기계
- `features/<name>/` 존재 → feature 상태 기계
- 양쪽 모두 존재 → feature 상태 기계 우선 (exploration은 이미 closed)
- 인자 없음 → 전체 현황 모드

---

## `/next <exploration_name>` — exploration 상태 기계

`BACKLOG.md` Explorations 테이블에서 해당 exploration의 Status를 확인한다.

### 상태: active

**출력 형식**:

```text
탐색 중: <name>
핵심 질문: <question.md 핵심 질문 한 줄>
후보: N개 (active M개, eliminated K개)
마지막 논의: <discussion.md 마지막 항목 날짜>
```

**동작**:

1. `explorations/<name>/question.md`, `candidates.md` 전체 읽기
2. `discussion.md` 마지막 10개 항목 읽기
3. 위 형식으로 현재 상태 출력
4. **AskUserQuestion**으로 확인:
   - 질문: `<name> 탐색을 어떻게 진행할까요?`
   - 선택지: `계속 탐색`, `탐색 완료 (close)`
   - `계속 탐색` → exploration 대화 재개 (`/explore <name>` 동작)
   - `탐색 완료` → `/explore close <name>` 동작 실행

### 상태: closed + feature 미생성

BACKLOG.md Explorations 테이블의 `Promoted To` 열이 비어 있으면:

```text
탐색 완료: <name>
findings.md에 결론이 기록되어 있습니다.
아직 이 탐색에서 Feature가 생성되지 않았습니다.

다음 단계:
  /feature new <candidate-label> --from <name>
```

위 형식으로 출력하고 종료한다.

### 상태: closed + feature 생성됨

`Promoted To` 열에 feature 이름이 있으면 해당 feature 이름으로 **feature 상태 기계**를 바로 실행한다.

---

## `/next <feature_name>` — feature 상태 기계

### Step 1: tasks.md 확인

`features/<name>/tasks.md` 를 읽는다.

- 파일이 없거나 태스크가 없으면 → `features/<name>/spec.md` + `decisions.md`를 읽는다. decisions.md에 `Source:` 링크가 있으면 해당 findings.md도 읽는다. task 목록을 생성하고 `tasks.md`에 저장한다. → Step 2로 계속.

### Step 2: todo 태스크 → GitHub Issue 일괄 생성

tasks.md에서 상태가 `todo`인 행을 확인한다.

todo 항목이 있으면:

1. 생성할 태스크 목록을 출력한다.
2. **AskUserQuestion** 도구로 확인:
   - 질문: `<name>의 미등록 태스크 N개를 GitHub Issue로 생성할까요?`
   - 선택지: `생성`, `취소`
3. `생성` 선택 시 각 태스크마다 순서대로:
   - `.claude/github-project.json`에서 milestone 파악을 위해 `feature_parents[<name>]`의 milestone을 조회:
     `gh issue view <parent-#> --json milestone --jq .milestone.title`
   - `gh issue create --title "<task-title>" --body "Feature: <name>\n\n<spec.md 목표 섹션>\n\n관련 설계 문서: features/<name>/" --milestone "<milestone>"` 실행
   - 생성된 이슈를 Project에 추가: `gh project item-add <project-num> --owner valtain --url <issue-url>`
   - Sub-issue 연결:
     `CHILD_ID=$(gh api repos/valtain/ZoneFlow/issues/<#> --jq .id)`
     `PARENT=$(cat .claude/github-project.json | jq ".feature_parents[\"<name>\"]")`
     `gh api repos/valtain/ZoneFlow/issues/$PARENT/sub_issues --method POST --field sub_issue_id=$CHILD_ID`
   - 생성된 `#N`으로 tasks.md 해당 행 상태 즉시 업데이트
   - 진행 상황 출력: `[1/N] #42 생성: <title>`
4. 완료 요약 출력.

→ Step 3으로 **멈추지 않고** 계속.

### Step 3: 다음 이슈 추천 및 확인

`gh issue list --state open --json number,title` 로 열린 이슈 목록을 수집한다.
tasks.md 번호 순서(의존 관계 반영)로 첫 번째 오픈 이슈 = 추천 이슈.

진행 현황을 출력한다:

```text
Feature: <name> 진행 현황
────────────────────────────
완료: N/M 태스크

남은 이슈:
  → #42 <title>  ← 추천
     #43 <title>
     #44 <title>
```

**AskUserQuestion** 도구로 확인:

- 질문: `#<추천번호> <제목>를 바로 시작할까요?`
- 선택지: `시작`, `다른 이슈 선택`, `나중에`
- `시작` → **TodoWrite**로 구현 단계 체크리스트 생성 후 `issue do` 동작으로 구현 시작 (컨텍스트 수집 → Haiku sub-agent 위임)
- `다른 이슈 선택` → 번호를 입력받아 해당 이슈로 구현 시작
- `나중에` → 현황만 출력하고 종료

### Step 4: 모든 이슈 완료

`Feature <name> 완료! 모든 태스크가 닫혔습니다.` 를 출력하고 종료.

---

## `/next <#>` — issue 상태 기계

1. `gh issue view <#> --json state,title` 로 상태 확인
2. `closed` → "이미 완료된 이슈입니다 (#N 닫힘)" 출력 후 종료
3. `open` → `issue.md`의 `do` 동작과 동일하게 실행:
   - `gh issue view <#> --json title,body,milestone,labels` 로 컨텍스트 수집
   - body에서 `Feature: <name>` 파싱 → `features/<name>/spec.md`, `decisions.md` 읽기. decisions.md에 `Source:` 링크가 있으면 해당 findings.md도 읽기.
   - Agent tool (`model='haiku'`)에 구현 위임

---

## `/next` — 전체 현황 모드

1. `BACKLOG.md`에서 active exploration 목록 수집
2. `features/` 하위 디렉토리 목록 수집
3. 각 feature의 tasks.md를 읽어 완료율 계산 (`#N` 상태 = 이슈 생성됨, `todo` = 미생성)
4. `gh issue list --state open --json number,title` 로 열린 이슈 확인
5. 현황 출력:

```text
전체 현황
─────────────────────────────────────────
탐색 중:
  → story-mode-stack  (active)

Features:
service_locator  ████████░░  4/5 완료
zone_system      ░░░░░░░░░░  0/6 (이슈 미생성)
bootstrap        ██████████  6/6 완료

추천: /next story-mode-stack  (탐색 재개)
```

---

## 주의사항

- `issue do` 구현 로직은 `issue.md`에 정의된 것을 그대로 따른다.
- tasks.md에 `#N` 상태인 이슈가 GitHub에서 `closed`면 완료로 집계한다.
- exploration과 feature 이름이 동일한 경우 feature 상태 기계가 우선한다.
