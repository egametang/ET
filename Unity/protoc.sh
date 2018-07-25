#!/bin/bash

protoc --csharp_out="./Assets/Scripts/Module/Message/" --proto_path="../Proto/" OuterMessage.proto
protoc --csharp_out="./Hotfix/Module/Message/" --proto_path="../Proto/" HotfixMessage.proto
echo finish

