param([string]$QuestRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference='Stop'
$questSoffice=Get-ChildItem -LiteralPath (Join-Path $QuestRoot 'QA\Tools\LibreOffice') -Recurse -Filter 'soffice.exe' | Select-Object -First 1
if(!$questSoffice) { throw 'Extracted renderer is missing.' }
$env:PATH=$questSoffice.DirectoryName+';C:\Users\Karthika S\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin;'+$env:PATH
foreach($questFile in Get-ChildItem -LiteralPath (Join-Path $QuestRoot 'HackathonDocs') -Filter '*.docx') {
    $questOut=Join-Path $QuestRoot ('QA\DocumentRenders\'+$questFile.BaseName)
    & 'C:\Users\Karthika S\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' 'C:\Users\Karthika S\.codex\plugins\cache\openai-primary-runtime\documents\26.826.12353\skills\documents\render_docx.py' $questFile.FullName --output_dir $questOut --emit_pdf
    if($LASTEXITCODE -ne 0) { throw "Render failed: $($questFile.Name)" }
}
