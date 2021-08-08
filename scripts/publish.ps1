param (
    [Parameter(Mandatory = $true)]
    [string]
    $source,
    
    [Parameter(Mandatory = $true)]
    [string]
    $apiKey
)

$folder = '.\packages'

if(Test-Path -Path $folder) 
{
    foreach($file in Get-ChildItem -Path .\packages)
    {
        dotnet nuget push .\packages\$file --source $source --api-key $apiKey --skip-duplicate
    }   
}