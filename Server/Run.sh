#!/bin/bash

xbuild ./Server.sln
cd Bin/Debug/
cmake ../..
make

pkill App.exe
mono --debug App.exe --id=1 --appType=Manager