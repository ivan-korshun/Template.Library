param(
    [switch] $Clean
)

$runArgs = @{
    Files         = "docker-compose.test.e2e.yml"
    ServiceName   = "library-e2e-tests"
    RemoveOrphans = $true
    Clean         = $Clean
}

& "$PSScriptRoot\_compose.ps1" @runArgs
