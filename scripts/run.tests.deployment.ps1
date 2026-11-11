param(
    [switch] $Clean
)

$runArgs = @{
    Files       = "docker-compose.test.deployment.yml"
    ServiceName = "library-e2e-tests"
    Clean       = $Clean
}

& "$PSScriptRoot\_compose.ps1" @runArgs
