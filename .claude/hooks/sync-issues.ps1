param([switch]$WhatIf)

Set-StrictMode -Off

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

# ── 1단계: 이슈 번호 수집 (마지막 컬럼 기준) ─────────────────────────────
# 마지막 컬럼이 `#N` 또는 `#N closed` 인 행만 대상
$issueSet = [System.Collections.Generic.HashSet[string]]::new()

foreach ($file in $taskFiles) {
    foreach ($line in (Get-Content $file.FullName -Encoding UTF8)) {
        if ($line -notmatch '^\|') { continue }
        $cols = ($line.TrimEnd().TrimEnd('|') -split '\|') |
                ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' }
        if ($cols.Count -lt 1) { continue }
        if ($cols[-1] -match '^#(\d+)(?:\s+closed)?$') {
            [void]$issueSet.Add($Matches[1])
        }
    }
}

if ($issueSet.Count -eq 0) {
    Write-Host "이슈 번호 없음 — 동기화 불필요"
    exit 0
}

Write-Host "이슈 $($issueSet.Count)개 GitHub 상태 조회 중..."

# ── 2단계: GitHub 상태 배치 조회 ─────────────────────────────────────────
$ghStates = @{}
foreach ($n in $issueSet) {
    $state = (gh issue view $n --json state --jq '.state' 2>$null).Trim()
    if ($LASTEXITCODE -eq 0 -and $state) {
        $ghStates[$n] = $state          # "OPEN" | "CLOSED"
    } else {
        Write-Warning "  #$n 조회 실패 — 건너뜀"
        $ghStates[$n] = "UNKNOWN"
    }
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

            if ($cols.Count -ge 1 -and $cols[-1] -match '^#(\d+)(?:\s+(closed))?$') {
                $n        = $Matches[1]
                $isClosed = $Matches[2] -eq 'closed'
                $ghState  = $ghStates[$n]

                if ($ghState -eq 'CLOSED' -and -not $isClosed) {
                    # `| #N |` → `| #N closed |`  (마지막 컬럼만 대상)
                    $candidate = $line -replace "(\| *#$n) *\|\s*$", "`$1 closed |"
                    if ($candidate -ne $line) {
                        Write-Host "  [$($file.Directory.Name)] #$n  open → closed"
                        $out = $candidate; $changed = $true
                    }
                }
                elseif ($ghState -eq 'OPEN' -and $isClosed) {
                    # `| #N closed |` → `| #N |`
                    $candidate = $line -replace "(\| *#$n) +closed *\|\s*$", "`$1 |"
                    if ($candidate -ne $line) {
                        Write-Host "  [$($file.Directory.Name)] #$n  closed → open"
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
