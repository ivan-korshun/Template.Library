#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Registers a hardcoded list of host names as 127.0.0.1 in the Windows hosts file.
#>

# ---- Hardcoded configuration -------------------------------------------
$IpAddress = '127.0.0.1'

$HostNames = @(
    'library-api',
    'org-api'
)
# ------------------------------------------------------------------------

$hostsPath = Join-Path $env:SystemRoot 'System32\drivers\etc\hosts'

if (-not (Test-Path $hostsPath)) {
    throw "Hosts file not found: $hostsPath"
}

$lines = @(Get-Content -Path $hostsPath)

# Host names already mapped to the target IP (comments ignored)
$existing = foreach ($line in $lines) {
    $clean = ($line -split '#')[0].Trim()
    if ($clean) {
        $parts = $clean -split '\s+'
        if ($parts[0] -eq $IpAddress) { $parts | Select-Object -Skip 1 }
    }
}

# Keep only new, unique, non-empty names (comparison is case-insensitive)
$toAdd = $HostNames |
    ForEach-Object { $_.Trim() } |
    Where-Object { $_ -and ($existing -notcontains $_) } |
    Select-Object -Unique

if (-not $toAdd) {
    Write-Host 'Nothing to add. All host names are already registered.' -ForegroundColor Yellow
    return
}

# Backup before modifying
$backup = "$hostsPath.bak_{0:yyyyMMdd_HHmmss}" -f (Get-Date)
Copy-Item -Path $hostsPath -Destination $backup
Write-Host "Backup created: $backup"

# One line per host name
$newLines = $toAdd | ForEach-Object { "$IpAddress`t$_" }

Set-Content -Path $hostsPath -Value ($lines + $newLines) -Encoding ASCII

Write-Host "Added $(@($toAdd).Count) entries:" -ForegroundColor Green
$newLines | ForEach-Object { Write-Host "  $_" }

# Flush DNS cache so changes apply immediately
Clear-DnsClientCache
