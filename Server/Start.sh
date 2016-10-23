#!/bin/bash

cd Bin/Debug/
pkill App.exe
mono App.exe --id=1 --appType=Manager