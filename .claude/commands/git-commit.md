# git-commit: staged 파일 선택 → .meta 자동처리 → 커밋 메시지 생성 → 커밋
<!-- Complexity Hint: Low → Haiku 4.5 -->

## 1. 변경 파일 선택

`git status --short` 출력 후, stage할 파일 번호를 선택받아. (예: `1 2` / `all` / `skip`)

## 2. diff 읽기

선택 파일의 `git diff HEAD -- <file>` 로 변경 내용 파악. 신규 파일은 내용 직접 읽기.

## 3. .meta 자동 처리

`Assets/` 하위 파일 stage 시 대응 `.meta` 가 있으면 자동 포함. 단 아래는 제외:

- `.`으로 시작하는 파일/폴더, `~`로 끝나는 파일
- `ProjectSettings/`, `Packages/manifest.json`, `Packages/packages-lock.json`

확정 목록을 사용자에게 보여줘.

## 4. git add

확정 목록으로 `git add` 실행.

## 5. 커밋 메시지 작성 및 확인

staged diff를 분석하여 아래 규칙으로 메시지 초안을 작성해.

- 형식: `[prefix] 한국어 메시지`
- prefix: feat / fix / refactor / style / test / docs / chore
- 명사형 종결, 마침표 금지, 설계 의도 중심

메시지 초안을 출력한 뒤 **AskUserQuestion** 도구로 확인을 받아:

- 질문: `"[prefix] 메시지" 로 커밋할까요?`
- 선택지:
  - `커밋` → `git commit -m "<메시지>"` 실행
  - `메시지 수정` → 수정할 내용을 입력받아 메시지를 업데이트하고 이 단계를 반복
  - `취소` → 중단. `git restore --staged .` 실행 후 종료

커밋 완료 후 `git log --oneline -1` 출력으로 실제 커밋 메시지를 검증해.
