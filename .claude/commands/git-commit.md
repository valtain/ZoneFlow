# git-commit: staged 파일 선택 → 사안별 그룹핑 → .meta 자동처리 → 순차 커밋
<!-- Complexity Hint: Low → haiku -->

## 1. 변경 파일 선택

`git status --short` 출력 후, stage할 파일 번호를 선택받아. (예: `1 2` / `all` / `skip`)

## 2. diff 읽기

선택 파일의 `git diff HEAD -- <file>` 로 변경 내용 파악. 신규 파일은 내용 직접 읽기.

## 3. 사안별 그룹핑

diff 내용을 분석해 선택 파일을 **사안(concern) 단위 그룹**으로 묶어.

- 서로 무관한 사안은 분리 (예: 새 기능 파일군 / 무관한 버그 수정 / 문서·설정 변경).
- 사안이 하나로 판단되면 그룹은 1개 → 자연히 단일 커밋.

각 그룹마다 아래를 준비해:

- **.meta 자동 포함**: `Assets/` 하위 파일의 대응 `.meta` 가 있으면 해당 그룹에 포함. 단 아래는 제외.
  - `.`으로 시작하는 파일/폴더, `~`로 끝나는 파일
  - `ProjectSettings/`, `Packages/manifest.json`, `Packages/packages-lock.json`
- **메시지 초안**: `[prefix] 한국어 메시지`
  - prefix: feat / fix / refactor / style / test / docs / chore
  - 명사형 종결, 마침표 금지, 설계 의도 중심

## 4. 분할안 제시 및 승인

각 그룹을 `번호 · [prefix] 메시지 · 파일 목록(+meta)` 형태로 출력한 뒤, **AskUserQuestion** 도구로 한 번만 확인받아:

- 질문: `이 분할안 N개 커밋으로 진행할까요?`
- 선택지:
  - `진행` → 5단계로 (승인 시점의 그룹·메시지 확정, 이후 그룹별 재확인 없음)
  - `조정` → 그룹 재구성/메시지 수정 내용을 입력받아 3단계 결과를 갱신하고 이 단계를 반복
  - `단일 커밋` → 전체를 1개 그룹으로 합쳐 진행
  - `취소` → 중단. `git restore --staged .` 실행 후 종료

## 5. 순차 자동 커밋 실행

그룹 간 stage 격리를 위해 **그룹별로 add→commit 을 순차 실행** (한 번에 한 그룹만 stage):

1. `git add <해당 그룹 파일 + meta>`
2. `git commit -m "<해당 그룹 메시지>"`
3. 다음 그룹 반복

승인된 분할안대로 재확인 프롬프트 없이 전체 커밋해.

## 6. 검증

`git log --oneline -<그룹 수>` 출력으로 실제 생성된 커밋들을 검증해.
