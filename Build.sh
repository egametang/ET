#!/bin/bash
xbuild ./Server/Server.sln
cd Bin
cmake ../
make