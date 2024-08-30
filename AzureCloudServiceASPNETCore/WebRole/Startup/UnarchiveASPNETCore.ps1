function Unarchive-ASPNETCorePackage {
    param (
        [Parameter(Mandatory=$true)]
        [string]
        $PhysicalPath
    )

    Write-Output "Unarchive-ASPNETCorePackage: $($PhysicalPath)"

    if (Test-Path $physicalPath) {

        Set-Location "$($physicalPath)\WebRoleCore"

        # Expand Archive
        Get-ChildItem -Exclude @('WebRoleCore.zip') | Remove-Item -Recurse -Force
        $path = Get-Location
        Write-Output "Unarchiving WebRoleCore at $path"
        Expand-Archive -Path ".\WebRoleCore.zip" -Force
    }
}

if (Test-Path "D:\sitesroot\0\" ) {
    Unarchive-ASPNETCorePackage -PhysicalPath "D:\sitesroot\0\"
} elseif (Test-Path "E:\sitesroot\0\") {
    Unarchive-ASPNETCorePackage -PhysicalPath "E:\sitesroot\0\"
} elseif (Test-Path "F:\sitesroot\0\") {
    Unarchive-ASPNETCorePackage -PhysicalPath "F:\sitesroot\0\"
} else {
    throw "No IIS site path found."
}