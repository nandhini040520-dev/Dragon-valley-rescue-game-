@echo off
setlocal EnableExtensions DisableDelayedExpansion
title Adventure Quest - The Dragon Valley Rescue

rem Works beside the executable or in the Unity project folder.
set "questGameDir=%~dp0"
if exist "%questGameDir%Adventure Quest.exe" goto check_files
set "questGameDir=%~dp0Build\Windows\"
if not exist "%questGameDir%Adventure Quest.exe" goto missing_game

:check_files
if not exist "%questGameDir%UnityPlayer.dll" goto incomplete_game
if not exist "%questGameDir%Adventure Quest_Data\globalgamemanagers" goto incomplete_game
if not exist "%questGameDir%MonoBleedingEdge\EmbedRuntime\mono-2.0-bdwgc.dll" goto incomplete_game
start "Adventure Quest" /D "%questGameDir%" "%questGameDir%Adventure Quest.exe"
if errorlevel 1 goto launch_failed
exit /b 0

:missing_game
echo The Windows game could not be found.
echo Extract the full Adventure-Quest-Windows.zip, then run this file inside it.
echo In the Unity project, the game must be built into Build\Windows.
pause
exit /b 1

:incomplete_game
echo Some game files are missing.
echo Extract the complete Windows game ZIP and keep all its files together.
pause
exit /b 1

:launch_failed
echo Windows could not start Adventure Quest.exe.
echo Try opening the executable in the extracted game folder.
pause
exit /b 1
