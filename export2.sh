#! /usr/bin/env bash
set -uvx
set -e
rm -rf tmp.master*
wget --no-check-certificate https://github.com/mbdavid/LiteDB/archive/master.zip -O tmp.master.zip
7z x -o"tmp.master" tmp.master.zip
cp -rp tmp.master/LiteDB-master/LiteDB JsonDLL/LiteDB
find JsonDLL/LiteDB -name "obj" -exec rm -rf {} +
find JsonDLL/LiteDB -name "*.cs" -exec sed -i -e "s/LiteDB/JsonDLL.LiteDB/g" {} +
rm -rf tmp.master*
