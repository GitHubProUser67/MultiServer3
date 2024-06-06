param (
    [string]$jsonFilePath,
    [string]$targetFramework
)

# Extract the version number using a regular expression
$version = if ($targetFramework -match 'net(\d+\.\d+)') { $matches[1] } else { $null }

if (-not $version) {
    Write-Error "Unable to extract version number from target framework: $targetFramework"
    exit 1
}

# Construct the property path dynamically
$propertyPath = ".NETCoreApp,Version=v$version"

# Define an array of property names to remove
$propertiesToRemove = @(
    "CastleLibrary/1.0.0",
    "CompressionLibrary/1.0.0",
    "CustomLogger/1.0.0",
    "CyberBackendLibrary/1.0.0",
    "EndianTools/1.0.0",
    "HorizonService/1.0.0"
)

# Read the JSON file
$jsonContent = Get-Content $jsonFilePath -Raw | ConvertFrom-Json

# Remove the specified properties
foreach ($property in $propertiesToRemove) {
    $jsonContent.targets.$propertyPath.PSObject.Properties.Remove($property)
}

# Save the modified JSON content back to the file
$jsonContent | ConvertTo-Json -Depth 32 | Set-Content $jsonFilePath