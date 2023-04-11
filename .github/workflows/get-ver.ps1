# Verify valid semver, and provide it along with an AssemblyVersion-compatible string as env vars.

# Set to the value provided by github.ref
param([string]$ghRef)

$semVerRegex = "(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$"

if (!($ghRef -match $semVerRegex)) {
    Write-Host "Could not find valid semver within ref. string. Given: $ghRef"
    Exit 1
}

$verRes = "VER={0}.{1}.{2}" -f $matches.major, $matches.minor, $matches.patch
$semVerRes = "SEMVER=" + $ghRef.Substring(10)

echo $verRes >> $env:GITHUB_ENV
echo $semVerRes >> $env:GITHUB_ENV
Write-Host "Result: $verRes, $semVerRes"