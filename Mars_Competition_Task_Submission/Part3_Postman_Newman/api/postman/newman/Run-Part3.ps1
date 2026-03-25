param(
  [ValidateSet('run')]
  [string]$Action = 'run',
  [string]$Collection = 'api/postman/Mars-Part3-A2-A9.postman_collection.json',
  [string]$Environment = 'api/postman/Mars-Part3.local.postman_environment.json',
  [string]$DataFile = 'api/postman/data/part3-users.csv',
  [string]$OutDir = 'api/postman/newman/reports',
  [string]$RunName = 'part3_api'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Get-Command newman -ErrorAction SilentlyContinue)) {
  throw 'Newman is not installed. Install with: npm i -g newman newman-reporter-htmlextra'
}

New-Item -ItemType Directory -Force $OutDir | Out-Null

$jsonOut = Join-Path $OutDir "$RunName-report.json"
$junitOut = Join-Path $OutDir "$RunName-junit.xml"
$htmlSummaryOut = Join-Path $OutDir "$RunName-report.html"
$htmlExtraOut = Join-Path $OutDir "$RunName-htmlextra.html"

$hasHtmlExtra = $false
try {
  $check = newman run $Collection --environment $Environment --iteration-data $DataFile --folder '__no_such_folder__' --reporters htmlextra 2>&1
  if ($check -notmatch 'could not find "htmlextra" reporter') { $hasHtmlExtra = $true }
} catch {
  $hasHtmlExtra = $false
}

$reporters = 'cli,json,junit'

$args = @(
  'run', $Collection,
  '--environment', $Environment,
  '--iteration-data', $DataFile,
  '--timeout-request', '15000',
  '--reporters', $reporters,
  '--reporter-json-export', $jsonOut,
  '--reporter-junit-export', $junitOut
)

Write-Host 'Running Newman (cli,json,junit)...'
newman @args

# Build a lightweight HTML summary from the Newman JSON output so the
# submission folder always contains a human-readable report.
try {
  $report = Get-Content $jsonOut -Raw | ConvertFrom-Json
  $detailRows = foreach ($exec in $report.run.executions) {
    [PSCustomObject]@{
      RequestName = $exec.item.name
      Method = $exec.request.method
      Url = ('{0}://{1}:{2}/{3}' -f $exec.request.url.protocol, ($exec.request.url.host -join '.'), $exec.request.url.port, ($exec.request.url.path -join '/'))
      Status = $exec.response.code
      ResponseTimeMs = [int]$exec.response.responseTime
      Assertions = ($exec.assertions | ForEach-Object { $_.assertion }) -join '; '
    }
  }

  $summary = @(
    [PSCustomObject]@{ Metric = 'Collection'; Value = $report.collection.info.name }
    [PSCustomObject]@{ Metric = 'Requests'; Value = $report.run.stats.requests.total }
    [PSCustomObject]@{ Metric = 'Assertions'; Value = $report.run.stats.assertions.total }
    [PSCustomObject]@{ Metric = 'Failures'; Value = $report.run.failures.Count }
    [PSCustomObject]@{ Metric = 'Average Response Time (ms)'; Value = [int]$report.run.timings.responseAverage }
  )

  $summaryHtml = $summary | ConvertTo-Html -Fragment -PreContent '<h2>Summary</h2>'
  $detailHtml = $detailRows | ConvertTo-Html -Fragment -PreContent '<h2>Request Details</h2>'
  $style = @"
<style>
body { font-family: Segoe UI, Arial, sans-serif; margin: 24px; color: #222; }
h1, h2 { margin-bottom: 12px; }
table { border-collapse: collapse; width: 100%; margin-bottom: 24px; }
th, td { border: 1px solid #d0d7de; padding: 8px; text-align: left; vertical-align: top; }
th { background: #f6f8fa; }
.pass { color: #137333; font-weight: 600; }
</style>
"@
  $html = ConvertTo-Html -Title 'Part 3 Newman HTML Summary' -Body "<h1>Part 3 Newman HTML Summary</h1><p class='pass'>Generated from Newman JSON output.</p>$summaryHtml$detailHtml" -Head $style
  Set-Content -Path $htmlSummaryOut -Value $html
  Write-Host "HTML summary: $htmlSummaryOut"
} catch {
  Write-Host 'HTML summary generation skipped.'
}

if ($hasHtmlExtra) {
  try {
    Write-Host 'Attempting optional htmlextra report...'
    newman run $Collection --environment $Environment --iteration-data $DataFile --reporters cli,htmlextra --reporter-htmlextra-export $htmlExtraOut 2>$null | Out-Null
    Write-Host "htmlextra report: $htmlExtraOut"
  } catch {
    Write-Host 'htmlextra reporter not available; skipped HTML extra report.'
  }
}

Write-Host "JSON report:  $jsonOut"
Write-Host "JUnit report: $junitOut"
