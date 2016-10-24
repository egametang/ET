#!/bin/bash

cd Bin/Debug/
pkill App.exe
mono --debug App.exe --id=1 --appType=Manager