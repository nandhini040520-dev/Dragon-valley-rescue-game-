$ErrorActionPreference = 'Stop'
$phaseArgs = '-questQA -screen-fullscreen 0 -screen-width 1600 -screen-height 900 -qaFolder "' + (Join-Path $PSScriptRoot 'QA\Phase2') + '" -logFile "' + (Join-Path $PSScriptRoot 'QA\Phase2\player.log') + '"'
# A visible interactive game window is required for screenshot validation.
$phaseProcess = Start-Process -FilePath (Join-Path $PSScriptRoot 'Build\Phase2\Adventure Quest Phase 2.exe') -ArgumentList $phaseArgs -WindowStyle Normal -PassThru -Wait
Get-Content -LiteralPath (Join-Path $PSScriptRoot 'QA\Phase2\acceptance-results.txt')
if ($phaseProcess.ExitCode -ne 0) { throw 'Phase 2 acceptance failed.' }
