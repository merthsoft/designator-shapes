param(
    [string]$RimWorldDir = "${env:ProgramFiles(x86)}\Steam\steamapps\common\RimWorld",
    [string]$HarmonyPath = "${env:ProgramFiles(x86)}\Steam\steamapps\workshop\content\294100\2009463077\1.7\Assemblies\0Harmony.dll"
)
$ErrorActionPreference = 'Stop'
if (!(Test-Path "$RimWorldDir\Assembly-CSharp.dll")) { throw 'Set -RimWorldDir to a RimWorld 1.7 installation.' }
dotnet build (Join-Path $PSScriptRoot 'Source\Merthsoft.DesignatorShapes.csproj') -c Release "-p:RimWorldDir=$RimWorldDir" "-p:HarmonyPath=$HarmonyPath"
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }