dotnet clean Sobyanina.csproj
rd /s /q bin 
rd /s /q obj
dotnet nuget locals all --clear
dotnet restore Sobyanina.csproj