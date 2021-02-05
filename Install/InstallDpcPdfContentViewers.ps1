<#
.SYNOPSIS
Installs DPC.PdfContentViewers

.DESCRIPTION
DPC.PdfContentViewers is a web application that contains Custom Actions for generating HTML and PDF

.EXAMPLE
  .\InstallDpcPdfContentViewers.ps1 -name PdfMedia
#>
param(
    ## QP site name
    [Parameter()]
    [String] $qp ='QP8',
    ## Dpc.Admin application name
    [Parameter()]
    [String] $name = 'DPC.PdfMedia',
    [Parameter(Mandatory = $true)]
    [int] $pdfServerPort

)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition

. (Join-Path $currentPath "Modules\Get-SiteOrApplication.ps1")

$qpApp = Get-SiteOrApplication -name $qp 
if (!$qpApp) { throw "site $qp is not exists"}

$app = Get-SiteOrApplication -name $qp -application $name 
if ($app) { throw "application $name in $qp already exists"}

$parentPath = Split-Path -parent $currentPath
$sourcePath = Join-Path $parentPath "PdfMedia"

$qpApp = Get-SiteOrApplication -name $qp 
$root = Split-Path -parent $qpApp.PhysicalPath
$appPath = Join-Path $root $name


New-Item -Path $appPath -ItemType Directory -Force | Out-Null

Copy-Item "$sourcePath\*" -Destination $appPath -Force -Recurse

Get-ChildItem "$appPath\src\*index.html" -recurse
{
    (Get-Content $_ | ForEach-Object {$_ -replace "||API_URL||", "http://${env:COMPUTERNAME}:$pdfServerPort"}) | Set-Content $_
}

New-Item "IIS:\sites\$qp\$name" -physicalPath $appPath -type Application | Out-Null


