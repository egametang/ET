#!/bin/bash
dotnet msbuild ./Server/Server.sln
cd Bin
cmake ../
make