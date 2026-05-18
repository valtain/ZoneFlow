# work-log: 오늘의 커밋 분석 → 설계 의도 중심 업무 보고서 생성
<!-- Complexity Hint: Low → Haiku 4.5 -->

## 1. 커밋 목록 조회

기본: 오늘 1일치. `--Ndays`로 N일치, 또는 hash 직접 입력:

```
git log --author="<user.name>" --since="today 00:00" --format="%h  %ad  %s" --date=format:"%H:%M"
```

결과를 `[번호]  hash  시각  메시지` 형식으로 출력하고 분석할 번호/all/hash 입력 요청.

## 2. 커밋 diff 읽기

```text
git show <hash> --stat
git diff <hash>^ <hash>
```

commit message가 아닌 실제 코드 변경 기반으로 설계 의도 해석.

## 3. 업무 보고서 작성

[포맷] 코드 블록 출력, 날짜 없음, 설계 의도 중심, 명사형 종결, 총 문서량 = 7~8줄

- 최상위: * ZoneFlow Framework 작업 중
- 구조 예

```text
  - 섹션명
    - 항목 — 설명
      - 세부 내용
```

[섹션]

- 새 기능 → 기능명 그대로
- 검증/테스트 → "검증 환경 구성"
- 도메인·API 정비 → "설계 개선" (컨벤션과 반드시 분리)
- 네이밍·포맷 → "코딩 컨벤션 적용"
