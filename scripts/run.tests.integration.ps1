param(
    [switch] $Clean
)

$runArgs = @{
    Files         = "docker-compose.test.integration.yml"
    ServiceName   = "library-integration-tests"
    RemoveOrphans = $true
    Clean         = $Clean
}

& "$PSScriptRoot\_compose.ps1" @runArgs
