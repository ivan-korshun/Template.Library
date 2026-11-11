param(
    [Parameter(Mandatory = $false)][string] $ServiceName,
    [Parameter(Mandatory = $false)][string] $ProjectName,
    [Parameter(Mandatory = $false)][string[]] $Files,
    [switch] $Clean,
    [switch] $Detach,
    [switch] $RemoveOrphans
)

Push-Location "$PSScriptRoot\..\"
$elapsed = [System.Diagnostics.Stopwatch]::StartNew()

try {
    $fileArgs = @()
    foreach ($file in $Files) {
        $fileArgs += @("-f", $file)
    }

    $projectArgs = if ($ProjectName) { @("-p", $ProjectName) } else { @() }
    $orphanArgs = if ($RemoveOrphans) { @("--remove-orphans") } else { @() }

    $downArgs =  $projectArgs + $fileArgs + @("down") + $orphanArgs
    docker-compose $downArgs
    docker volume prune -f


    $composeArgs = $projectArgs + @("--env-file", ".env") + $fileArgs + @("up") + $orphanArgs + @("--build")

    if ($Detach) {
        $composeArgs += "--detach"
    }

    if ($ServiceName) {
        $composeArgs += $ServiceName
    }

    Write-Host "Executing docker compose with $($composeArgs -join ' ')"

    docker-compose $composeArgs

    if ($Clean) {
        docker-compose $downArgs
    }
}
finally {
    Write-Host "Total time: $($elapsed.Elapsed.ToString())"
    Pop-Location
}
