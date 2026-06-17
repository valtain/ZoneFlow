param([switch]$WhatIf)

Set-StrictMode -Off

# gh CLI가 출력하는 UTF-8 JSON(한글 description 등)을 올바로 디코딩하기 위해 콘솔 출력 인코딩 고정
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch {}

$repoRoot = (git rev-parse --show-toplevel 2>$null).Trim()
if (-not $repoRoot -or $LASTEXITCODE -ne 0) {
    Write-Error "Git 저장소 루트를 찾을 수 없습니다."
    exit 1
}

$featuresDir = Join-Path $repoRoot "features"
if (-not (Test-Path $featuresDir)) {
    Write-Host "features/ 디렉토리 없음 — 동기화 불필요"
    exit 0
}

$taskFiles = Get-ChildItem -Path $featuresDir -Filter "tasks.md" -Recurse
if (-not $taskFiles) {
    Write-Host "tasks.md 파일 없음 — 동기화 불필요"
    exit 0
}

# ── 보드 Status 이름 → tasks.md 토큰 매핑 ─────────────────────────────────
$statusToken = @{
    'Todo'        = 'todo'
    'In Progress' = 'in-progress'
    'Blocked'     = 'blocked'
    'Done'        = 'done'
}

# ── 0단계: Project 메타 로드 ──────────────────────────────────────────────
$projectFile = Join-Path $repoRoot ".claude/github-project.json"
if (-not (Test-Path $projectFile)) {
    Write-Error ".claude/github-project.json 없음 — 보드 번호를 알 수 없습니다."
    exit 1
}
$projectNumber = (Get-Content $projectFile -Raw -Encoding UTF8 | ConvertFrom-Json).number
$owner = (gh repo view --json owner --jq '.owner.login' 2>$null).Trim()
if ($LASTEXITCODE -ne 0 -or -not $owner) {
    Write-Error "GitHub 저장소 owner를 조회할 수 없습니다 (gh 인증 확인)."
    exit 1
}

# ── 1단계: 이슈 번호 수집 (마지막 컬럼 기준) ─────────────────────────────
# 마지막 컬럼이 `#N` 또는 `#N <token>` (closed|todo|in-progress|blocked|done) 인 행만 대상
$issueSet = [System.Collections.Generic.HashSet[string]]::new()

foreach ($file in $taskFiles) {
    foreach ($line in (Get-Content $file.FullName -Encoding UTF8)) {
        if ($line -notmatch '^\|') { continue }
        $cols = ($line.TrimEnd().TrimEnd('|') -split '\|') |
                ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' }
        if ($cols.Count -lt 1) { continue }
        if ($cols[-1] -match '^#(\d+)(?:\s+(closed|todo|in-progress|blocked|done))?$') {
            [void]$issueSet.Add($Matches[1])
        }
        elseif ($cols[-1] -match '^#(\d+)\b') {
            Write-Warning "  [$($file.Directory.Name)] 비표준 상태 컬럼 '$($cols[-1])' — 건너뜀"
        }
    }
}

if ($issueSet.Count -eq 0) {
    Write-Host "이슈 번호 없음 — 동기화 불필요"
    exit 0
}

Write-Host "Project #$projectNumber ($owner) 보드 상태 조회 중..."

# ── 2단계: 보드 상태 배치 조회 (1회 호출) ────────────────────────────────
$json = (gh project item-list $projectNumber --owner $owner --format json --limit 200 2>$null) -join "`n"
if ($LASTEXITCODE -ne 0 -or -not $json.Trim()) {
    Write-Error "보드 조회 실패 — gh project item-list (인증·권한 확인)."
    exit 1
}

$boardStatus = @{}   # "이슈번호" → "Todo" | "In Progress" | "Blocked" | "Done"
foreach ($item in (($json | ConvertFrom-Json).items)) {
    if ($item.content.type -ne 'Issue') { continue }
    $num = [string]$item.content.number
    if ($num) { $boardStatus[$num] = $item.status }
}

# ── 3단계: tasks.md 업데이트 ──────────────────────────────────────────────
$changedFiles = [System.Collections.Generic.List[string]]::new()

foreach ($file in $taskFiles) {
    $lines    = Get-Content $file.FullName -Encoding UTF8
    $newLines = [System.Collections.Generic.List[string]]::new()
    $changed  = $false

    foreach ($line in $lines) {
        $out = $line

        if ($line -match '^\|') {
            $cols = ($line.TrimEnd().TrimEnd('|') -split '\|') |
                    ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' }

            if ($cols.Count -ge 1 -and $cols[-1] -match '^#(\d+)(?:\s+(closed|todo|in-progress|blocked|done))?$') {
                $n       = $Matches[1]
                $oldTok  = if ($Matches[2]) { $Matches[2] } else { '(none)' }

                if ($boardStatus.ContainsKey($n)) {
                    $status = $boardStatus[$n]
                    $token  = if ($status -and $statusToken.ContainsKey($status)) { $statusToken[$status] } else { $null }

                    $replacement = if ($token) { "`$1 $token |" } else { "`$1 |" }
                    # 줄 끝 앵커로 마지막 컬럼만 교체 (기존 토큰이 무엇이든)
                    $candidate = $line -replace "(\| *#$n)(?: +[^|]*?)? *\|\s*$", $replacement
                    if ($candidate -ne $line) {
                        $newTok = if ($token) { $token } else { '(none)' }
                        Write-Host "  [보드][$($file.Directory.Name)] #$n  $oldTok → $newTok"
                        $out = $candidate; $changed = $true
                    }
                }
            }
        }

        $newLines.Add($out)
    }

    if ($changed) {
        if (-not $WhatIf) {
            $newLines | Set-Content $file.FullName -Encoding UTF8
        }
        $changedFiles.Add($file.FullName)
    }
}

# ── 4단계: 결과 요약 ──────────────────────────────────────────────────────
if ($changedFiles.Count -eq 0) {
    Write-Host "변경 없음 — 모두 동기화됨"
    exit 0
}

if ($WhatIf) {
    Write-Host "[WhatIf] $($changedFiles.Count)개 파일 변경 예정 (실제 저장 안 함)"
} else {
    Write-Host "$($changedFiles.Count)개 파일 업데이트 완료"
}
