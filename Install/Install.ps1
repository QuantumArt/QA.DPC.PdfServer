<#
    .SYNOPSIS
    Installs PdfGenerator module for QP8.ProductCatalog

    .DESCRIPTION
    During installation script does the following actions:
    - Validates input parameters and environment
    - Deletes components from previous installation
    - Installs services and applications:
        • Dpc.PdfLayout: NodeJS service for generating HTML with template and data
        • Dpc.PdfServer: Web API for HTML and PDF generation
        • Dpc.PdfContentViewers: Custom Actions for QP that allow to generate HTML and PDF interactively

    .EXAMPLE
    .\Install.ps1 -installRoot C:\QA -customerCode catalog_consolidation 
    .EXAMPLE
    .\Install.ps1 -installRoot C:\QA -customerCode catalog_consolidation -pdfLayoutPort 5000 -pdfServerPort 8200
#>
param (
    ## Cleanup (or not) previous version of catalog
    [Parameter()]
    [bool] $cleanUp = $true,
    ## Dpc.PdfLayout port
    [Parameter()]
    [int] $pdfLayoutPort = 3000,
    ## Dpc.PdfServer port
    [Parameter()]
    [int] $pdfServerPort = 8035,
    ## Dpc.SearchApi port
    [Parameter()]
    [int] $searchApiPort = 8014,
    ## Folder to install services
    [Parameter(Mandatory = $true)]
    [ValidateScript({ if ($_) { Test-Path $_} })]
    [string] $installRoot,
    ## QP site name
    [Parameter()]
    [string] $qpName = 'QP8',
    ## Dpc.PdfContentViewers application name
    [Parameter()]
    [string] $pdfContentViewers = 'Dpc.PdfContentViewers',
    ## DPC.ActionsService service name
    [Parameter()]
    [string] $pdfLayoutName = 'DPC.PdfLayout',
    ## Dpc.SearchApi site name
    [Parameter()]
    [string] $pdfServerName = 'Dpc.PdfServer',
    ## Customer code
    [Parameter()]
    [string] $customerCode = 'test_catalog',
    ## Upload folder name
    [Parameter()]
    [string] $uploadFolderName = 'test_catalog',
    ## Log folder path
    [Parameter()]
    [string] $logPath = 'C:\Logs',
    ## Temp folder path
    [Parameter()]
    [string] $tempPath = 'C:\Temp'
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

if ($logPath){
    $installLog = Join-Path $logPath "install.log"
    Start-Transcript -Path $installLog -Append
}

$currentPath = Split-path -parent $MyInvocation.MyCommand.Definition
$parentPath = Split-Path -parent $currentPath


if (-not(Get-Module -Name WebAdministration)) {
    Import-Module WebAdministration
}

. (Join-Path $currentPath "Modules\Database.ps1")
. (Join-Path $currentPath "Modules\CustomerCode.ps1")
. (Join-Path $currentPath "Modules\Get-SiteOrApplication.ps1")

if ($cleanUp) {
    $uninstallPath = Join-Path $currentPath "Uninstall.ps1"
    $params = "-InstallRoot '$installRoot' -qpName '$qpName' -pdfContentViewers '$pdfContentViewers' -pdfLayoutName '$pdfLayoutName' -pdfServerName '$pdfServerName'"
    Invoke-Expression "$uninstallPath $params"
}

$def = Get-Item "IIS:\sites\Default Web Site" -ErrorAction SilentlyContinue
if (!$def) { throw "Default Web Site doesn't exist"}

$customer = Get-CustomerCode -CustomerCode $customerCode
if (!$customer) { throw "Customer code $customerCode doesn't exist"}

$validationPath = Join-Path $currentPath "Validate.ps1"
Invoke-Expression "$validationPath -pdfServerPort $pdfServerPort -pdfLayoutPort $pdfLayoutPort"

$scriptName = Join-Path $currentPath "UpdateCustomerCode.ps1"
Invoke-Expression "$scriptName -customerCode $customerCode"

$scriptName = Join-Path $currentPath "InstallDpcPdfContentViewers.ps1"
Invoke-Expression "$scriptName -qp $qpName -name $pdfContentViewers -pdfServerPort $pdfServerPort" 

$scriptName = Join-Path $currentPath "InstallDpcPdfServer.ps1"
Invoke-Expression "$scriptName -port $pdfServerPort -pdfLayoutPort $pdfLayoutPort -searchApiPort $searchApiPort -siteName '$pdfServerName' -LogPath '$logPath'"

$scriptName = Join-Path $currentPath "InstallDpcPdfLayout.ps1"
Invoke-Expression "$scriptName -port $pdfLayoutPort -InstallRoot $installRoot -Name '$pdfLayoutName' -LogPath '$logPath' -TempPath '$tempPath'"

$scriptName = Join-Path $currentPath "InstallTemplates.ps1"
Invoke-Expression "$scriptName -folderName $uploadFolderName"
