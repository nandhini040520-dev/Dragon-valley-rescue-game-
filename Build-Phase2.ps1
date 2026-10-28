$ErrorActionPreference = 'Stop'
$phaseRoot = $PSScriptRoot
$phaseWorkspace = Join-Path $phaseRoot 'QA\Phase2\BuildWorkspace'
New-Item -ItemType Directory -Force -Path $phaseWorkspace | Out-Null
foreach ($phaseFolder in @('Assets','Packages','ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $phaseRoot $phaseFolder) -Destination $phaseWorkspace -Recurse -Force
}
$phaseArgs = '-batchmode -quit -projectPath "' + $phaseWorkspace + '" -executeMethod DragonValley.Editor.Phase2Builder.Build -phase2Output "' + (Join-Path $phaseRoot 'Build\Phase2\Adventure Quest Phase 2.exe') + '" -logFile "' + (Join-Path $phaseRoot 'QA\Phase2\build.log') + '"'
$phaseProcess = Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe' -ArgumentList $phaseArgs -WindowStyle Hidden -PassThru -Wait
if ($phaseProcess.ExitCode -ne 0) { throw 'Phase 2 build failed. See QA/Phase2/build.log.' }
Get-Content -LiteralPath (Join-Path $phaseRoot 'Build\Phase2\build-result.txt')
