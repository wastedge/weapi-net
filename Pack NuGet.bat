@echo off

pushd "%~dp0"

cd WastedgeApi
..\Libraries\NuGet.exe pack -Prop Configuration=Release WastedgeApi.csproj

pause

popd
