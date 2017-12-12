#!/bin/bash

if [$1 = ""]
then
    echo "please input config file! for example:"
    echo "bash Run.sh Config/StartConfig/192.168.12.188.txt"
    exit 9
fi

dotnet msbuild ./Server/Server.sln
cd Bin
cmake ..
make

ps -ef | grep App.exe | awk '{print $2}' | xargs kill -9
dotnet App.dll --appId=1 --appType=Manager --config=../$1