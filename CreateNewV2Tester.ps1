[CmdletBinding()]
param(
    [string]$ProjectName,
    [switch]$SkipDependencyInstall
)

$ErrorActionPreference = 'Stop'
$solutionRoot = $PSScriptRoot
$solutionPath = Join-Path $solutionRoot 'PETEL_Tester.slnx'
$generatedProjectsRoot = Join-Path $solutionRoot 'LocalTesters'

function Confirm-Net472TargetingPack {
    $programFilesX86 = ${env:ProgramFiles(x86)}
    $referencePath = Join-Path $programFilesX86 'Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2'
    if (Test-Path $referencePath) {
        return
    }

    if ($SkipDependencyInstall) {
        throw '.NET Framework 4.7.2 targeting pack is not installed. Install it through Visual Studio Installer and run this script again.'
    }

    Write-Host '.NET Framework 4.7.2 targeting pack was not found.' -ForegroundColor Yellow
    $install = Read-Host 'Install the targeting pack through Visual Studio Installer now? (Y/N)'
    if ($install -notmatch '^[Yy]') {
        throw '.NET Framework 4.7.2 targeting pack is required to build a tester project.'
    }

    $installerPath = Join-Path $programFilesX86 'Microsoft Visual Studio\Installer\setup.exe'
    $vswherePath = Join-Path $programFilesX86 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path $installerPath) -or -not (Test-Path $vswherePath)) {
        throw 'Visual Studio Installer was not found. Install the .NET Framework 4.7.2 targeting pack from Visual Studio Installer, then run this script again.'
    }

    $installPath = & $vswherePath -latest -products * -property installationPath | Select-Object -First 1
    if ($null -eq $installPath -or [string]::IsNullOrWhiteSpace($installPath.Trim())) {
        throw 'No Visual Studio installation was found. Install Visual Studio with the .NET Framework 4.7.2 targeting pack, then run this script again.'
    }
    $installPath = $installPath.Trim()

    Write-Host 'Starting Visual Studio Installer. Administrator approval may be required.' -ForegroundColor Yellow
    $arguments = "modify --installPath `"$installPath`" --add Microsoft.Net.Component.4.7.2.TargetingPack --passive --norestart"
    Start-Process -FilePath $installerPath -ArgumentList $arguments -Wait

    if (-not (Test-Path $referencePath)) {
        throw 'The .NET Framework 4.7.2 targeting pack is still unavailable. Complete the Visual Studio Installer operation and run this script again.'
    }
}

function Write-TemplateFile {
    param(
        [string]$Path,
        [string]$Content
    )

    Set-Content -LiteralPath $Path -Value $Content -Encoding UTF8
}

if ([string]::IsNullOrWhiteSpace($ProjectName)) {
    $ProjectName = Read-Host 'New tester project name'
}

$ProjectName = $ProjectName.Trim()
if ($ProjectName -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
    throw 'Project name must start with a letter or underscore and contain only letters, numbers, or underscores.'
}

if (-not (Test-Path $solutionPath)) {
    throw "Solution file not found: $solutionPath"
}

$projectPath = Join-Path $generatedProjectsRoot $ProjectName
if (Test-Path $projectPath) {
    throw "A project folder already exists: $projectPath"
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'The .NET SDK was not found. Install Visual Studio or the .NET SDK, then run this script again.'
}

Confirm-Net472TargetingPack

New-Item -ItemType Directory -Path $projectPath | Out-Null

$projectFilePath = Join-Path $projectPath "$ProjectName.csproj"
Write-TemplateFile -Path $projectFilePath -Content @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net472</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>disable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\PETEL_V2_Core\PETEL_V2_Core.csproj" />
    <ProjectReference Include="..\..\PETEL_Runner_V2\PETEL_Runner_V2.csproj" ReferenceOutputAssembly="false" />
  </ItemGroup>

  <Target Name="CopyRunnerToTesterOutput" AfterTargets="Build">
    <ItemGroup>
      <RunnerOutput Include="..\..\PETEL_Runner_V2\bin\`$(Configuration)\net472\*.*" />
    </ItemGroup>
    <Copy SourceFiles="@(RunnerOutput)" DestinationFolder="`$(OutputPath)" SkipUnchangedFiles="true" />
  </Target>
</Project>
"@

Write-TemplateFile -Path (Join-Path $projectPath 'Program.cs') -Content @'
using System;
using System.Reflection;
using PETEL_VPL;

internal static class Program
{
    private const string TestCasesTypeName = "PETEL_VPL.TestCases";

    private static int Main(string[] args)
    {
        var testCasesType = Type.GetType(TestCasesTypeName)
            ?? throw new InvalidOperationException($"Test class '{TestCasesTypeName}' not found.");

        var createTesterMethod = testCasesType.GetMethod("CreateTester", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"CreateTester method not found in '{TestCasesTypeName}'.");

        var createTester = (Func<VPLTester>)Delegate.CreateDelegate(typeof(Func<VPLTester>), createTesterMethod);
        return MainTesterHost.Run(createTester, TestCasesTypeName);
    }
}
'@

Write-TemplateFile -Path (Join-Path $projectPath 'TeacherAnswer.cs') -Content @'
public static class TeacherAnswer
{
    public static int AddOne(int value)
    {
        return value + 1;
    }
}
'@

Write-TemplateFile -Path (Join-Path $projectPath 'StudentAnswer.cs') -Content @'
public static class StudentAnswer
{
    public static int AddOne(int value)
    {
        // Replace this example with the student solution you want to test.
        return value + 1;
    }
}
'@

Write-TemplateFile -Path (Join-Path $projectPath 'TestCases.cs') -Content @'
using PETEL_VPL;

namespace PETEL_VPL
{
    public static class TestCases
    {
        public static VPLTester CreateTester()
        {
            return new VPLTester(studentMethodName: "AddOne");
        }

        public static void Case_1_AddOne(VPLTester tester)
        {
            tester.TestMethod(
                testName: "Add one to 4",
                points: 10,
                parameters: new object[] { 4 }
            );
        }

        public static void Code_1_UsesReturn(VPLTester tester)
        {
            tester.TestCodeTokenExists(
                testName: "Solution uses return",
                points: 5,
                tokenText: "return",
                failureMessage: "Use return to send the result back."
            );
        }
    }
}
'@

Write-Host "Adding $ProjectName to PETEL_Tester.slnx..." -ForegroundColor Cyan
& dotnet sln $solutionPath add $projectFilePath
if ($LASTEXITCODE -ne 0) {
    throw 'The project was created but could not be added to the solution.'
}

Write-Host 'Restoring and building the new tester project...' -ForegroundColor Cyan
& dotnet build $projectFilePath
if ($LASTEXITCODE -ne 0) {
    throw 'The project was created but the initial build failed.'
}

Write-Host "Created runnable tester project: $projectPath" -ForegroundColor Green
Write-Host 'The LocalTesters folder is intentionally ignored by Git.' -ForegroundColor Green

$openSolution = Read-Host 'Open the solution in Visual Studio now? (Y/N)'
if ($openSolution -match '^[Yy]') {
    Start-Process $solutionPath
}
