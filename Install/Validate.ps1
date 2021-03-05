<#
    .SYNOPSIS
    Validates whether module could be installed

    .DESCRIPTION
    Checks:
    - ASP.NET Core Runtime 3.1.12 or newer is installed
    - NodeJS v10.x installed
    - QP is installed
    - Ports are available

    .EXAMPLE
    .\ValidateConsolidation.ps1 -actionsPort 8011 -notifyPort 8012 -frontPort 8013 -searchApiPort 8014 -syncApiPort 8015 -webApiPort 8016
#>
param(
    ## QP site name
    [Parameter()]
    [string] $qpName = 'QP8',
    ## Порт DPC.PdfServer
    [Parameter()]
    [int] $pdfServerPort,
    ## Порт DPC.PdfLayout
    [Parameter()]
    [int] $pdfLayoutPort
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

function Test-Port
{
    param(
        [int] $port,
        [string] $name
    )

    if ($port) {
        Write-Host "Checking port $port..."
        $connected = $false

        Try{
            $connected = (New-Object System.Net.Sockets.TcpClient('localhost', $port)).Connected
        } Catch { }

        If ($connected){
            Throw "$name $port is busy"
        }
    }
  
}


$requiredRuntime = '3.1.1[2-9]'
  
Try {
    $actualRuntimes = (Get-ChildItem (Get-Command dotnet).Path.Replace('dotnet.exe', 'shared\Microsoft.AspNetCore.App')).Name
} Catch {
    Write-Error $_.Exception
    Throw "Check ASP.NET Core runtime : failed"
}

if (!($actualRuntimes | Where-Object {$_ -match $requiredRuntime})){ Throw "Check ASP.NET Core runtime 3.1.x (3.1.12 or newer) : failed" }


$requiredNodeVersion = 'v10.[0-9.]'
Try {
    $nodeVersion = Invoke-Expression "node --version"
} Catch {
    Write-Error $_.Exception
    Throw "Check NodeJS version : failed"
} 

if ($nodeVersion -notmatch $requiredNodeVersion){ Throw "Check NodeJS version 10.x : failed" }


$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition
. (Join-Path $currentPath "Modules\Get-SiteOrApplication.ps1")

$qpApp = Get-SiteOrApplication -name $qpName 
if (!$qpApp) { throw "Site $qpName is not exists"}

Test-Port -Port $pdfServerPort -Name "PdfServerPort"
Test-Port -Port $pdfLayoutPort -Name "PdfLayoutPort"
