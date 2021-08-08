param (
    [Parameter(Mandatory = $true)]
    [string]
    $commitSHA,
    
    [Parameter(Mandatory = $true)]
    [string]
    $prevSHA
)


foreach($directory in Get-ChildItem -Directory -Path ./src)
{
	$files = (& git diff $prevSHA $commitSHA --name-only $directory.FullName "$($directory.FullName)")
    if ($files.Count -gt 0) 
    {
         $csprojFile = "$($directory.FullName)\$($directory.Name).csproj"

         & dotnet pack $csprojFile --output .\\packages --no-restore --no-build --configuration Release

         if (-Not($?)) 
         {
            throw "Package failed!"
         }
    }
}