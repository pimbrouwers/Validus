[CmdletBinding()]
param (
    [Switch] $NoClean,
    [switch] $Watch
)

function RunCommand {
    param ([string] $CommandExpr)
    Write-Verbose "  $CommandExpr"
    Invoke-Expression $CommandExpr
}


$assemblyName = "Validus.Tests"
$assemblyPath = Join-Path -Path $PSScriptRoot -ChildPath "test\$assemblyName\"

if (!(Test-Path -Path $assemblyPath))
{
    throw "Invalid project"
}

if(!$NoClean)
{
    RunCommand -CommandExpr "dotnet clean --nologo --verbosity quiet"
}

RunCommand -CommandExpr "dotnet restore --force --force-evaluate --verbosity quiet"

if ($Watch)
{
    RunCommand -CommandExpr "dotnet watch --project $assemblyPath -- test"
}
else
{
    RunCommand -CommandExpr "dotnet test $assemblyPath"
}