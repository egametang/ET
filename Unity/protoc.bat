@echo off
protoc.exe --csharp_out="./Assets/Model/Module/Message/" --proto_path="../Proto/" OuterMessage.proto
protoc.exe --csharp_out="./Assets/Hotfix/Module/Message/" --proto_path="../Proto/" HotfixMessage.proto
echo finish... 
pause