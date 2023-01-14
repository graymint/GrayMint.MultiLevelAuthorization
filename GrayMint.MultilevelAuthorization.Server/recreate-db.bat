SET curdir=%~dp0
cd "%curdir%"

dotnet build
dotnet run /recreatedb

