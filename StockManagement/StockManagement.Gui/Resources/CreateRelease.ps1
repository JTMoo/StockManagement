﻿# Usage inside Powershell cmdlet of StockManagement.Gui directory: .\Resources\CreateRelease.ps1 (Or with absolute path)
# Usage with additional param for tag e.g.: .\Resources\CreateRelease.ps1 pre-release
# This will create a tag with *version of Gui.dll*-pre-release
# It might be necessary to set the ExecutionPolicy of the current process to Bypass with the following code: Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process -Force
param (
    [string]$TagSuffix = ""
)

# Define relative paths
$projectPath = Join-Path -Path $PSScriptRoot -ChildPath "..\StockManagement.Gui.csproj"
$publishPath = Join-Path -Path $PSScriptRoot -ChildPath "..\..\publish"

# Publish the project
Write-Host "Publishing StockManagement.Gui.csproj..."
dotnet publish $projectPath --output $publishPath

# Check if the publish succeeded
if ($LASTEXITCODE -eq 0) {
    Write-Host "Publishing succeeded. Output is located in '$publishPath'."
} else {
    Write-Host "Publishing failed with exit code $LASTEXITCODE."
    exit $LASTEXITCODE
}

# Define the relative path to the DLL file based on the script's location
$filePath = Join-Path -Path $PSScriptRoot -ChildPath "..\..\publish\StockManagement.Gui.dll"

# Check if the file exists
if (Test-Path $filePath) {
    Write-Host "File found: $filePath"

    # Retrieve the file version
    $fileVersion = (Get-Item $filePath).VersionInfo.FileVersion

    if ($fileVersion) { 
        Write-Host "File version: $fileVersion"
        
        # Append the optional suffix to the tag
        $tagVersion = $fileVersion
        if ($TagSuffix -ne "") {
            $tagVersion = "$fileVersion-$TagSuffix"
        }

        # Delete All local tags
        Write-Host "Executing: git tag -d $(git tag -l)"
        & git tag -d $(git tag -l)
        
        # Get all remote tags
        Write-Host "Executing: git fetch"
        & git fetch
        
        # Create a new tag with the version of the *gui.dll
        Write-Host "Executing: git tag $tagVersion"
        & git tag $tagVersion

        # Push all local tags to remote
        Write-Host "Executing: git push origin --tags"
        & git push origin --tags

        Write-Host "Git commands executed successfully."
    } else {
        Write-Error "Unable to retrieve file version."
    }
} else {
    Write-Error "File not found: $relativePath"
}