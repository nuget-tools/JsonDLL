#! /usr/bin/env bash
set -uvx
set -e
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`
version="${ts}"

cd $cwd/JsonDLL
#sed -i -e "s/<Version>.*<\/Version>/<Version>${version}<\/Version>/g" JsonDLL.csproj
rm -rf obj bin
rm -rf *.nupkg
dotnet restore JsonDLL.sln -p:Configuration=Release -p:Platform="Any CPU"
#dotnet pack -o . -p:Configuration=Release -p:Platform="Any CPU" JsonDLL.sln
msbuild.exe JsonDLL.sln -p:Configuration=Release -p:Platform="Any CPU"

cd $cwd/JsonDLL/bin/Release/net462/x64/
cp -rp JsonDLL.dll ../JsonDLL-64bit.dll
cd $cwd/JsonDLL/bin/Release/net462/x86/
cp -rp JsonDLL.dll ../JsonDLL-32bit.dll

rm -rf ~/cmd/JsonDLL
mkdir -p ~/cmd/JsonDLL
#cp -rp $cwd/JsonDLL/bin/Release/net462/*.dll ~/cmd/JsonDLL/
cp -rp $cwd/JsonDLL/bin/Release/net462/JsonDLL-*bit.dll ~/cmd/JsonDLL/

cd $cwd
git add .
git commit -m"JsonDLL v$version"
git tag -a v$ts -mv$version
git push origin v$version
git push
git remote -v
