$ErrorActionPreference = 'Stop'
$questRoot = $PSScriptRoot
$questEditor = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
if (-not (Test-Path -LiteralPath $questEditor)) { throw 'Unity 6000.6.0f1 is required. Update the editor path in this script if installed elsewhere.' }
New-Item -ItemType Directory -Force -Path (Join-Path $questRoot 'QA') | Out-Null
$questArgs = '-batchmode -quit -projectPath "' + $questRoot + '" -executeMethod DragonValley.Editor.AdventureBuilder.Build -logFile "' + (Join-Path $questRoot 'QA\unity-build.log') + '"'
$questProcess = Start-Process -FilePath $questEditor -ArgumentList $questArgs -WindowStyle Hidden -PassThru -Wait
if ($questProcess.ExitCode -ne 0) { throw 'Build failed. See QA/unity-build.log.' }
Copy-Item -LiteralPath (Join-Path $questRoot 'Run-Adventure.bat') -Destination (Join-Path $questRoot 'Build\Windows\Run-Adventure.bat') -Force
Get-Content -LiteralPath (Join-Path $questRoot 'QA\build-result.txt')
