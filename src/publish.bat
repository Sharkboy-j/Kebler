del publish\ /F /Q
dotnet clean Kebler.sln
dotnet test Kebler.sln
dotnet publish Kebler.sln -p:PublishProfile=Properties\PublishProfiles\Release86.pubxml

dotnet publish Kebler.sln -p:PublishProfile=Properties\PublishProfiles\Portable64.pubxml
move publish\Kebler.exe publish/KeblerPortable64.exe

dotnet publish Kebler.sln -p:PublishProfile=Properties\PublishProfiles\Portable86.pubxml
move publish\Kebler.exe publish/KeblerPortable86.exe
del publish\Kebler.pdb /F /Q
pause