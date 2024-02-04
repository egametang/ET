# ET English Documentation
### __Discussion QQ Group: 474643097__

## Language
You can click below to switch to other languages:  
    - [ET Chinese Document](README.md)

## Table of Contents
You can view the content by clicking the sections below:
1. [Version Notification and Features](#section1)
2. [About ET](#section2)

<h2 id="section1">ET8.1 Released</h2>
The 8.1 version does not have significant changes compared to version 8.0, with the code structure remaining unchanged. The primary change is the modification of the compilation method, which allows compilation in Unity using F6 or in the IDE. Code hot reloading during runtime can be done by first compiling with F6, then pressing F7.

## ET8 Released! Diao Chan
1. Multi-threaded and multi-process architecture, more flexible and powerful structure, detailed content on multi-threading design available in the multi-threading design course.
2. Abstracted the concept of Fibers, similar to Erlang processes, allowing for the effortless creation of multiple fibers to leverage multi-core processors while still providing a single-threaded development experience.
3. Fiber scheduling: Main thread, thread pool, one thread per fiber, 3 types of scheduling.
4. Actor messaging mechanism for communication between Fibers.
5. For Entity aspect, domain changed to IScene. As long as the IScene interface is implemented, an Entity becomes a domain, making the definition of domain more flexible.
6. Implemented frame synchronization with predictive rollback. For detailed understanding, please refer to the frame synchronization course.
7. Switched protobuf to memorypack, achieving GC-free networking.
8. Pure C# version of the kcp library, very high performance, contributed by sj.
9. Hot-update dll changed to IDE compilation for convenience.
10. Sj utilized source generator to implement automatic code template feature. Currently, it can automatically generate the System class. Developers only need to define the static Awake Update method, which is especially convenient.
11. Sj developed an analyzer that implements EntitySystemOf, generating corresponding system methods based on the entity interface with one click.
12. The client utilizes fiber to implement an independent network thread (already implemented in the demo). Even the logic and representation can use independent fibers, making better use of multi-core processors.
13. Frame synchronization demo directly uses fibers to create rooms, more convenient.
14. Pure C# version of pathfinding dotrecast, making ET fully C# with no cpp code.
15. KCP and soft router support both TCP and WebSocket at the bottom layer. When UDP cannot be connected, it can switch to TCP WebSocket and supports dynamic switching during runtime without player disconnection!
16. Integrated sj's unmanaged container library, explosive performance.

## 17 Reasons to Use ET
1. Multi-process, multi-threaded Actor architecture. Both client and server can easily create fibers (fiber) to utilize multi-core processors. For example, the client network is one fiber, pathfinding is another, frame synchronization logic layer is another, and the presentation layer is yet another.
2. Async await coroutine for synchronous code writing, avoiding callback hell.
3. 0GC consumption, powerful MemoryPack serialization, excellent network layer performance.
4. Support for kcp, very rapid network response, and flash disconnection of wifi 4g will not cause disconnection, essential for competitive games.
5. The underlying layer of kcp can use TCP UDP WebSocket protocols. When UDP cannot be connected, it can switch to TCP WebSocket and supports dynamic switching during runtime without player disconnection!
6. Soft routing anti-attack design. Cheap hosts can fend off hacker attacks, much cheaper than high-defense ones, and users won't be disconnected.
7. Dual-end C# development, shared code between front and back ends. C# itself has very strong performance, second only to CPP. There's no need to learn various miscellaneous languages. Many independent game developers can develop MMORPG games with ET alone.
8. Powerful compiler analyzer helps everyone write correct ET-style code.
9. Client hybridclr hot update support.
10. Both client and server support runtime hot reloading. There's no need to close the process to modify the code, greatly improving development and operational efficiency.
11. Complete demo with state synchronization and predictive rollback frame synchronization.
12. Perfect robot development mechanism. Robots directly share the client logic code, reducing 95% of robot development workload, making AI robot integration very easy. Large-scale robot stress testing, easy and effortless.
13. Powerful AI development mechanism, easier than behavior trees.
14. Strong unit testing development mechanism. Each unit test is the entire game environment, no need for mock isolation, making development very easy.
15. Beautiful program structure, complete separation of data and methods.
16. All-in-one development experience. Only need to start Unity during development, but can separately release the server side, and can cross Windows and Linux platforms.
17. Client-server data development period is fully visualized. Enable the ENABLE_VIEW macro to see all Entity objects and field content of the client and server in the Unity Hierarchy panel.
18. WebGL and WeChat mini-game support. There's an ET8 version of WebGL. The development experience is completely consistent with ET8, seamlessly docking with ET8's server.

## Panda's Three Courses
For those interested, please add QQ 80081771:
1. [Online Game Architecture Design](https://www.bilibili.com/video/BV1h84y1G7aH/?spm_id_from=333.999.0.0&vd_source=e55f8234b8f8039504cbf131082c93dd) Based on ET7.2, a total of 27 episodes, explaining the framework design details of ET7.2.
2. [Frame Synchronization Design](https://www.bilibili.com/video/BV1tX4y1C7pM/?share_source=copy_web&vd_source=001b901865c99550d1b2a8cd663695d4) Based on ET8, a total of 12 episodes, explaining the design of predictive rollback frame synchronization.
3. [Multi-threading Architecture Design](https://www.bilibili.com/video/BV1Ah4y1f7QT/?spm_id_from=333.999.0.0&vd_source=e55f8234b8f8039504cbf131082c93dd) Based on ET8, a total of 11 episodes, explaining the multi-threading design of ET8.
4. WebGL Mini-game Framework, based on ET8, complete network, configuration, hot update, etc., the same usage as ET8.

## Related Links

#### [ET Forum](https://et-framework.cn)  

#### [ET Store](./Store)  

#### [ET Video Tutorials](https://community.uwa4d.com/search?keyword=ET&scope=1)   

#### [Operating Guide](./Book/1.1运行指南.md)  

#### [Analyzer Description](https://www.yuque.com/u28961999/yms0nt/)

## Benchmark
100W Ping Pong averages around 4 seconds, with an average of 200,000 messages sent and received per second. This network performance far exceeds the needs of the main thread. The testing method is as follows:
`Unity Menu->ServerTools select Benchmark`, `Start Watcher`. Then in the `Logs` directory, open the `Debug` log and wait a while for all connections to complete, and you will see the logs below.
<details>
<summary>Log Details</summary>
2022-12-02 22:19:48.9837 (C2G_BenchmarkHandler.cs:13) benchmark count: 1000001  
2022-12-02 22:19:53.4621 (C2G_BenchmarkHandler.cs:13) benchmark count: 2000001  
2022-12-02 22:19:57.0416 (C2G_BenchmarkHandler.cs:13) benchmark count: 3000001  
2022-12-02 22:20:00.6186 (C2G_BenchmarkHandler.cs:13) benchmark count: 4000001  
2022-12-02 22:20:04.1384 (C2G_BenchmarkHandler.cs:13) benchmark count: 5000001  
2022-12-02 22:20:08.2236 (C2G_BenchmarkHandler.cs:13) benchmark count: 6000001  
2022-12-02 22:20:12.2842 (C2G_BenchmarkHandler.cs:13) benchmark count: 7000001  
2022-12-02 22:20:15.8544 (C2G_BenchmarkHandler.cs:13) benchmark count: 8000001  
2022-12-02 22:20:19.4085 (C2G_BenchmarkHandler.cs:13) benchmark count: 9000001  
2022-12-02 22:20:24.2969 (C2G_BenchmarkHandler.cs:13) benchmark count: 10000001  
2022-12-02 22:20:41.1448 (C2G_BenchmarkHandler.cs:13) benchmark count: 11000001  
2022-12-02 22:20:44.7174 (C2G_BenchmarkHandler.cs:13) benchmark count: 12000001  
2022-12-02 22:20:48.3188 (C2G_BenchmarkHandler.cs:13) benchmark count: 13000001  
2022-12-02 22:20:51.7793 (C2G_BenchmarkHandler.cs:13) benchmark count: 14000001  
2022-12-02 22:20:55.3379 (C2G_BenchmarkHandler.cs:13) benchmark count: 15000001  
2022-12-02 22:20:58.8810 (C2G_BenchmarkHandler.cs:13) benchmark count: 16000001  
2022-12-02 22:21:02.5156 (C2G_BenchmarkHandler.cs:13) benchmark count: 17000001  
2022-12-02 22:21:06.0132 (C2G_BenchmarkHandler.cs:13) benchmark count: 18000001  
2022-12-02 22:21:09.5320 (C2G_BenchmarkHandler.cs:13) benchmark count: 19000001  
</details>

## ET7 Released! 18-year-old Yifei
1. Adjusted structure, merged the robot engineering with the server for easier use. One process can both serve as a server and create robots, truly ALL IN ONE! -- Implemented.
2. Merged client and server. Server code is all placed in the client. The client can contain a server, making development super convenient. When releasing the server, you can choose to release it as Dotnet or UnityServer, ultimate All IN ONE. -- Implemented.
3. Entity visualization. All entities of the client and server are visualized. Enable the ENABLE_CODES macro, run the game, view the Hierarchy panel, expand Init/Global/Scene(Process) to see. -- Implemented.
4. Due to all code being in Unity, developing ET plugins becomes very easy. Simply use Unity's import and export. -- Implemented.
5. Added soft routing to prevent various network attacks without affecting normal players, a must-have for online games! -- Implemented.
6. Added DomainSceneType to various events and network message subscriptions, more precise, less prone to errors. -- Implemented.
7. Sj brother added various analyzers. Analyzers ensure that the code written must comply with ET standards, otherwise, it will not compile! (This was also added to ET6). -- Implemented.
8. ET7 integrated the huatuo hot update library. Note! (Do not confuse client-side hot updates with server-side hot updates, server-side hot updates have always been part of ET).
9. The network was changed to an independent thread, serialization and deserialization are handled in the network thread, greatly reducing the load on the main thread. And the network layer code was reorganized, making it more elegant.
10. Integrated Unity.Mathematic math library. The logic layer of the client and server both use this set of math libraries, thus the server and client are completely unified.
11. ENABLE_CODES mode split into 4 assemblies to solve the problem of the analyzer being ineffective.
12. Game's Singleton management added ISingletonUpdate and ISingletonLateUpdate interfaces. Implement the respective interface to execute the corresponding Update and LateUpdate methods. The Game class is decoupled from singleton classes like EventSystem.
13. Actor messages determine if they are sent to their own process, then they don't need to go through the network, they can be processed directly, greatly improving performance.

## ET6 Released!
ET6 has a huge change compared to ET5, it can be said to be a transformation from Feng Jie to Yifei. ET6 has the following amazing features:
1. Full hot update of client logic (based on ILRuntime), no part cannot be updated.
2. Both client and server can hot reload, no need to restart the client or server to modify logic code, extremely convenient for development.
3. Robot framework. ET6's client logic and presentation are separated. The robot program directly shares the client's logic layer code for stress testing. Only very little code is needed to create robots, convenient for stress testing the server.
4. Test case framework. Use the client's logic layer code to write unit tests. Each unit test is a complete game environment, no need for various mock-ups.
5. AI framework. Easier to use than a behavior tree, writing AI is as simple as writing UI.
6. New server architecture, extremely elegant.
7. Inner and outer network kcp networking, strong performance, combined with soft routing modules, can prevent various network attacks.

## ET's commercial MMO project Qian Gu Feng Liu successfully launched
The MMO project with a single server on a single physical machine with 64 cores and 128G memory had 15,000 online at the same time (in reality, the planner limited a single server to 6,000 people online at the same time for ecological reasons, consuming about 30% of the CPU). For the sake of stack line numbers, the online version runs Debug version. If using the Release version with optimizations, performance could double, reaching 30,000 online on a single physical machine! After five months online, it's been very stable. Qian Gu Feng Liu was developed from scratch using the ET framework, taking two years. This development speed is said to be unmatched. The successful launch of Qian Gu Feng Liu proves that ET is capable of developing any large-scale game, with development speed and efficiency being astounding. Client-server technologies used by Qian Gu Feng Liu:
1. Dynamic instances and lines, allocated as needed, recycled after use.
2. Line merging, lines with few people will merge multiple lines. This line merging feature is rarely seen in other MMO games.
3. Client-server scene seamless switching, i.e., seamless large world technology.
4. Cross-server instances, cross-server battlefields.
5. Integrated front-end and back-end, using client code to develop server stress test robots. 4 machines with 24 cores easily simulate 10,000 people doing tasks.
6. Various AI designs of Qian Gu Feng Liu, using ET's newly developed AI framework, making AI development as simple as writing UI.
7. Test case framework. Most important systems of Qian Gu Feng Liu have test cases. Unlike other test cases on the market, each test case of Qian Gu Feng Liu is a complete game environment, protocol level, no need for various interfaces to mock. Very quick to write.
8. Dynamic adjustment of visible players with nine-square grid aoi implementation, reducing server load.
9. Anti-attack. Qian Gu Feng Liu developed the soft routing function. Even if attacked, it can only attack the soft routing. Once attacked, if the player's client finds no response for a few seconds, it can dynamically switch to other soft routing. The whole process is almost imperceptible to the user. The client's network connection does not break, and data is not lost.
10. And many more, not elaborated here.

<h2 id="section2">Introduction to ET</h2>
ET is an open-source game client (based on unity3d) and server dual-end framework. The server side is developed in C# .net core and is a distributed game server. Its characteristics are high development efficiency, strong performance, logic code shared between client and server, perfect hot update mechanism for client and server, support for reliable UDP TCP WebSocket protocols, server-side 3D recast pathfinding, and more.

## Features of ET:
### 1. Distributed server that can be single-step debugged with VS, N becomes 1.
Generally, distributed servers need to start many processes. Once there are many processes, single-step debugging becomes very difficult, causing server development to rely on printing logs to find problems. Usually, developing game logic also requires starting a bunch of processes, not only slow to start but also inconvenient to find problems among piles of logs, a very bad feeling. No one has solved this problem for so many years. The ET framework uses a component design similar to Overwatch, all server contents are broken down into components, and components are mounted on startup based on server type. This is a bit like a computer, computers are modularized into parts like memory, CPU, motherboard, etc., different parts are assembled into different computers. For example, a home desktop computer needs memory, CPU, motherboard, graphics card, monitor, hard drive. But the server used by companies does not need a monitor or graphics card, and the computer in the internet cafe may not need a hard drive, etc. Just because of this design, the ET framework can hang all server components on a server process, then this server process has all server functions, one process can be used as a whole set of distributed servers. This is also like a computer, a desktop computer has all computer components, then it can also be used as a company server, or as an internet cafe computer.
### 2. Distributed server with freely splittable functions, 1 becomes N.
Distributed servers need to develop various types of server processes, such as Login server, gate server, battle server, chat server, friend server, etc., a bunch of various servers. The traditional development method requires knowing in advance which server the current function will be placed on. When more and more functions are developed, such as the chat function was previously on a central server and later needs to be split out into a separate server, this will involve a lot of code migration work, annoyingly troublesome. With the ET framework, you don't really need to care about which server the function you're currently developing will be placed on during normal development. Develop functions in the form of components using a single process. When publishing, use a multi-process configuration to publish in a multi-process form, isn't it convenient? You can split the server however you want. Just hang different components on different servers!
### 3. Cross-platform distributed server.
The ET framework uses C# for the server side, which is now completely cross-platform. Install .net core on Linux, and without modifying any code, it can run. In terms of performance, the performance of .net core is now very strong, much faster than Lua, Python, JS, and so on. It's completely sufficient for game servers. Normally, we develop with VS on Windows, and when publishing, we publish to Linux. The ET framework also provides a one-click synchronization tool, open unity->tools->rsync sync, to sync code to Linux.
```bash
./Run.sh Config/StartConfig/192.168.12.188.txt 
```
can be used to compile and launch the server.

### 4. Coroutine Support
C# natively supports the asynchronous-to-synchronous syntax async and await, which is much more powerful than the coroutine mechanisms in lua and python. Newer versions of languages such as Python and JavaScript have even adopted C#'s coroutine syntax. In distributed server-side development, remote calls between a large number of servers would be very cumbersome without the support of asynchronous syntax. Hence, Java, lacking asynchronous syntax, is suitable for single-server operations but not for large-scale distributed game servers. For example:

```c#
// Send C2R_Ping and wait for the response message R2C_Ping
R2C_Ping pong = await session.Call(new C2R_Ping()) as R2C_Ping;
Log.Debug("Received R2C_Ping");

// Query MongoDB for a Player with id 1 and wait for the return
Player player = await Game.Scene.GetComponent<DBProxyComponent>().Query<Player>(1);
Log.Debug($"Print player name: {player.Name}")
```
As seen, with async and await, all server-to-server asynchronous operations become very coherent, eliminating the need to split logic into multiple stages. This greatly simplifies the development of distributed servers.

### 5. Actor Message Mechanism Similar to Erlang
One major advantage of the Erlang language is its location-transparent message mechanism, where users do not need to worry about the process in which the object resides. With an id, messages can be sent to the object. The ET framework also provides an actor message mechanism. Entity objects only need to attach the MailBoxComponent component, turning them into an Actor. Any server, knowing the id of the entity object, can send messages to it without having to worry about which server or physical machine the entity object is on. The principle is simple: the ET framework offers a location server, and all entity objects with the MailBoxComponent will register their id and location with this location server. If other servers want to send messages to this entity object and do not know its location, they will first query the location server and then send the message upon receiving the location.

### 6. Server-Side Dynamic Logic Update without Downtime
Hotfixing is an indispensable feature for game servers. The ET framework, using a component design, allows for a design similar to Overwatch. Components have members but no methods; all methods are made into extension methods and placed in the hotfix dll. Runtime logic can be hotfixed by reloading the dll.

### 7. Client-Side Hotfix Using C#, with One-Click Switching
The client-side hotfix can be achieved with minor modifications to csharp.lua or ILRuntime. There's no need to use the cumbersome lua anymore. The client side can implement hot updates for all logic, including protocols, config, UI, etc.

### 8. Client-Side Hot Reload
Developers can modify client-side logic code without having to restart the client, making development extremely convenient.

### 9. Client and Server Use the Same Language, Sharing Code
By downloading the ET framework and opening the server project, it can be seen that the server references a lot of code from the client. This achieves code sharing between client and server. For example, the network messages between client and server can completely share one file, and modifying a message only requires changing it in one place.

### 10. Seamless Switching between KCP, ENET, TCP, and Websocket Protocols
The ET framework not only supports TCP but also reliable UDP protocols (ENET and KCP). ENet, used by League of Legends, is known for its speed and performs very well even in conditions of packet loss. We have tested that TCP performance becomes unacceptable with 5% packet loss in a MOBA game, but ENet still does not feel laggy even with 20% packet loss, which is very powerful. The framework also supports the KCP protocol, another reliable UDP protocol, which is said to perform better than ENET. Note that when using KCP, you need to implement your own heartbeat mechanism; otherwise, the server will disconnect if no packets are received in 20 seconds. Protocols can be seamlessly switched.

### 11. 3D Recast Pathfinding Functionality
Unity can export scene data for server-side recast pathfinding. This is very convenient for MMOs. The demo demonstrates the server-side 3d pathfinding functionality.

### 12. Server-Side Supports REPL, Can Dynamically Execute New Code
This allows for the printing of any data in the process, greatly simplifying the difficulty of server-side problem diagnosis. To enable REPL, simply enter 'repl' in the console and press enter to enter REPL mode.

### 13. Client-Side Robot Framework Support
With just a few lines of code, robots can be created to log into the game. Robot stress testing becomes easy. Robots are exactly like normal players. Stress testing with robots before going live greatly reduces the risk of crashes upon launch.

### 14. AI Framework
ET's AI framework makes writing AI simpler than UI.

### 15. Test Case Framework
Unlike test cases on the market, ET's test cases are a complete game environment, targeting the protocol level, without the need for various interfaces to mock. This makes writing very fast.

### 16. Many More Features, Too Numerous to List
a. Extremely convenient for checking CPU usage and memory leak detection, with built-in analysis tools in VS, no need to worry about performance and memory leak detection.
b. Using the NLog library, logging is extremely convenient. During normal development, all server logs can be sent to one file, eliminating the need to search for logs file by file.
c. Uniform use of Mongodb's bson for serialization. Messages and configuration files are all bson or json, and using mongodb as a database eliminates the need for format conversion.
d. Provides a synchronization tool.

The ET framework is a powerful and flexible distributed server architecture that can meet the needs of most large-scale games. Using this framework, client-side developers can complete the development of both sides on their own, saving a lot of manpower, material resources, and communication time.

Relevant Websites:
[ET Forum](https://et-framework.cn)

Shared by Group Members:
[Behavior Tree and fgui Branch (Developed and maintained by Duke Chiang)](https://github.com/DukeChiang/ET.git)
[ET Learning Notes Series (Written by 烟雨迷离半世殇)](https://www.lfzxb.top/)
[Graphics Rendering and ET Learning Notes (Written by 咲夜詩)](https://acgmart.com/)
[Framework Server-Side Operation Process](http://www.cnblogs.com/fancybit/p/et1.html)
[ET Startup Configuration](http://www.cnblogs.com/fancybit/p/et2.html)
[Framework Demo Introduction](http://www.jianshu.com/p/f2ea0d26c7c1)
[Linux Deployment](http://gad.qq.com/article/detail/35973)

Commercial Projects:
1. [千古风流](https://www.qiangu.com/)
2. [神选誓约](https://www.taptap.cn/app/248095)
3. [魔法点点2](https://www.taptap.com/app/227804)
4. [养不大](https://www.taptap.com/app/71064)
5. 天天躲猫猫2（ios2019春节下载排行19）
6. [牛虎棋牌](https://gitee.com/ECPS_admin/PlanB)
7. [五星麻将](https://github.com/wufanjoin/fivestar)
8. [神选誓约](https://www.taptap.cn/app/248095)
9. [代号肉鸽：无限](https://www.taptap.cn/app/247225)

Group Member Demos:
1. [Landlords (Client-Server)](https://github.com/Viagi/LandlordsCore)
2. [Inventory System](https://gitee.com/ECPS_admin/planc)
3. [Mobile Rendering Technology Demo](https://github.com/Acgmart/Sekia_TechDemo)
4. [Ball Battle (ET7.2)](https://github.com/FlameskyDexive/Legends-Of-Heroes)

Video Tutorials:
[Letter Brother ET6.0 Tutorial](https://edu.uwa4d.com/course-intro/1/375)
[Teacher Rou Bing's Lecture](http://www.taikr.com/my/course/972)
[Jian Ming's Lecture](https://edu.manew.com/course/796)
[ET Beginner Tutorial - Chu Jian's Lecture](https://pan.baidu.com/s/1a5-j2R5QctZpC9n3sMC9QQ) Password: ru1j
[ET Beginner Tutorial New Version - Chu Jian's Lecture](https://www.bilibili.com/video/av33280463/?redirectFrom=h5)
[ET Framework Operation Guide on Mac - L's Lecture](https://pan.baidu.com/s/1VUQbdd1Yio7ULFXwAv7X7A) Password: l3e3
[ET Framework Series Tutorials - Yan Yu's Lecture - Version 6.0](https://space.bilibili.com/33595745/favlist?fid=759596845&ftype=create)

.net core Game Resource Sharing:
[Collection of Various dotnet core Projects](https://github.com/thangchung/awesome-dotnet-core)

__Discussion QQ Group: 474643097__


# Alipay Donation
![Donate to this project via Alipay](https://github.com/egametang/ET/blob/master/Book/donate.png)

# Friendly Links
[Box2DSharp](https://github.com/Zonciu/Box2DSharp) - C# port of box2d, very performant.
[xasset](https://github.com/xasset/xasset) - Dedicated to providing a concise and robust resource management environment for Unity projects.
[QFramework](https://github.com/liangxiegame/QFramework) - Your first K.I.S.S Unity3d Framework.
[ET UI Framework](https://github.com/zzjfengqing/ET-EUI) - Alphabet Brother's implementation of the UI framework, ET style, various event distributions.
[ETCsharpToXLua](https://github.com/zzjfengqing/ETCsharpToXLua) - ET client hot update implemented by Alphabet Brother using csharp.lua.
[et-6-with-ilruntime](https://www.lfzxb.top/et-6-with-ilruntime) - ET client hot update implemented by Yan Yu using ILRuntime.
[Luban](https://github.com/focus-creative-games/luban) - Game configuration solution suitable for large and medium-sized projects.
[ET-YIUI](https://github.com/LiShengYang-yiyi/YIUI/tree/YIUI-ET7.2) - ETUI Framework.