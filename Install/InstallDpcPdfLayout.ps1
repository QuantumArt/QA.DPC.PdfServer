<#
.SYNOPSIS
Installs DPC.PdfLayout

.DESCRIPTION
DPC.PdfLayout is a NodeJS windows service for generating HTML from JS-templates

.EXAMPLE
  .\InstallDpcPdfLayout.ps1 -actionsPort 8011 -notifyPort 8012 -installRoot 'C:\QA' -logPath 'C:\Logs' 

.EXAMPLE
  .\InstallDpcPdfLayout.ps1 -actionsPort 8011 -notifyPort 8012 -installRoot 'C:\QA' -logPath 'C:\Logs' -name 'DPC.ActionsService' 

#>
param(
    ## Service Name
    [Parameter()]
    [String] $name = 'DPC.PdfLayout',
    ## Service Display Name
    [Parameter()]
    [String] $displayName = 'DPC PDF Layout Service',
    ## Service Description
    [Parameter()]
    [String] $description = 'Generates HTML from JS-templates',
    ## Path to install DPC Services
    [Parameter(Mandatory = $true)]
    [String] $installRoot,
    ## User account to run service
    [Parameter()]
    [String] $login = 'NT AUTHORITY\SYSTEM',
    ## User password to run service
    [Parameter()]
    [String] $password = 'dummy',
    ## Service port to run
    [Parameter(Mandatory = $true)]
    [int] $port,
    ## Logs folder
    [Parameter(Mandatory = $true)]
    [String] $logPath   

)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$currentPath = Split-path -parent $MyInvocation.MyCommand.Definition

. (Join-Path $currentPath "Modules\Install-Service.ps1")

$parentPath = Split-Path -parent $currentPath
$sourcePath = Join-Path $parentPath "PdfLayout"

$installParams = @{
  name = $name;
  displayName = $displayName;
  description = $description;
  installRoot = $installRoot;
  login = $login;
  password = $password;
  binaryName = "daemon\qadpcnodepdfgenerator.exe";
  source = $sourcePath;
}

Install-Service @installParams

$installPath = Join-Path $installRoot $name
$defaultPath = Join-Path $installPath "config\default.js"
$defaultContent = Get-Content -Path $defaultPath
$defaultContent = $defaultContent.Replace("apiPort: 3000", "apiPort: " + $port)
$defaultContent | Out-File $defaultPath

$s = Get-Service $name

if ( $s.Status -eq "Stopped")
{
    Write-Host "Starting service $name..."
    $s.Start()
}
$timeout = "00:03:00";
try { $s.WaitForStatus("Running", $timeout) } catch [System.ServiceProcess.TimeoutException] { throw [System.ApplicationException] "Service '$name' hasn't been started in '$timeout' interval" } 
Write-Host "$name Running"
