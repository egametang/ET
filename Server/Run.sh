#!/bin/bash

xbuild ./Server.sln
cd Bin/Debug/
cmake ../..
make

pkill App.exe
mono App.exe --id=1 --appType=Manager