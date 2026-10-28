$ErrorActionPreference = 'Stop'
$questRoot = $PSScriptRoot
$questPlayer = Join-Path $questRoot 'Build\Windows\Adventure Quest.exe'
if (-not (Test-Path -LiteralPath $questPlayer)) { throw 'Build the Windows prototype first with Build-Windows.ps1.' }
$questArgs = '-questQA -screen-fullscreen 0 -screen-width 1600 -screen-height 900 -logFile "' + (Join-Path $questRoot 'QA\player-qa.log') + '"'
# A visible player window is necessary for rendered screenshots and realistic frame pacing.
$questProcess = Start-Process -FilePath $questPlayer -ArgumentList $questArgs -WindowStyle Normal -PassThru -Wait
Get-Content -LiteralPath (Join-Path $questRoot 'QA\acceptance-results.txt')
if ($questProcess.ExitCode -ne 0) { throw 'Acceptance run failed. See QA/acceptance-results.txt and QA/player-qa.log.' }
