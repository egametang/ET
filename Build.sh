#!/bin/bash
cd Server
dotnet restore
cd ../
dotnet msbuild ./Server/Server.sln
cd Bin
cmake ../
make
