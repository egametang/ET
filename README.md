# [中文](https://github.com/egametang/Egametang/blob/master/README-ZH.md) 

# __Discussion QQ group : 474643097__  
# email: egametang@qq.com  

# [ET Forum](https://et-framework.cn)  

# [ET Store](https://github.com/egametang/ET/tree/master/Store)  

# [ET6.0 video tutorial online](https://edu.uwa4d.com/course-intro/1/375)   

# [Run Guide](https://github.com/egametang/ET/blob/master/Book/1.1RunGuide.md)  

# Major Notes.
1. Hotfix with HotfixView is pure logic, do not have any fields in the class, otherwise the hotfix will be lost  
2. ETTask and either call Coroutine or await, open the error list window in VS, do not use these two will be reported as a problem, although neither await nor Coroutine, if you can compile through, but will lose the exception, very dangerous  
3. do not use any virtual functions, use logical distribution instead  
4. Please do not use any inheritance, except for Entity inheritance, use components instead.  


# ET6 has huge changes compared with ET5, and it can be said that Phoenix has become Yifei. 6.0 has the following amazing features
1. client-side logic code full hot update (based on ILRuntime) on ios, no part that can not be changed  
2. client and server can be hot reload, development without restarting the client and server can modify the logic code, development is extremely convenient  
3. robot framework, ET6 client-side logic and performance separation, robot program directly share the use of client-side logic layer code to do pressure testing, only a very small amount of code to make the robot, easy to pressure test the server  
4. test case framework, using the client's logic layer code to write unit tests, each unit test is a complete game environment, without all kinds of nasty mock  
5. AI framework, more convenient than the behavior tree, write AI than writing UI is still simple  
6. the new server-side architecture, extremely beautiful  
7. intranet and extranet kcp network, strong performance, with soft routing module, can prevent all kinds of network attacks  

# ET development of commercial mmo project thousand ancient wind flow successfully online, 64 core 128G memory single service single physical machine 1.5W online (the actual online planning for ecological restrictions for a single service 6000 people online at the same time, 6000 people then cpu consumption is about 30%). In order to stack line number normal, online run is Debug version, if you use Release version to open optimization, performance can also double, to reach a single physical machine 3W online! On-line for 5 months is very stable. Thousand ancient wind flow using the ET framework developed from scratch, it took two years, this development speed can be said that no one can be its right. The successful launch of Thousand Ancient Winds proves that ET has the ability to develop any large game, development speed, development efficiency are breathtaking! The client server technology used in Thousand Ancient Winds and Currents: 1.  
1. dynamic copies and sub-lines, on-demand allocation, recycling after use  
2. split line merge line, split line less number of people will merge multiple lines. Combined line function basically other mmo games rarely see  
3. seamless client-server scene switching, that is, seamless world technology  
4. cross-services copies, cross-services battlefield  
5. front and back-end integration, the use of client-side code to develop server pressure testing robot, four 24-core machine easily simulate 1W people to do the task  
6. a variety of ai design, the use of ET's new development of ai framework, so that ai development is as simple as writing ui  
7. test case framework, most of the important system, the thousand ancient wind flow are written test cases, different from the test cases on the market, each thousand ancient wind flow test cases are a complete game environment, for the protocol level, do not need to engage in a variety of interfaces to mock. write up very fast.  
8. aoi implementation of the nine-gong grid, dynamic adjustment of the players seen to reduce the server load  
9. anti-attack, a thousand ancient wind flow developed a soft route function, even if the attack can only attack to the soft route, once attacked, the player client found a few seconds no response, you can dynamically switch to other soft routes, the user almost no perception. The whole process of client network connection does not open, no loss of data.  
10. there are many, many more, here will not be verbose  


# ET's introduction.
ET is an open source game client (based on unity3d) server-side dual-end framework , the server side is developed using C# .net core distributed game server , which is characterized by high development efficiency , strong performance , dual-end shared logic code , client-side server hot more mechanism is perfect , while supporting reliable udp tcp websocket protocol , support for server-side 3D recast pathfinding, etc.  

