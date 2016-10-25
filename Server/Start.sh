#!/bin/bash

cd Bin/Debug/
ps -ef | grep App.exe | awk '{print $2}' | xargs kill -9
mono --debug App.exe --appId=1 --appType=Manager