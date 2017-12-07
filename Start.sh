#!/bin/bash

if [$1 = ""]
then
    echo "please input config file! for example:"
    echo "bash Start.sh Config/StartConfig/192.168.12.188.txt"
    exit 9
fi

cd Bin
ps -ef | grep App.exe | awk '{print $2}' | xargs kill -9
dotnet App.dll --appId=1 --appType=Manager --config=../$1