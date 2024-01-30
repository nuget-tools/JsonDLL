#! /usr/bin/env bash
set -uvx
set -e
cwd=`pwd`
cd $cwd/GlobalLibrary
doxygen
rm -rf $cwd/../nuget-tools.github.io/GlobalLibrary
cp -rp build-doxygen/html $cwd/../nuget-tools.github.io/GlobalLibrary
#start $cwd/../nuget-tools.github.io/GlobalLibrary/index.html
start $cwd/../nuget-tools.github.io/GlobalLibrary/class_global_1_1_util.html
