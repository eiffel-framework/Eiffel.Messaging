param (
    [Parameter(Mandatory = $true)]
    [string]
    $commitMsg
)

& git config user.name efeozyer
& git config user.email efeozyer@yandex.com

$ErrorActionPreference = "SilentlyContinue";

try {
    $latestTag = (& git describe --abbrev=0 --tags); -ErrorAction SilentlyContinue
}
catch {
    throw $_
}

$tagVersion = [version]$latestTag;

If($commitMsg.Contains("--major")) 
{
    $tagVersion = [version]::new($tagVersion.Major + 1, 0, 0)
}
ElseIf($commitMsg.Contains("--minor")) 
{
    $tagVersion = [version]::new($tagVersion.Major, $tagVersion.Minor + 1, 0)
}
ElseIf($commitMsg.Contains("--patch")) 
{
    $tagVersion = [version]::new($tagVersion.Major, $tagVersion.Minor, $tagVersion.Build + 1)
}

If($commitMsg.Contains("--alpha")) {
    $tagVersion = [string]$tagVersion + '-alpha'
}
ElseIf($commitMsg.Contains("--beta")) {
    $tagVersion = [string]$tagVersion + '-beta'
}
Else {
    $tagVersion = [string]$tagVersion
}

& git tag -a $tagVersion -m "Release $($tagVersion)"

& git push origin $tagVersion


