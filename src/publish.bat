del publish\ /F /Q
set -Ux DOTNET_CLI_TELEMETRY_OPTOUT 1
dotnet clean Kebler.sln
dotnet test Kebler.sln
dotnet publish Kebler.sln -p:PublishProfile=Properties\PublishProfiles\Release86.pubxml

dotnet publish Kebler.sln -p:PublishProfile=Properties\PublishProfiles\Portable64.pubxml
move publish\Kebler.exe publish/KeblerPortable64.exe

dotnet publish Kebler.sln -p:PublishProfile=Properties\PublishProfiles\Portable86.pubxml
move publish\Kebler.exe publish/KeblerPortable86.exe
dotnet publish Kebler.Update\Kebler.Update.csproj -p:PublishProfile=Properties\PublishProfiles\INSTALLER_PUB.pubxml
del publish\*.pdb /F /Q
del publish\RELEASE\*.pdb /F /Q
pause