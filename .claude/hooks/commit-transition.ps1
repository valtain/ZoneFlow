$jsonStr = [Console]::In.ReadToEnd()
if (-not $jsonStr) { exit 0 }

try { $json = $jsonStr | ConvertFrom-Json } catch { exit 0 }

$toolName = $json.tool_name
if ($toolName -notin @('Bash', 'PowerShell')) { exit 0 }

$command = $json.tool_input.command
if (-not $command) { exit 0 }
if ($command -notmatch 'git\s+commit') { exit 0 }

# 커밋 실패 시 (tool_result에 error 포함) 아무것도 하지 않음
$result = $json.tool_result
if ($result -and ($result -match '(?i)error|failed|fatal')) { exit 0 }

Write-Output "[Autopilot Hook] 커밋이 감지되었습니다."
Write-Output "BACKLOG.md에서 in_progress 상태의 Task를 in_review로 전환하세요."
Write-Output "해당 feature의 tasks.md에서도 동일 Task의 Status를 in_review로 업데이트하세요."
