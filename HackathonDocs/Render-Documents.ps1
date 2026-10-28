param([string]$QuestRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference='Stop'
$questRenderRoot=Join-Path $QuestRoot 'QA\DocumentRenders'
New-Item -ItemType Directory -Path $questRenderRoot -Force | Out-Null
# This Windows runtime has no LibreOffice. Use the installed Word layout engine,
# then the managed runtime's Poppler to review the actual DOCX pagination.
$questWord=New-Object -ComObject Word.Application
$questWord.Visible=$false
$questWord.DisplayAlerts=0
$questWord.Options.UpdateLinksAtOpen=$false
$questWord.Options.SaveNormalPrompt=$false
try {
    foreach($questFile in (Get-ChildItem -LiteralPath (Join-Path $QuestRoot 'HackathonDocs') -Filter '*.docx' | Sort-Object Length)) {
        $questOut=Join-Path $questRenderRoot $questFile.BaseName
        New-Item -ItemType Directory -Path $questOut -Force | Out-Null
        "Opening $($questFile.Name)"
        $questDoc=$questWord.Documents.Open($questFile.FullName,$false,$true)
        try {
            "Exporting $($questFile.Name)"
            $questPdf=Join-Path $questOut ($questFile.BaseName+'.pdf')
            $questDoc.ExportAsFixedFormat($questPdf,17)
            $questPages=$questDoc.ComputeStatistics(2)
            "$($questFile.Name): $questPages pages (Microsoft Word)"
        } finally { $questDoc.Close(0) }
        & 'C:\Users\Karthika S\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdftoppm.exe' -scale-to 1700 -png $questPdf (Join-Path $questOut 'page')
        if($LASTEXITCODE -ne 0) { throw 'Document image rendering failed.' }
    }
} finally { $questWord.Quit() }
