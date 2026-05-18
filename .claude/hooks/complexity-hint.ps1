$jsonStr = [Console]::In.ReadToEnd()
if (-not $jsonStr) { exit 0 }

try { $json = $jsonStr | ConvertFrom-Json } catch { exit 0 }
$prompt = $json.prompt
if (-not $prompt) { exit 0 }
$prompt = $prompt.Trim()

if ($prompt -notmatch '^/(\w[\w-]*)') { exit 0 }
$command = $Matches[1].ToLower()

$map = @{
    'bridge'          = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'git-commit'      = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'work-log'        = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'init'            = @{ level = 'Medium'; model = 'sonnet'; action = 'none' }
    'review'          = @{ level = 'Medium'; model = 'sonnet'; action = 'none' }
    'simplify'        = @{ level = 'Medium'; model = 'sonnet'; action = 'none' }
    'security-review' = @{ level = 'High';   model = 'opus';   action = 'confirm' }
    'explore'         = @{ level = 'High';   model = 'opus';   action = 'confirm' }
}

if (-not $map.ContainsKey($command)) { exit 0 }
$info = $map[$command]

switch ($info.action) {
    'auto' {
        Write-Output "[Complexity Hook] /$command → $($info.level) → model='$($info.model)' 자동 선택"
        Write-Output "이 커맨드는 Low 복잡도입니다. Agent 도구로 model='$($info.model)' 서브에이전트를 생성하여 전체 작업을 위임하세요."
    }
    'confirm' {
        Write-Output "[Complexity Hook] /$command → $($info.level) → model='$($info.model)' 권장"
        Write-Output "이 커맨드는 High 복잡도입니다. 작업 시작 전 사용자에게 확인하세요: '$($info.model) 모델로 진행하시겠습니까? (추가 비용 발생)'"
    }
}
