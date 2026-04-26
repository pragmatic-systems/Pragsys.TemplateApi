Set-StrictMode -Version Latest

# Get staged files (added/modified only)
$staged = git diff --cached --name-only --diff-filter=ACM |
          Where-Object { $_ -match '\.(cs|json|yml|yaml|env|sh|ps1|txt|xml)$' }

if (-not $staged) { exit 0 }

# Regex for common secret patterns (case‑insensitive)
$secretRegex = '(?i)(password|secret|api[_-]?key|access_token|token|aws_secret_access_key)\s*[:=]\s*[\''"]?[^\''"\n]+[\''"]?'

Write-Host "Scanning files..."
$found = $false
foreach ($file in $staged) {
    if (Select-String -Path $file -Pattern $secretRegex -Quiet) {
        Write-Host "Potential secret found in $file" -ForegroundColor Red
        $found = $true
    }
}

if ($found) {
    Write-Host "`nCommit aborted – remove the secrets or use git commit --no-verify if intentional." -ForegroundColor Yellow
    exit 1
}

Write-Host "Clean scan..."
exit 0