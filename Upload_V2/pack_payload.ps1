Param(
  [string]$Output = "tester_payload.tar.gz"
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$outPath = Join-Path $root $Output

$include = @(
  "PETEL_MainTester_V2.dll",
  "PETEL_MainTester_V2.deps.json",
  "PETEL_MainTester_V2.runtimeconfig.json",
  "PETEL_Runner_V2.dll",
  "PETEL_Runner_V2.deps.json",
  "PETEL_Runner_V2.runtimeconfig.json",
  "PETEL_V2_Core.dll"
)

$tempDir = Join-Path $env:TEMP ("petel_payload_" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempDir | Out-Null

try {
  foreach ($rel in $include) {
    $src = Join-Path $root $rel
    if (-not (Test-Path -LiteralPath $src)) {
      throw "Missing required payload file: $rel"
    }
    Copy-Item -LiteralPath $src -Destination (Join-Path $tempDir $rel)
  }

  if (Test-Path -LiteralPath $outPath) {
    Remove-Item -LiteralPath $outPath -Force
  }

  $tar = Get-Command tar -ErrorAction SilentlyContinue
  if (-not $tar) {
    throw "tar not found. On Windows, install bsdtar or use a Git Bash / WSL shell."
  }

  tar -czf $outPath -C $tempDir .
  Write-Host "Created payload: $outPath"
}
finally {
  if (Test-Path -LiteralPath $tempDir) {
    Remove-Item -LiteralPath $tempDir -Recurse -Force
  }
}
