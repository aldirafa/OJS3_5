<#
.SYNOPSIS
    Compiles every code example in the documentation, so examples cannot silently drift from the API.

.DESCRIPTION
    - The VB <code> blocks of the XML documentation in Library/ (into xmldoc/).
    - The ```vb, ```csharp and ```fsharp blocks of README.md and docs/wiki/*.md (into vb/, cs/, fs/).
      Blocks whose fence carries "nocheck" (e.g. ```csharp nocheck) are skipped.

    Each example becomes one async method/function, so examples may use Await/await/let! and may assume an
    OjsClient named "ojs". Needs PowerShell 7, Python 3 and the .NET 10 SDK.

.EXAMPLE
    pwsh tools/doc-check/check.ps1
#>
$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$repo = (Resolve-Path (Join-Path $here '../..')).Path
$python = if (Get-Command python3 -ErrorAction SilentlyContinue) { 'python3' } else { 'python' }

& $python -I (Join-Path $here 'extract_xmldoc.py') (Join-Path $repo 'Library') (Join-Path $here 'xmldoc/Snippets.vb')
& $python -I (Join-Path $here 'extract_markdown.py') $here (Join-Path $repo 'README.md') (Join-Path $repo 'docs/wiki')

$failed = $false
foreach ($project in 'xmldoc/XmlDocSnippets.vbproj', 'vb/MarkdownVb.vbproj', 'cs/MarkdownCs.csproj', 'fs/MarkdownFs.fsproj') {
    Write-Host "== $project"
    $output = dotnet build (Join-Path $here $project) -v q -nologo 2>&1
    if ($LASTEXITCODE -ne 0) {
        $failed = $true
        $output | Select-String ' error ' | ForEach-Object { $_.ToString() } | Select-Object -Unique | Write-Host
    } else {
        Write-Host 'OK'
    }
}
if ($failed) { Write-Host 'Some examples do not compile.'; exit 1 }
Write-Host 'All examples compile.'
