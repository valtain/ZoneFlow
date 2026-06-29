---
paths:
  - "Assets/**/Editor/**"
---

# Rule: Editor 코드

`Assets/**/Editor/**`의 파일을 Edit/Write 하기 전에 적용한다.
원문(canonical): [docs/conventions/coding-style.md](../../docs/conventions/coding-style.md).

## 필수

- **`#if UNITY_EDITOR`를 사용하지 않는다.** `Editor/` 폴더 내 파일은 Unity가 자동으로 에디터 전용으로 처리한다. (`#if UNITY_EDITOR`는 Runtime 폴더에서 에디터 전용 코드를 *부분* 포함할 때만.)
- 나머지 네이밍·스타일은 Runtime 규칙과 동일 — `_camelCase` private 필드, PascalCase 타입, Interface `I` prefix.
- public/protected 멤버에 한국어 XML doc.
