#!/bin/bash
dotnet msbuild ./Server/Server.sln
cd netcoreapp2.0
cmake ../
make