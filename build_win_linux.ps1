Param(
  [string]$UnityExe,
  [string]$ProjectPath
)

$ErrorActionPreference = 'Stop'

if (-not $ProjectPath) {
  $ProjectPath = (Resolve-Path $PSScriptRoot).Path
}

$logDir = Join-Path $ProjectPath "Builds"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$logFile = Join-Path $logDir "build.log"

function Resolve-UnityEditor {
  param([string]$Hint)
  if ($Hint -and (Test-Path $Hint)) { return $Hint }

  $hubRoot = "C:\\Program Files\\Unity\\Hub\\Editor"
  if (Test-Path $hubRoot) {
    $candidates = Get-ChildItem $hubRoot -Directory | Sort-Object Name -Descending
    foreach ($dir in $candidates) {
      $exe = Join-Path $dir.FullName "Editor/Unity.exe"
      if (Test-Path $exe) { return $exe }
    }
  }
  return $null
}

$UnityExe = Resolve-UnityEditor -Hint $UnityExe
if (-not $UnityExe) {
  Write-Error "Unity Editor not found. Provide -UnityExe 'C:\\Path\\to\\Unity.exe' or install via Unity Hub."
  exit 2
}

$argList = @(
  "-batchmode",
  "-nographics",
  "-quit",
  "-projectPath \"$ProjectPath\"",
  "-logFile \"$logFile\"",
  "-executeMethod BuildCI.BuildAll"
)

Write-Host "[Build] Unity   : $UnityExe"
Write-Host "[Build] Project : $ProjectPath"
Write-Host "[Build] Log     : $logFile"

$proc = Start-Process -FilePath $UnityExe -ArgumentList $argList -Wait -PassThru -NoNewWindow
$code = $proc.ExitCode

if ($code -ne 0) {
  Write-Host "[Build] FAILED (ExitCode=$code)"
  if (Test-Path $logFile) {
    Write-Host "---- Log Tail (last 200 lines) ----"
    Get-Content $logFile -Tail 200
  }
  exit $code
}
else {
  Write-Host "[Build] SUCCEEDED"
  if (Test-Path $logFile) {
    Write-Host "---- Log Tail (last 50 lines) ----"
    Get-Content $logFile -Tail 50
  }
  exit 0
}
