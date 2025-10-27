# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/FFT_enhanced.UI.unique_icons/*" -Force -Recurse
dotnet publish "./FFT_enhanced.UI.unique_icons.csproj" -c Release -o "$env:RELOADEDIIMODS/FFT_enhanced.UI.unique_icons" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location