# ET features.
### 1. available VS single-step debugging distributed server, N to 1  
Generally speaking, distributed server-side has to start many processes, and once there are more processes, single-step debugging becomes very difficult, resulting in server-side development basically relying on playing logs to find problems. ET framework uses a component design similar to Watchtower, all server-side content is broken down into individual components, which are mounted according to the type of server they need when starting. This is somewhat similar to computers, which are modularly broken down into memory, CPU, motherboard and other parts, with different parts can be assembled into a different computer, for example, a home desktop needs memory, CPU, motherboard, graphics card, monitor, hard disk. While servers for companies do not need monitors and graphics cards, computers in Internet cafes may not need hard disks, etc. Because of this design, the ET framework can hang all the server components on one server process, then this server process has all the functions of a server, and one process can be used as a whole group of distributed servers. This is also similar to the computer, the desktop has all the computer components, then it can also be completely used as a company server, but also as an Internet cafe computer.  
### 2. Distributed server that can be split into functions at will, 1 to N  
Distributed server to develop a variety of types of server processes, such as login server, gate server, battle server, chat server friend server and so on a large number of various servers, the traditional development method needs to know in advance the current function to be placed on which server, when more and more functions ET framework in the normal development time simply do not need to care about the current development of the function will be placed on what server, only use a process for development, functional development into the form of components. Release time to use a multi-process configuration can be released into the form of multi-process, is not very convenient it? Split the server however you want. You only need to modify a very small amount of code to split it. Different servers hooked up to different components on the line!  
### 3. Cross-platform distributed server  
ET framework using C# to do the server , C# is now completely cross-platform , install .netcore on linux , you can , without modifying any code , you can run . performance, now .netcore performance is very strong, than lua, python, js what is much faster. It is much faster than lua, python, js or anything else. It is not a problem to do game server. ET framework also provides a key synchronization tool , open unity->tools->rsync synchronization , you can synchronize the code to linux  
```bash
. /Run.sh Config/StartConfig/192.168.12.188.txt 
```
That's all it takes to compile and start the server.  
### 4. Provide concurrent support  
C# inherently supports asynchronous to synchronous syntax async and await, much more powerful than lua, python's concurrency, new versions of python and javascript language even copied C#'s concurrency syntax. Distributed server-side remote calls between a large number of servers, without the support of asynchronous syntax, development will be very troublesome. So java does not have asynchronous syntax, do a single service is okay, not suitable for large distributed game server. For example.  

```c#
// send C2R_Ping and wait for response message R2C_Ping
R2C_Ping pong = await session.Call(new C2R_Ping()) as R2C_Ping;
Log.Debug("Received R2C_Ping");

// query mongodb for a Player with id 1 and wait for the return
Player player = await Game.Scene.GetComponent<DBProxyComponent>().Query<Player>(1);
Log.Debug($"Printing player name: {player.Name}")
```
As you can see, with async await, all the asynchronous operations between servers become very coherent and don't have to be split into multiple pieces of logic. Greatly simplifies distributed server development  
### 5. provide erlang-like actor message mechanism  
erlang language is a major advantage is the location of transparent messaging mechanism , the user does not care about the object in which the process , get the id can send messages to the object . ET framework also provides an actor message mechanism , the entity object only needs to hang on the MailBoxComponent component , the entity object becomes an Actor , any server only needs to know the Any server only needs to know the id of the entity object can send messages to it, do not care which server the entity object in which physical machine. Its implementation principle is also very simple, ET framework provides a location server, all mounted MailBoxComponent entity objects will be registered to the location server with their id and location, other servers to send messages to this entity object if you do not know the location of the entity object, will first go to the location server query, query the location and then send.
### 6. Provide server non-stop dynamic update logic function  
hot is an indispensable feature of the game server , ET framework using the component design , you can make the Watchtower design , the component only members , no methods , all the methods made to extend the method into the hot more dll , reload the dll at runtime to hot more all logic .
### 7. Client use C# hot update, hot update one key switch  
You can use csharp.lua or ILRuntime to do client-side hot update with a little modification. No need to use shit lua anymore, client can realize all logic hot update, including protocol, config, ui and so on.  
### 8. Client-side hot reload  
Development without restarting the client can modify the client logic code, development is extremely convenient  

### 9. Client-side server in the same language and share code  
Download the ET framework , open the server-side project , you can see that the server side references the client side of a lot of code , through the reference to the client code to achieve a dual-side shared code . For example, the network messages between the client-side and the server-side completely share a file, add a message only need to modify once.  
### 10. KCP ENET TCP Websocket protocol seamless switching  
ET framework not only supports TCP, but also supports reliable UDP protocols (ENET and KCP), ENet is the network library used by League of Legends, which is characterized by fast and very good performance in the case of network packet loss, which we have tested TCP in the case of packet loss of 5%, the moba game on the card can not be, but using ENet, packet loss of 20% will still not feel card. Very powerful. Framework also supports the use of KCP protocol, KCP is also a reliable UDP protocol, said to be better than ENET performance, please note that the use of kcp, you need to add their own heartbeat mechanism, otherwise 20 seconds did not receive the packet, the server will be disconnected. The protocol can be switched seamlessly.  
### 11. 3D Recast pathfinding function  
Unity can export scene data to the server side to do recast pathfinding. Very convenient to do MMO, demo demonstrates the server-side 3d pathfinding function  
### 12. server-side support for repl, you can also dynamically execute a new code  
This can print out any data in the process, greatly simplifying the difficulty of finding problems on the server side, open the repl method, enter repl directly in the console to enter repl mode  
### 13. Provide client-side bot framework support  
A few lines of code to create a robot to log into the game. Robot pressure testing is a breeze, the robot is exactly the same as the normal player, use the robot to do a good pressure test before going online, greatly reducing the chance of crashing online   
### 14. AI framework  
ET's AI framework makes AI writing even easier than UI.     
### 15. Test case framework  
Unlike the test cases on the market, ET's test cases are a complete game environment, for the protocol level, no need to engage in a variety of interfaces to mock. write up very quickly    


