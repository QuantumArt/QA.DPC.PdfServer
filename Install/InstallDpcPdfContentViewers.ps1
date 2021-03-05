<#
.SYNOPSIS
Installs DPC.PdfContentViewers

.DESCRIPTION
DPC.PdfContentViewers is a web application that contains Custom Actions for generating HTML and PDF

.EXAMPLE
  .\InstallDpcPdfContentViewers.ps1 -name PdfContentViewers
#>
param(
    ## QP site name
    [Parameter()]
    [String] $qp ='QP8',
    ## Dpc.Admin application name
    [Parameter()]
    [String] $name = 'DPC.PdfContentViewers',
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
$sourcePath = Join-Path $parentPath "PdfContentViewers"

$qpApp = Get-SiteOrApplication -name $qp 
$root = Split-Path -parent $qpApp.PhysicalPath
$appPath = Join-Path $root $name


New-Item -Path $appPath -ItemType Directory -Force | Out-Null

Write-Host "Copying files from $sourcePath to $appPath..."
Copy-Item "$sourcePath\*" -Destination $appPath -Force -Recurse
Write-Host "Done"

Get-ChildItem "$appPath\*\index.html" | % { (( Get-Content $_ ) -replace "\|\|API_URL\|\|", "http://${env:COMPUTERNAME}:$pdfServerPort/api") | Set-Content $_ -Encoding UTF8 }

$adminPool = Get-Item "IIS:\AppPools\$qp.$name" -ErrorAction SilentlyContinue

if (!$adminPool) { 

    Write-Host "Creating application pool $qp.$name..."

    $adminPool = New-Item –Path "IIS:\AppPools\$qp.$name"
    $adminPool | Set-ItemProperty -Name managedRuntimeVersion -Value 'v4.0'

    Write-Host "Done"
}


New-Item "IIS:\sites\$qp\$name" -physicalPath $appPath -applicationPool "$qp.$name" -type Application | Out-Null


