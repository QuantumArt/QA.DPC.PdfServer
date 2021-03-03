<#
.SYNOPSIS
Installs templates and mappers for DPC.PdfServer

.DESCRIPTION
Installs templates and mappers for generating HTML and PDF

.EXAMPLE
  .\InstallTemplates.ps1 -folderName 'test_catalog'

.EXAMPLE
   .\InstallTemplates.ps1
#>
param(
    ## Dpc.PdfServer site name
    [Parameter()]
    [String] $folderName ='test_catalog'
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition

. (Join-Path $currentPath "Modules\Get-SiteOrApplication.ps1")

$def = Get-SiteOrApplication "Default Web Site"
if (!$def) { throw "Default Web Site doesn't exist"}

$root = $def.PhysicalPath -replace "%SystemDrive%",$env:SystemDrive
$sitePath = Join-Path $root $siteName
$templateContents = @(475, 581)
$mapperContents = @(473, 579)

foreach ($templateContent in $templateContents) {
    $folder = "$sitePath\contents\$templateContent"
    Write-Verbose "Copy templates to $folder..."
    New-Item $folder -Type Directory -Force | Out-Null
    Copy-Item "$parentPath\templates\*" -Destination $folder -Force -Recurse
    Write-Verbose "Done"
}

foreach ($mapperContent in $mapperContents) {
    $folder = "$sitePath\contents\$mapperContent"
    Write-Verbose "Copy mappers to $folder..."
    New-Item $folder -Type Directory -Force | Out-Null
    Copy-Item "$parentPath\mappers\*" -Destination $folder -Force -Recurse
    Write-Verbose "Done"
}

