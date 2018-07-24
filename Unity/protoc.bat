@echo off
protoc.exe --csharp_out="./Assets/Scripts/Module/Message/" --proto_path="../Proto/" OuterMessage.proto
protoc.exe --csharp_out="./Hotfix/Module/Message/" --proto_path="../Proto/" HotfixMessage.proto
protoc.exe --csharp_out="./Assets/Scripts/Module/FrameSync" --proto_path="../Proto/" FrameMessage.proto
echo finish... 
pause