param([String]$ScriptArgs = "", [String]$Target = "Default")


Push-Location .\Build
$scriptArgsToUse = "-appName=connectionbox $ScriptArgs"

Write-Host "Running with ScriptArgs $scriptArgsToUse"

.\build.ps1 -Target $Target -ScriptArgs $scriptArgsToUse
$BUILDEXITCODE = $LASTEXITCODE

Pop-Location
Write-Host Exiting with code $BUILDEXITCODE

exit $BUILDEXITCODE
