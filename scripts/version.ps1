param (
    [Parameter(Mandatory = $true)]
    [string]
    $commitSHA,
    [Parameter(Mandatory = $true)]
    [string]
    $prevSHA,
    [Parameter(Mandatory = $true)]
	[string]
	$commitMsg
)

& git config user.name efeozyer
& git config user.email efeozyer@yandex.com

foreach($directory in Get-ChildItem -Directory -Path ./src)
{
	$files = (& git diff $prevSHA $commitSHA --name-only $directory.FullName "$($directory.FullName)")
    
    if ($files.Count -gt 0) 
    {
        $csprojFile = "$($directory.FullName)\$($directory.Name).csproj"

        $csprojXml = [Xml] (Get-Content $csprojFile)

        $currentVersion = [version] $csprojXml.Project.PropertyGroup.Version

        if ([string]::IsNullOrWhiteSpace($csprojXml.Project.PropertyGroup.Version)) 
        {
            $newVersion = [version]::new(0, 0, 1);
            $versionNode = $csprojXml.CreateElement("Version")
            $versionNode.InnerText = $newVersion
            $csprojXml.SelectSingleNode("//Project//PropertyGroup").AppendChild($versionNode) | Out-Null
        }
        ElseIf($commitMsg.Contains("--")) 
        {
            If($commitMsg.Contains("--major")) 
            {
                $newVersion = [version]::new($currentVersion.Major + 1, 0, 0)
            }
            ElseIf($commitMsg.Contains("--minor")) 
            {
                $newVersion = [version]::new($currentVersion.Major, $currentVersion.Minor + 1, 0)
            }
            ElseIf($commitMsg.Contains("--patch")) 
            {
                $newVersion = [version]::new($currentVersion.Major, $currentVersion.Minor, $currentVersion.Build + 1)
            }

            If([string]::IsNullOrWhiteSpace([string]$newVersion)) {
                $newVersion = $currentVersion;
            }

            $finalVersion = [string]::Empty;

            If($commitMsg.Contains("--alpha")) {
                $finalVersion = [string]$newVersion + '-alpha'
            }
            ElseIf($commitMsg.Contains("--beta")) {
                $finalVersion = [string]$newVersion + '-beta'
            }
            ElseIf($commitMsg.Contains("--preview")) {
                $finalVersion = [string]$newVersion + '-preview'
            }

            Else {
                $finalVersion = [string]$newVersion
            }

            $csprojXml.Project.PropertyGroup.Version = $finalVersion;
			
			$csprojXml.Save($csprojFile)
			
            & git add .
			
			& git commit -m "Package version upgraded to $($finalVersion)"

            & git push origin
        }
    }
}