#! /usr/bin/env bash
set -uvx
set -e
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`
version="${ts}"

cd $cwd/JsonDLL
sed -i -e "s/<Version>.*<\/Version>/<Version>${version}<\/Version>/g" JsonDLL.csproj
rm -rf obj bin
rm -rf *.nupkg
dotnet pack -o . -p:Configuration=Release -p:Platform="Any CPU"

#exit 0

cd $cwd
git add .
git commit -m"JsonDLL v$version"
git tag -a v$ts -mv$version
git push origin v$version
git push
git remote -v
