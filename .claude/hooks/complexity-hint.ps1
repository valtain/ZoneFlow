$jsonStr = [Console]::In.ReadToEnd()
if (-not $jsonStr) { exit 0 }

try { $json = $jsonStr | ConvertFrom-Json } catch { exit 0 }
$prompt = $json.prompt
if (-not $prompt) { exit 0 }
$prompt = $prompt.Trim()

if ($prompt -notmatch '^/(\w[\w-]*)(?:\s+(\w[\w-]*))?') { exit 0 }
$command = $Matches[1].ToLower()
$sub     = if ($Matches[2]) { $Matches[2].ToLower() } else { '' }
$map = @{
    # 기존
    'bridge'           = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'git-commit'       = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'work-log'         = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'init'             = @{ level = 'Medium'; model = 'sonnet'; action = 'none' }
    'review'           = @{ level = 'Medium'; model = 'sonnet'; action = 'none' }
    'simplify'         = @{ level = 'Medium'; model = 'sonnet'; action = 'none' }
    'security-review'  = @{ level = 'High';   model = 'opus';   action = 'confirm' }
    'explore'          = @{ level = 'High';   model = 'opus';   action = 'confirm'; agent = 'architecture-director' }
    # issue 서브커맨드
    'issue new'        = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'issue list'       = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'issue show'       = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'issue close'      = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'issue do'         = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'unity-specialist' }
    'issue review'     = @{ level = 'High';   model = 'opus';   action = 'confirm'; agent = 'architecture-director' }
    # feature 서브커맨드
    'feature new'      = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'feature list'     = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'feature show'     = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'feature plan'     = @{ level = 'Medium'; model = 'sonnet'; action = 'none' }
    # next
    'next'             = @{ level = 'Medium'; model = 'sonnet'; action = 'none' }
    # quick
    'quick'            = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    # level 서브커맨드
    'level list'       = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'level new'        = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'level-designer' }
    'level improve'    = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'level-designer' }
    'level review'     = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'level-designer' }
    # ui 서브커맨드
    'ui list'          = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'ui new'           = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'ui-designer' }
    'ui improve'       = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'ui-designer' }
    'ui review'        = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'ui-designer' }
    # battle 서브커맨드
    'battle list'      = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'battle new'       = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'combat-specialist' }
    'battle tune'      = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'combat-specialist' }
    'battle review'    = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'combat-specialist' }
    # systems 서브커맨드
    'systems list'     = @{ level = 'Low';    model = 'haiku';  action = 'auto' }
    'systems new'      = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'systems-designer' }
    'systems improve'  = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'systems-designer' }
    'systems review'   = @{ level = 'Medium'; model = 'sonnet'; action = 'none'; agent = 'systems-designer' }
}

$key  = if ($sub -and $map.ContainsKey("$command $sub")) { "$command $sub" } else { $command }
if (-not $map.ContainsKey($key)) { exit 0 }
$info = $map[$key]

switch ($info.action) {
    'auto' {
        Write-Output "[Complexity Hook] /$key → $($info.level) → model='$($info.model)' 자동 선택"
        Write-Output "이 커맨드는 Low 복잡도입니다. Agent 도구로 model='$($info.model)' 서브에이전트를 생성하여 전체 작업을 위임하세요."
    }
    'confirm' {
        Write-Output "[Complexity Hook] /$key → $($info.level) → model='$($info.model)' 권장"
        Write-Output "이 커맨드는 High 복잡도입니다. 작업 시작 전 사용자에게 확인하세요: '$($info.model) 모델로 진행하시겠습니까? (추가 비용 발생)'"
    }
}

if ($info.agent) {
    Write-Output "[Subagent] /$key → '$($info.agent)' 역할 에이전트로 위임하세요 (Agent 도구, subagent_type='$($info.agent)'). 사용자 승인 게이트는 메인 세션이 중재."
}
