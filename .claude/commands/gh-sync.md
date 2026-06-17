# /gh-sync
<!-- Complexity Hint: Low → Haiku 4.5 -->

GitHub와 로컬 파일 간 동기화를 수행한다.

## 사용법

```text
/gh-sync              기본값: issues 동기화
/gh-sync issues       features/*/tasks.md 이슈 open/closed 상태 동기화
/gh-sync board        features/*/tasks.md 를 GitHub Project 보드 Status에 맞춤 (역방향)
```

## tasks.md 스키마 규칙 (중요)

- 이슈 토큰은 반드시 **표의 마지막 컬럼**에 `#N` 또는 `#N <token>` 형식으로 둔다.
  `<token>`은 `closed` / `todo` / `in-progress` / `blocked` / `done` 중 하나.
  예: `| 1 | 태스크 설명 | #21 done |`
- 첫 컬럼에 `#N`만 두고 마지막 컬럼에 상태를 **단어**(`closed` 등)로 적으면
  두 sync 스크립트 모두 해당 행을 인식하지 못해 영원히 동기화되지 않는다. 금지.
- **권위 모델**: 상태 컬럼의 단일 권위 소스는 **보드 Status**다.
  - `board` = 수동 주기 실행으로 보드 Status에 수렴(권위 소스).
  - `issues`(야간 워크플로우 포함) = 중간 open/closed 자동 반영. `closed`만 인식하므로
    보드 토큰(`done` 등)이 적힌 행은 건드리지 않는다.

---

## 서브커맨드: issues (기본값)

`features/*/tasks.md`의 이슈 상태(`#N` / `#N closed`)를 GitHub Issues 실제 상태와 맞춘다.

### 동작

1. `.claude/hooks/sync-issues.ps1 -WhatIf` 로 변경 예정 목록 미리 확인
2. 변경이 없으면 "이미 동기화됨" 출력 후 종료
3. 변경이 있으면 목록을 출력하고 **AskUserQuestion** 도구로 확인:
   - 질문: `N개 이슈 상태를 동기화할까요?`
   - 선택지: `동기화`, `취소`
4. `동기화` 선택 시 `.claude/hooks/sync-issues.ps1` 실행
5. 변경된 파일이 있으면 커밋 여부를 **AskUserQuestion** 도구로 확인:
   - 질문: `동기화 완료. 커밋할까요?`
   - 선택지: `커밋`, `나중에`

---

## 서브커맨드: board

GitHub **Project 보드 Status**(Todo / In Progress / Blocked / Done)를 `features/*/tasks.md`의
마지막 컬럼 토큰(`#N todo` / `#N in-progress` / `#N blocked` / `#N done`)에 반영한다. 보드가
권위 소스인 **역방향**(GitHub → 로컬) 동기화다. 보드에 없거나 Status가 비어있는 이슈는 `#N`(토큰 제거).

> 주의: `board` 동기화 후 status 컬럼은 보드 Status가 단일 권위 소스가 된다. `issues`
> 서브커맨드 및 야간 워크플로우(`sync-issues.ps1`)는 `closed`만 인식하므로 보드 토큰(`done` 등)이
> 적힌 행은 건드리지 않는다.

### 동작

1. `.claude/hooks/sync-board.ps1 -WhatIf` 로 변경 예정 목록 미리 확인
2. 변경이 없으면 "이미 동기화됨" 출력 후 종료
3. 변경이 있으면 목록을 출력하고 **AskUserQuestion** 도구로 확인:
   - 질문: `N개 이슈 상태를 보드 Status에 맞춰 동기화할까요?`
   - 선택지: `동기화`, `취소`
4. `동기화` 선택 시 `.claude/hooks/sync-board.ps1` 실행
5. 변경된 파일이 있으면 커밋 여부를 **AskUserQuestion** 도구로 확인:
   - 질문: `동기화 완료. 커밋할까요?`
   - 선택지: `커밋`, `나중에`
