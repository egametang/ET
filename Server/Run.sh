#!/bin/bash

xbuild ./Server.sln
cd Bin/Debug/
cmake ../..
make

ps -ef | grep App.exe | awk '{print $2}' | xargs kill -9
mono --debug App.exe --id=1 --appType=Manager