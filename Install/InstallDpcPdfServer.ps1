<#
.SYNOPSIS
Installs DPC.PdfServer

.DESCRIPTION
DPC.PdfServer provides program interface for generating HTML and PDF

.EXAMPLE
  .\InstallDpcPdfServer.ps1 -port 8035 -pdfLayoutPort 3000 -webApiPort 8016 -siteName 'Dpc.PdfServer' -logPath 'C:\Logs'

.EXAMPLE
   .\InstallDpcPdfServer.ps1 -port 8035 -pdfLayoutPort 3000 -webApiPort 8016 -logPath 'C:\Logs'
#>
param(
    ## Dpc.PdfServer site name
    [Parameter()]
    [String] $siteName ='Dpc.PdfServer',
    ## Dpc.PdfServer port
    [Parameter(Mandatory = $true)]
    [int] $port,
    ## DPC.PdfLayout port
    [Parameter(Mandatory = $true)]
    [int] $pdfLayoutPort,
    ## DPC.SearchApi port
    [Parameter(Mandatory = $true)]
    [int] $searchApiPort,
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

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition

. (Join-Path $currentPath "Modules\Get-SiteOrApplication.ps1")

$b = Get-WebBinding -Port $port
if ($b) { throw "Binding for port $port already exists"}

$s = Get-SiteOrApplication $siteName
if ($s) { throw "Site $siteName already exists"}

$def = Get-SiteOrApplication "Default Web Site"
if (!$def) { throw "Default Web Site doesn't exist"}

$root = $def.PhysicalPath -replace "%SystemDrive%",$env:SystemDrive
$sitePath = Join-Path $root $siteName
Write-Verbose $sitePath
New-Item -Path $sitePath -ItemType Directory -Force | Out-Null

$parentPath = Split-Path -parent $currentPath
$sourcePath = Join-Path $parentPath "PdfServer"

Write-Host "Copying files from $sourcePath to $sitePath..."
Copy-Item "$sourcePath\*" -Destination $sitePath -Force -Recurse
Write-Host "Done"

$nLogPath = Join-Path $sitePath "nlog.config"

[xml]$nlog = Get-Content -Path $nLogPath

$nlog.nlog.internalLogFile = [string](Join-Path $logPath "internal-log.txt")

$node = $nlog.nlog.variable | Where-Object {$_.name -eq 'logDirectory'}
$node.value = [string](Join-Path $logPath $siteName)

$node = $nlog.nlog.rules.logger | Where-Object {$_.writeTo -eq 'console'}
$node.writeTo = "allfile"

Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)

$appSettingsPath = Join-Path $sitePath "appsettings.json"
$json = Get-Content -Path $appSettingsPath | ConvertFrom-Json

$json.PSObject.Properties.Remove("ConfigurationService")

$nodeServer = $json.NodeServer
$nodeServer | Add-Member NoteProperty "GenerateBaseUrl" "http://${env:COMPUTERNAME}:$pdfLayoutPort" -Force
$nodeServer | Add-Member NoteProperty "OutputBaseUrl" "http://${env:COMPUTERNAME}:$pdfLayoutPort/output" -Force

$nodeServer = $json.DPCApi
$nodeServer | Add-Member NoteProperty "BaseUrl" "http://${env:COMPUTERNAME}:$searchApiPort/api" -Force

Set-ItemProperty $appSettingsPath -name IsReadOnly -value $false
$json | ConvertTo-Json | Out-File $appSettingsPath

$p = Get-Item "IIS:\AppPools\$siteName" -ErrorAction SilentlyContinue

if (!$p) { 

    Write-Host "Creating application pool $siteName..."

    $p = New-Item –Path "IIS:\AppPools\$siteName"
    $p | Set-ItemProperty -Name managedRuntimeVersion -Value 'v4.0'

    Write-Host "Done"
}

$s = New-Item "IIS:\sites\$siteName" -bindings @{protocol="http";bindingInformation="*:${port}:"} -physicalPath $sitePath -type Site
$s | Set-ItemProperty -Name applicationPool -Value $siteName
