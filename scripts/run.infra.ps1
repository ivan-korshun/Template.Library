param(
    [switch] $Clean
)

$runArgs = @{
    Files         = "docker-compose.infra.yml"
    RemoveOrphans = $true
    Clean         = $Clean
}

& "$PSScriptRoot\_compose.ps1" @runArgs
