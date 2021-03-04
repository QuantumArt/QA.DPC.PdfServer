<#
.SYNOPSIS
Deletes components from previous installation

.DESCRIPTION
While deleting
- For windows services:
    • service is stopped
    • service files are removed
- For web applications:
    • application is removed from IIS
    • application files are removed
- Deletes customer code from QP

.EXAMPLE
  .\UninstallConsolidation.ps1 -installRoot 'C:\QA' -pdfContentViewers 'Dpc.PdfContentViewers' -pdfLayoutName 'DPC.PdfLayout' -pdfServerName 'DPC.PdfServer'
#>
param(
    ## Dpc.PdfContentViewers application name
    [Parameter()]
    [string] $pdfContentViewers = 'Dpc.PdfContentViewers',
    ## DPC.ActionsService service name
    [Parameter()]
    [string] $pdfLayoutName = 'DPC.PdfLayout',
    ## Dpc.SearchApi site name
    [Parameter()]
    [string] $pdfServerName = 'Dpc.PdfServer',
    ## QP site name
    [Parameter()]
    [string] $qpName = 'QP8',
    ## Путь к каталогу установки сервисов каталога
    [Parameter()]
    [String] $installRoot = 'C:\QA'
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}


if (-not(Get-Module -Name WebAdministration)) {
    Import-Module WebAdministration
}

function DeleteService    
{
   param(
     [string] $name,
     [string] $installRoot
   )

   $s = Get-Service -DisplayName $name -ErrorAction SilentlyContinue

    if ($s){
        if ( $s.Status -eq "Running"){
            Write-Host "$name stopping "
            $s.Stop()
            $s.WaitForStatus("Stopped", "00:03:00")
            Start-Sleep -s 10
            Write-Host "$name stopped"
        }

        $sobj = Get-WmiObject -Class Win32_Service -Filter "DisplayName='$name'" 
        $sobj.Delete() | Out-Null    
        Write-Host "$name deleted"   
    }
    else{
        Write-Host "$name is not installed"
    }

    $path = Join-Path $installRoot $name

    if (Test-Path $path){
        Remove-Item $path -Recurse -Force
        Write-Host "$name files removed"
    }
    else{
        Write-Host "$name files is not exists"
    }
}

function DeleteSite
{
    param(
        [string] $qp,
        [string] $name
    )  

    $alias = if ($qp) { "IIS:\sites\$qp\$name" } else { "IIS:\sites\$name" }
    $displayName = if ($qp) { "application $name for site $qp" } else { "Site $name" }

    $app = Get-Item $alias -ErrorAction SilentlyContinue

    if ($app) {      
        $path =  $app.PhysicalPath
        $poolName = $app.applicationPool

        if ($poolName) {
            Stop-AppPool $poolName | Out-Null
            Remove-Item "IIS:\AppPools\$poolName" -Recurse -Force
            Write-Host "pool $poolName deleted"
        }

        Remove-Item $alias -Recurse -Force    
        Write-Host "$displayName deleted"

        if (Test-Path $path){
            Remove-Item $path -Recurse -Force
            Write-Host "files of $displayName deleted"
        }
    }
}

function Stop-AppPool
{
    param(
        [Parameter(Mandatory = $true)]
        [String] $AppPoolName
    )

    $s = Get-Item "IIS:\AppPools\$AppPoolName" -ErrorAction SilentlyContinue

    if ($s -and $s.State -ne "Stopped")
    {
        Write-Verbose "Stopping AppPool $AppPoolName..." 
        $s.Stop()
        $endTime = $(get-date).AddMinutes('1')
        while($(get-date) -lt $endtime)
        {
            Start-Sleep -Seconds 1
            if ($s.State -ne "Stopping")
            {
                if ($s.State -eq "Stopped") {
                    Write-Verbose "Stopped" 
                }
                break
            }
        }
    }

    return $s.State -eq "Stopped"
}


$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition

if (-not(Get-Module -Name WebAdministration)) {
    Import-Module WebAdministration
}   

DeleteService -name $pdfLayoutName -installRoot $installRoot
DeleteSite -qp $qpName -name $pdfContentViewers
DeleteSite -name $pdfServerName

Remove-CustomerCode -CustomerCode $customerCode