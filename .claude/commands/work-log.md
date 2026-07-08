# work-log: 마지막 실행 이후 커밋 분석 → 설계 의도 중심 업무 보고서 생성
<!-- Complexity Hint: Low → haiku -->

## 1. 커밋 목록 조회

기본(무인자): 마지막 work-log 실행 지점 마커(`work-log-mark`) 이후 커밋을 대상으로 한다.

- **base 결정**: `git rev-parse -q --verify work-log-mark`
  - 성공 → base = `work-log-mark`
  - 실패(첫 실행, 마커 없음) → **fallback = 오늘 날짜 기준**. "마커 없음 → 오늘 기준 조회, 실행 후 마커 생성" 1줄 안내.
- **목록 조회** (범위가 여러 날 걸칠 수 있어 날짜 표시):

```
git log <base>..HEAD --author="<user.name>" --format="%h  %ad  %s" --date=format:"%m/%d %H:%M"
```

  (fallback 시 `<base>..HEAD` 대신 `--since="today 00:00"`)
- **빈 범위** (`base..HEAD` 커밋 없음) → "이전 실행 이후 새 커밋 없음" 안내 후 종료 (마커 전진 스킵).

결과를 `[번호]  hash  시각  메시지` 형식으로 출력하고 분석할 번호/all/hash 입력 요청.

**오버라이드** (마커 미전진):
- `/work-log <hash|ref>` → base = 해당 ref (`ref..HEAD` 범위)
- `/work-log --Ndays` → 날짜 N일치 모드

## 2. 커밋 diff 읽기

```text
git show <hash> --stat
git diff <hash>^ <hash>
```

commit message가 아닌 실제 코드 변경 기반으로 설계 의도 해석.

## 3. 업무 보고서 작성

[포맷] 코드 블록 출력, 날짜 없음, 설계 의도 중심, 명사형 종결, 총 문서량 = 7~8줄

- 최상위: * AI Native Development workspace 구축 작업 중
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

## 4. 마커 전진

기본 모드로 실행해 보고서 생성까지 완료되면 마커를 HEAD로 이동한다 (오버라이드 모드는 스킵):

```
git tag -f work-log-mark HEAD
```

출력: `마커 전진: <old7> → <new7>` (재실행·복구용으로 이전 해시 명시)
