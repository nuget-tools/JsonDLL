#! /usr/bin/env bash
set -uvx
set -e
#svn export https://github.com/JamesNK/Newtonsoft.Json/trunk/Src/Newtonsoft.Json JavaCommons.Json
rm -rf tmp.master* JsonDLL/JsonDLL.Json
wget --no-check-certificate https://github.com/JamesNK/Newtonsoft.Json/archive/master.zip -O tmp.master.zip
7z x -o"tmp.master" tmp.master.zip
cp -rp tmp.master/Newtonsoft.Json-master/src/Newtonsoft.Json JsonDLL/JsonDLL.Json
rm -rf JsonDLL/JsonDLL.Json/Properties
find JsonDLL/JsonDLL.Json -name "obj" -exec rm -rf {} +
find JsonDLL/JsonDLL.Json -name "*.cs" -exec sed -i -e "s/Newtonsoft[.]Json/JsonDLL.Json/g" {} +
rm -rf tmp.master*
