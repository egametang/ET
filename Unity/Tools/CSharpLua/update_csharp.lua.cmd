set csharplua=D:\Project\Person\CSharp.lua
set launcher=%csharplua%\CSharp.lua.Launcher\bin\Debug\netcoreapp3.0
set coresystem=%csharplua%\CSharp.lua\CoreSystem.Lua
set localcoresystem=..\..\Assets\Lua\CoreSystemLua

xcopy %launcher% CSharp.lua /y 
del /s /q CSharp.lua\*.dev.json
del /s /q %localcoresystem%\*.lua
xcopy %coresystem% %localcoresystem% /s /y

del /s /q %localcoresystem%\All.lua
rd /s /q %localcoresystem%\CoreSystem\Numerics
rd /s /q %localcoresystem%\CoreSystem\Globalization
xcopy codes\All.lua %localcoresystem% /s /y
