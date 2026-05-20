# Custom Command: /bridge
<!-- Complexity Hint: Low → Haiku 4.5 -->

현재 세션의 핵심 맥락을 압축해 새 대화의 첫 프롬프트로 쓸 요약본을 코드 블록으로 출력한다.

## 생성 전 수집 단계

출력 전 아래를 실행해 최신 상태를 반영한다:
1. `git log -1 --oneline` → 마지막 커밋
2. `gh issue list --state open --json number,title` → 열린 이슈 목록
3. `features/` 하위 tasks.md 스캔 → 진행 중인 feature 파악

## 출력 항목

- **Project**: 프로젝트명 및 핵심 작업 단계
- **Decisions**: 확정된 설계/로직 선택
- **Progress**: 마지막 수정 파일 및 코드 상태
- **Next**: 새 세션에서 바로 이어할 작업 목록
- **Resume**: 새 세션에서 복사해 바로 실행할 커맨드

## 출력 형식

코드 블록 하나로 제공:

```text
### [Session Bridge Context]

* **Project:** {프로젝트명} — {진행 상황}
* **Decisions:**
  - {결정 1}
* **Progress:** {수정 파일 및 상태} | 마지막 커밋: {hash 7자} {메시지}
* **Next:**
  1. {작업 1}
* **Resume:**
  /next {feature_name 또는 issue#}    # {이유 한 줄}

준비됐으면 시작하자.
```

Resume 커맨드 결정 기준:
- 열린 이슈가 있고 in_progress 상태 → `/next <#>`
- 열린 이슈는 있지만 아직 시작 안 함 → `/next <feature_name>`
- 열린 이슈 없음 → `/next` (전체 현황 확인 권장)
