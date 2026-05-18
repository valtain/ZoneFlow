# Custom Command: /bridge
<!-- Complexity Hint: Low → Haiku 4.5 -->

현재 세션의 핵심 맥락을 압축해 새 대화의 첫 프롬프트로 쓸 요약본을 코드 블록으로 출력한다.

## 출력 항목

- **Project**: 프로젝트명 및 핵심 작업 단계
- **Decisions**: 확정된 설계/로직 선택
- **Progress**: 마지막 수정 파일 및 코드 상태
- **Next**: 새 세션에서 바로 이어할 작업 목록

## 출력 형식

코드 블록 하나로 제공:

```text
### [Session Bridge Context]

* **Project:** {프로젝트명} — {진행 상황}
* **Decisions:**
  - {결정 1}
* **Progress:** {수정 파일 및 상태}
* **Next:**
  1. {작업 1}

준비됐으면 시작하자.
```
