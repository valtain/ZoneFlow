# /explore

Architecture exploration 커맨드. repo context를 가진 채로 구조 문제를 탐색하고,
여러 candidate를 비교하며, 결론을 정식 issue로 승격한다.

외부 LLM(ChatGPT 등)과의 copy-paste 병행 사용을 지원한다.

## 사용법

```text
/explore new <name>              새 exploration 시작
/explore <name>                  기존 exploration 재개
/explore list                    활성 exploration 목록
/explore promote <name> <label>  candidate를 정식 feature issue로 승격
/explore close <name>            탐색 완료 및 findings.md 작성
/explore context <name>          외부 LLM 붙여넣기용 briefing 생성
/explore sync <name>             외부 LLM 대화 결과를 discussion.md에 기록
```

## 폴더 구조

```text
explorations/<name>/
  question.md     탐색 질문, 범위, 성공 기준
  candidates.md   경쟁하는 접근 방향들 (active | eliminated | promoted)
  discussion.md   append-only 논의 로그
  findings.md     결론 요약 (close 시 작성)
```

---

## 서브커맨드

### new — 새 exploration 시작

**동작**:

1. `explorations/<name>/` 폴더가 없으면 아래 4개 파일을 생성한다.

**question.md 템플릿**:

```markdown
# <Name> — 탐색 질문

> 핵심 질문 (한 줄)

## 컨텍스트

[현재 상황, 무엇이 문제인가]

## 탐색 범위

[고려할 측면]

Out of scope: [탐색하지 않을 것]

## 성공 기준

[이 탐색이 완료되려면 무엇을 알아야 하는가]
```

**candidates.md 템플릿**:

```markdown
# 후보 방향들
```

**discussion.md 템플릿**:

```markdown
# 탐색 로그
```

**findings.md 템플릿**:

```markdown
# 탐색 결과
```

2. `BACKLOG.md`의 `## Explorations` 테이블에 행을 추가한다 (Status=active).
3. 사용자에게 핵심 질문을 물어 `question.md`를 채운다.
4. **Brainstorming** — 아래를 병렬로 실행한 뒤, 관련 소스 파일을 조회해 탐색을 계속한다:
   - **WebSearch** / **WebFetch**: Unity 공식 문서, GitHub 이슈, 관련 패턴 레퍼런스 검색
   - **병렬 Agent**: 동일 설계 문제를 복수 관점(성능 / 유지보수성 / Unity 관행 / 테스트 용이성)에서 동시 탐색
   - **TodoWrite**: 탐색 중 부상하는 후보를 실시간 캡처 → `candidates.md` 초안에 반영
   - 새 candidate 도출 시 `discussion.md`에 `[YYYY-MM-DD | brainstorm]` 태그로 append

---

### `<name>` — 기존 exploration 재개

인자가 슬래시 없는 이름이면 `explorations/<name>/`을 로드해 논의를 이어간다.

**동작**:

1. `question.md`, `candidates.md` 전체 읽기
2. `discussion.md` 마지막 20개 항목 읽기 (context 절약)
3. 컨텍스트 복원 후 논의 재개
4. 새 논의 내용은 `discussion.md`에 `[YYYY-MM-DD | explore]` 형식으로 append

---

### list — 활성 exploration 목록

`BACKLOG.md`의 `## Explorations` 섹션을 읽어 출력한다.

---

### promote — candidate를 정식 issue로 승격

```text
/explore promote <name> <candidate-label>
```

**동작**:

1. `candidates.md`에서 해당 candidate 내용을 읽는다.
2. 사용자에게 신규 feature 이름을 확인한다.
3. `features/<feature-name>/` 폴더를 생성하고 `spec.md` 초안을 candidate 내용으로 채운다.
4. `tasks.md`, `decisions.md`, `testcases.md` 빈 템플릿을 생성한다.
5. `BACKLOG.md` Features 테이블에 신규 feature 행을 추가한다 (status=backlog).
6. `candidates.md`에서 해당 candidate의 `**상태**`를 `promoted`로 변경한다.
7. `explorations/<name>/discussion.md`에 아래 형식으로 append한다:

```text
- [YYYY-MM-DD | promote] <Candidate Label> → features/<feature-name>/ 생성
```

8. `BACKLOG.md` Explorations 테이블의 `Promoted To` 열을 업데이트한다.

---

### close — exploration 완료

**동작**:

1. `findings.md`를 아래 형식으로 작성한다:

```markdown
# 탐색 결과

**결론**: ...

**채택된 방향**: ...

**폐기된 방향**: ... — 이유: ...

**생성된 Feature**: [있으면 기재]

**CLAUDE.md 반영 필요**: [있으면 기재, 없으면 "없음"]
```

2. `BACKLOG.md` Explorations 테이블의 Status를 `closed`로 변경한다.
3. `discussion.md`에 `[YYYY-MM-DD | close] 탐색 완료.` append한다.

---

### context — 외부 LLM 브리핑 블록 생성

```text
/explore context <name>
```

**동작**:

1. `question.md`, `candidates.md`, `discussion.md` 최근 항목을 읽는다.
2. 관련 소스 파일의 핵심 부분을 조회한다.
3. 아래 형식으로 출력한다 (터미널에 표시, 사용자가 복사):

```text
## [Exploration: <name>]

**핵심 질문**: ...

**현재 후보들**:
- Candidate A: ...
- Candidate B: ...

**폐기된 방향**: ... (이유: ...)

**핵심 제약** (codebase):
- ...

**관련 코드 요약**:
  [파일명]: [핵심 내용]

**이 대화에서 얻고 싶은 것**: 트레이드오프 비교 / 놓친 접근 발굴 / 특정 질문
```

---

### sync — 외부 LLM 논의 결과 import

```text
/explore sync <name>
```

**동작**:

1. 사용자가 외부 LLM 대화 내용을 붙여넣는다.
2. 핵심 인사이트, 새 candidate, 폐기 근거를 추출한다.
3. `discussion.md`에 `[YYYY-MM-DD | external]` 태그로 요약을 append한다:

```text
- [YYYY-MM-DD | external] <출처 LLM>. 핵심: ... / 새 Candidate: ... / 기존 평가 변경: ...
```

4. 새 candidate가 있으면 `candidates.md`에 추가한다.
5. 기존 candidate 평가가 바뀌었으면 해당 항목의 `**상태**`를 업데이트한다.

---

## discussion.md 로그 태그

| 태그 | 의미 |
| --- | --- |
| `start` | exploration 시작 |
| `explore` | Claude Code 내 논의 요약 |
| `brainstorm` | WebSearch + 병렬 Agent 탐색으로 도출된 후보 |
| `external` | 외부 LLM 논의 import |
| `decision` | 방향을 좁히는 결정 |
| `promote` | candidate → feature issue 승격 |
| `close` | 탐색 완료 |

---

## 주의사항

- `discussion.md`는 append-only. 기존 항목 수정 금지.
- exploration 중에는 Plan 모드 진입 불필요. TASK ID는 promote 이후에만 발급.
- candidates.md에 복수 방향을 동시에 유지해도 된다 (branching 허용).
- promote 전 사용자에게 feature 이름을 반드시 확인한다.
