################################################################################
# Copyright © 2024 EFS
################################################################################
# FI 20240423 Retrieve trace files from the last 7 days

################################################################################
# Get all Trace Files of Spheres® (Retrieve trace files from the last 7 days.)
################################################################################

Param([int] $nbDays=7)


# Prompt the user for confirmation
$confirmation = Read-Host "This script retrieves all last trace files of Spheres® from the last $nbDays days located in the current directory ($PSScriptRoot) and subdirectories. Are you sure you want to continue? (Y/N)"
$isOK =$true;

if ($confirmation -eq "Y" -or $confirmation -eq "y") {
    Write-Host "User confirmed. Continuing...";
} 
elseif ($confirmation -eq "N" -or $confirmation -eq "n") {
    Write-Host "User declined. Exiting..."
    $isOK =$false;
} 
else {
    Write-Host "Invalid input. Please enter Y or N.";
    $isOK =$false;
}

if ($isOK -eq $true) {
    
    $folderRoot = $PSScriptRoot;
    $isWeb =  Test-Path .\web.config -PathType Leaf
    
    
    
    # Specify the destination folder
    $folderDestination = "$folderRoot\SpheresGetTraceFilesResult";
    
    # Create the destination folder
    if (-not (Test-Path -Path $folderDestination)) {
        New-Item -ItemType Directory -Path $folderDestination | Out-Null;
    }

    # Remove all childs in  the destination folder
    Remove-Item -Path "$folderDestination\*" -Force ;


    # Retrieve trace files recursively
    $dateStart = (Get-Date).AddDays($(-1)*$nbDays);
    Write-Host "Getting all Spheres® trace files from $folderRoot";
    $trcFiles = Get-ChildItem -Path $folderRoot -Filter *trc*.txt -Recurse -Exclude "$folderRoot\SpheresGetTraceFilesResult" | Where-Object {  $_.Name.StartsWith('Trc') -or ( $_.Name.Contains('_Trc') -and $_.LastAccessTime -ge $dateStart) };
    
    if ($trcFiles.Length){
        # Loop through each file in the list and copy it to the destination folder
        foreach ($item in $trcFiles) {
            Copy-Item -Path $item.FullName -Destination $folderDestination;
        }

        # Retrieve trace files from folderDestination
        $trcFiles = Get-ChildItem -Path $folderDestination -File;

        $trcFileNames = @()
        # Loop through each file path and extract the filename
        foreach ($item in $trcFiles) {
            $trcFileNames += $item.FullName;
        }

        Write-Host "Compressing ...."; 
        if ($isWeb -eq $true)
         {
            $zipFileName = "$folderDestination\SpheresGetTraceFilesWebResult.zip";
         }
        else 
         {
            $zipFileName = "$folderDestination\SpheresGetTraceFilesServicesResult.zip";
         }
        # Compress the files into a zip archive
        Compress-Archive  -Path $trcFileNames -DestinationPath $zipFileName -Update;

        Write-Host "All files are compressed in $zipFileName"; 
        
        # Remove files  previously compressed
        Remove-Item -Path "$folderDestination\*trc*.txt" -Force; 
    }
    else {
        Write-Host "No Spheres® trace file found using folder $folderRoot."; 
    }
}

Write-Host "Script ended.";