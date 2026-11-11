param(
    [switch] $Clean,
    [switch] $Detach
)

$runArgs = @{
    ServiceName   = "library-api"
    RemoveOrphans = $true
    Clean         = $Clean
    Detach        = $Detach
}

& "$PSScriptRoot\_compose.ps1" @runArgs
