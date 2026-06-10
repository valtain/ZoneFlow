# /gh-sync
<!-- Complexity Hint: Low → Haiku 4.5 -->

GitHub와 로컬 파일 간 동기화를 수행한다.

## 사용법

```text
/gh-sync              기본값: issues 동기화
/gh-sync issues       features/*/tasks.md 이슈 상태 동기화
```

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
