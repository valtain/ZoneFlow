# /quick
<!-- Complexity Hint: Low → Haiku 4.5 -->

이슈 추적 없이 소규모 작업을 즉시 처리한다.

## 사용법

```text
/quick                  description 없이 시작 → 내부에서 무엇을 할지 질문
/quick <description>    바로 작업 내용 전달
```

예시:
```text
/quick
/quick superpowers 플러그인 활성화
/quick BACKLOG.md의 zone_system feature 상태를 active로 변경
/quick coding-style.md에 UniTask 가이드라인 추가
/quick .claude/settings.json에 권한 항목 추가
```

---

## 적합 범위

아래 조건을 모두 만족하면 `/quick` 사용:
- 설정 변경, 플러그인 활성화, 문서 업데이트, 단순 텍스트 수정
- 변경 파일 2개 이하
- 새 C# 코드 구현이 없음

아래 중 하나라도 해당하면 다른 커맨드 안내:
- 새 C# 클래스·메서드 구현 → `/next <feature_name>` 또는 `/explore`
- 아키텍처 결정이 필요한 변경 → `/explore`
- 3개 이상 파일에 걸친 연동 변경 → `/next <feature_name>`

---

## 동작

1. description이 없으면 **AskUserQuestion** 도구로 "어떤 작업을 할까요?" 질문 후 입력 대기. 있으면 그대로 사용.
2. **TodoWrite** 도구로 작업 단계 체크리스트 생성:
   - `[ ] 파일 목록 확인 및 사용자 승인`
   - `[ ] 파일 수정`
   - `[ ] 커밋 여부 확인`
3. 내용을 실행 의도로 해석하고 변경될 파일 목록을 예상해 출력한다.
4. **AskUserQuestion** 도구로 확인:
   - 질문: `이 파일들을 수정할까요?`
   - 선택지: `진행`, `취소`
5. `진행` 선택 시 작업을 실행한다. TodoWrite의 `파일 수정` 항목 체크.
6. 변경 요약을 출력하고 `/git-commit` 실행 여부를 **AskUserQuestion** 도구로 확인:
   - 질문: `변경 완료. 커밋할까요?`
   - 선택지: `커밋`, `나중에`

---

## 주의사항

- 범위를 벗어난 작업이라고 판단되면 먼저 알리고 적합한 커맨드를 제안한다.
- 파일을 변경하기 전에 반드시 현재 내용을 읽는다.
