<#
.SYNOPSIS
Updates customer code

.DESCRIPTION
Updates customer code for enabling HTML and PDF generation

.EXAMPLE
  .\UpdateCustomerCode.ps1 -customerCode 'test_catalog'

.EXAMPLE
   .\UpdateCustomerCode.ps1
#>
param(
    ## Dpc.PdfServer site name
    [Parameter()]
    [String] $customerCode ='test_catalog'
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$parentPath = Split-Path -parent $currentPath

. (Join-Path $currentPath "Modules\Database.ps1")
. (Join-Path $currentPath "Modules\CustomerCode.ps1")

$customer = Get-CustomerCode -CustomerCode $customerCode

$sb = New-Object System.Data.Common.DbConnectionStringBuilder
$cnnstr = $customer.db.Replace("Provider=SQLOLEDB;", "")
$sb.set_ConnectionString($cnnstr)

$cnnParams = @{
    Server = if ($sb["Server"]) { $sb["Server"] } else { $sb["Data Source"] };
    Database = if ($sb["Database"]) { $sb["Database"] } else { $sb["Initial Catalog"] };
    Name = $sb["User Id"];
    Pass = $sb["Password"];
    DbType = if ($customer.db_type -eq "postgres") { 1 } else { 0 }
}

$bittrue =  if ($dbType -eq 0) { "1" } else { "true" }
$now = if ($dbType -eq 0) { "getdate()" } else { "now()" }

$customActionQuery = "
delete from context_menu_item where action_id in (select id from backend_action where id in (select action_id From custom_action where alias in ('pdf_viewer', 'html_viewer', 'mapping_viewer')));

insert into context_menu_item(context_menu_id, action_id, name, icon, 'order')
select cm.id, ba.id, ca.name, ca.icon_url, ca.order + 1000 From custom_action ca 
inner join backend_action ba on ca.action_id = ba.id
inner join entity_type et on ba.entity_type_id = et.id
inner join context_menu cm on et.code = cm.code
where ca.alias in ('pdf_viewer', 'html_viewer', 'mapping_viewer');

update custom_action set show_in_menu = $bittrue, modified = $now where alias in ('pdf_viewer', 'html_viewer', 'mapping_viewer');
"
Execute-Sql @cnnParams -query $customActionQuery | Out-Null

Write-Host "Database ${cnnParams.Database} updated"  

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

