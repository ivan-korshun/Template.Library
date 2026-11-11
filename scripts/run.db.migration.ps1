param(
    [switch] $Clean
)

$runArgs = @{
    Files = "docker-compose.database.migrate.yml"
    Clean = $Clean
}

& "$PSScriptRoot\_compose.ps1" @runArgs
