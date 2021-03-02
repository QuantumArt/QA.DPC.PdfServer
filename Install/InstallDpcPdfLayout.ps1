<#
.SYNOPSIS
Installs DPC.PdfLayout

.DESCRIPTION
DPC.PdfLayout is a NodeJS windows service for generating HTML from JS-templates

.EXAMPLE
  .\InstallDpcPdfLayout.ps1 -actionsPort 8011 -notifyPort 8012 -installRoot 'C:\QA' -logPath 'C:\Logs' 

.EXAMPLE
  .\InstallDpcPdfLayout.ps1 -actionsPort 8011 -notifyPort 8012 -installRoot 'C:\QA' -logPath 'C:\Logs' -name 'DPC.PdfLayout' 

#>
param(
    ## Service Display Name
    [Parameter()]
    [String] $name = 'DPC.PdfLayout',
    ## Service Description
    [Parameter()]
    [String] $description = 'Generates HTML from JS-templates',
    ## Path to install DPC Services
    [Parameter(Mandatory = $true)]
    [String] $installRoot,
    ## Service port to run
    [Parameter(Mandatory = $true)]
    [int] $port,
    ## Logs folder
    [Parameter(Mandatory = $true)]
    [String] $logPath,
    ## Temp folder
    [Parameter(Mandatory = $true)]
    [String] $tempPath  

)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$currentPath = Split-path -parent $MyInvocation.MyCommand.Definition
$parentPath = Split-Path -parent $currentPath
$sourcePath = Join-Path $parentPath "PdfLayout"

if (-not(Test-Path $installRoot)) { New-Item $installRoot -ItemType Directory | Out-Null }
$installPath = Join-Path $installRoot $name
if (Test-Path $installPath) { throw "Service folder $installPath already exists" } else { New-Item $installPath -ItemType Directory | Out-Null }
Write-Host "Copy item from $sourcePath to $installPath ..." 
Copy-Item "$sourcePath\*" "$installPath" -Force -Recurse
Write-Host "Done" 

$defaultPath = Join-Path $installPath ".env"
@"
SVC_NAME=$name
SVC_DESCRIPTION=$description
PORT=$port
WORKDIR_PATH=$tempPath\$name.workdir
LOGS_PATH=C:\Logs\$name
OUTPUT_PATH=output
"@ | Out-File $defaultPath

Push-Location $installPath
Invoke-Expression "node installService.js"
Pop-Location

$s = Get-Service -DisplayName $name

if ( $s.Status -eq "Stopped")
{
    Write-Host "Starting service $name..."
    $s.Start()
}
$timeout = "00:03:00";
try { $s.WaitForStatus("Running", $timeout) } catch [System.ServiceProcess.TimeoutException] { throw [System.ApplicationException] "Service '$name' hasn't been started in '$timeout' interval" } 
Write-Host "$name Running"
