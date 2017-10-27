##### 1.visual studio must use vs2017, other versions do not support VS2017, need to check the installation of the following contents:
.net desktop development
Using C++ desktop development, VC++ 2017 v141 toolset, XP support for C++
Visual studio tools for unity.Netcore2.0
##### must install unity 2017.1.0p5 2. unity to 2017.1.2, the other version is not supported
3. ##### start menu File->open project->open unity2017, select the Egametang/Unity folder, click to select the folder button.
4. click the Unity menu Assets->open ##### C# project vs compiler (compiler must start, right-click the VS solution, all the compiler)
5. ##### opened with vs2017 Egametang/Server/Server.sln compiler (must be compiled, right-click the VS solution, all the compiler)
##### 6. Unity->tools menu - > command line configuration, select the LocalAllServer.txt which is the start of a single App, if you want to start a group of multi App server, select 127.0.0.1.txt in the command line tool, click start, the specific configuration can they use the command-line utility to modify
##### 7. click Tools in the start, this will start the server (you can also use VS to start, convenient debugging step)
##### 8. running Unity, enter the account, then click on the login log connection Gate success, said OK!
## frame synchronization test
##### 1. Unity->tools menu - > package ->PC package, hit a PC package in the Release directory
##### 2. run Unity login into the hall into the scene
##### 3. run PC package login into the hall then there will be two people (overlap)
##### 4. click the right mouse button to move characters
# note:
The 15.4 version of VS2017 bug, Hotfix Assembly-CSharp.dll will prompt can not find, you need to Hotfix project Unity quoted with Unity.Plugin removed, directly referenced in the Unity\Library\ScriptAssemblies directory with Assembly-CSharp.dll Assembly-CSharp-firstpass.dll two DLL
The general reason for error is 1. not compiled. 2. Chinese catalogue. 3.vs does not install vs tools or is not the latest vs tools. 4. not installing.Netcore2.0