### 16. There are many, many more features that I won't go into detail about  
a. and its convenient to check the CPU occupation and memory leak check, vs comes with analysis tools, no longer have to worry about performance and memory leak check  
b. The use of NLog library, playing log and its convenience, the usual development, you can play all the server log to a file, no longer have to search for logs one file at a time  
c.Unify the use of Mongodb bson serialization, messages and configuration files are all bson or json, and later use mongodb to do the database, no longer need to do the format conversion.  
d. Provide a synchronization tool  

ET framework is a powerful and flexible distributed server-side architecture that can fully meet the needs of most large games. Use this framework, the client developer can complete their own dual-ended development, saving a lot of manpower and resources, saving a lot of communication time.  
  
Related websites :  
[ET Forum](https://et-framework.cn)  

Groupies share.  
[Behavior tree and fgui branching (developed and maintained by Duke Chiang)](https://github.com/DukeChiang/ET.git)   
[ET Learning Notes Series (written by Smoke and Rain Daze Half World Gothic)](https://www.lfzxb.top/)   
[ET Learning Notes Series (written by Saki Yoshi)](https://acgmart.com/unity/)   
[Framework server-side operation process](http://www.cnblogs.com/fancybit/p/et1.html)  
[ET startup configuration](http://www.cnblogs.com/fancybit/p/et2.html)  
[framework demo introduction](http://www.jianshu.com/p/f2ea0d26c7c1)  
[linux deployment](http://gad.qq.com/article/detail/35973)  
[linux deployment, mongo installation, resource service build](http://www.tinkingli.com/?p=25)  
[ET Framework heartbeat package component development](http://www.tinkingli.com/?p=111)  
[ET Framework Actor use and insights](http://www.tinkingli.com/?p=117)  
[ET Framework and UGUI based on a simple UI framework implementation (gradually write)](http://www.tinkingli.com/?p=124)  
[ET framework notes (laugh at the world to write)](http://www.tinkingli.com/?p=76)  
[ET framework how to develop with MAC](http://www.tinkingli.com/?p=147)  
[ET's dynamic addition of events and trigger components](http://www.tinkingli.com/?p=145)  

Commercial projects :  
1. [A Thousand Times the Wind](https://www.qiangu.com/)  
2. [Magic Dots 2](https://www.taptap.com/app/227804)  
3. [Raise not big](https://www.taptap.com/app/71064)  
4. [Tian Tian Tian Hide-and-Seek 2](ios2019 Spring Festival Download Ranking 19)  
5. [Niuhu chess](https://gitee.com/ECPS_admin/PlanB)  
6. [Five Star Mahjong](https://github.com/wufanjoin/fivestar)  

Groupies demos.  
1. [Landlord (client-side server)](https://github.com/Viagi/LandlordsCore)  
2. [Backpack system](https://gitee.com/ECPS_admin/planc)  
3. [ET mini-game collection](https://github.com/Acgmart/ET-MultiplyDemos)  



Video tutorials.  
[Alphabet Brother ET 6.0 Tutorial](https://edu.uwa4d.com/course-intro/1/375)   
[Meatloaf Teacher Lecture](http://www.taikr.com/my/course/972)  
[Jianming Guan Lecture](https://edu.manew.com/course/796)  
[ET Newbie Tutorial - First Look Main Lecture](https://pan.baidu.com/s/1a5-j2R5QctZpC9n3sMC9QQ) Password: ru1j  
[ET Tutorial for Beginners New Version-Hatsumi Main Lecture](https://www.bilibili.com/video/av33280463/?redirectFrom=h5)  
[ET Running Guide on Mac-L Main Lecture](https://pan.baidu.com/s/1VUQbdd1Yio7ULFXwAv7X7A) Password: l3e3  
[ET Framework Tutorial Series - Smoky Rain - Version 6.0](https://space.bilibili.com/33595745/favlist?fid=759596845&ftype=create)  

net core game resources to share  
[various dotnet core project collection](https://github.com/thangchung/awesome-dotnet-core)  

__discussion QQ group : 474643097__


# Paypal donation  
! [Use Alipay to donate to this project](https://github.com/egametang/ET/blob/master/Book/donate.png)

# Links  
[Box2DSharp](https://github.com/Zonciu/Box2DSharp) box2d's C# port version, very strong performance  
[xasset](https://github.com/xasset/xasset) Dedicated to providing a lean and robust resource management environment for Unity projects  
[QFramework](https://github.com/liangxiegame/QFramework) Your first K.I.S.S Unity3d Framework  
[ET UI Framework](https://github.com/zzjfengqing/ET-EUI) alphabetical implementation of the UI framework, ET style, a variety of event distribution  
[ETCsharpToXLua](https://github.com/zzjfengqing/ETCsharpToXLua) Alphabet Brother uses csharp.lua to implement the ET client hot update  
[et-6-with-ilruntime](https://www.lfzxb.top/et-6-with-ilruntime) Smokey uses ILRuntime to implement the ET client hot update  
[Luban](https://github.com/focus-creative-games/luban) A game configuration solution for medium and large projects  

