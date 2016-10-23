#!/bin/bash
xbuild ./Server.sln
cd Bin/Debug/
cmake ../..
make