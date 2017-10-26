# [中文](https://github.com/egametang/Egametang/blob/master/README-CN.md) 

__Chinese Tencent QQ group : 474643097__  
[google group](https://groups.google.com/forum/#!forum/et-game-framework) 

### 1.A distributed server, can use visual studio debugging，N->1  
Generally speaking, distributed server starts a lot of processes, once the process is more, single step debugging becomes very difficult, leading to server development basically rely on log to find the problem. Common development has opened a lot of game logic process, not only the slow start, and find the problem and not convenient to log pile check problem in a pile, this feeling is very bad, so many years no one can solve the problem. The ET framework uses a component design similar to the watch pioneer, and all server contents are disassembled into components, and the components that need to be mounted according to the type of server are started. It is a bit like a computer, the computer module is split into memory, CPU, motherboard parts and so on, collocation of different parts can be assembled into a different computer, such as home desktop CPU, motherboard, memory, graphics, display, hard disk. And the company uses the server does not need the display and graphics card, the Internet bar computer may not need hard disk, etc.. Because of this design, the ET framework can be all server components are linked in a server process, the server process has all the functions of the server, a process can be used as a whole set of distributed servers. It's also like a computer, which has all the computer components, and it can be used as a company server or as an Internet cafe.  
### 2.A Distributed server, can freely split function，1->N  
Distributed server to develop various types of server processes, such as Login server, gate server, battle server, chat server friend server, a server, the traditional development mode need to know in advance the function which should be put in the server, when more and more functions, such as chat on a central server then, need to split out into a separate server, this will involve a large number of code migration work, tired. The ET framework does not really need to be concerned about what kind of functionality the current development will place on server, and only uses one process to develop it, and the function is developed into a component. Is it convenient to use a multi process configuration to publish it into multiple processes when you publish it? How do you split the server?. You can split it with very few code changes. Different server hangs different components on it?!
### 3.Cross platform distributed server 
ET framework uses C# as server-side, and now C# is completely cross platform, install.Netcore on Linux, you can do without modifying any code, you can run. Performance, now.Netcore performance is very strong, faster than Lua, python, JS faster. The game server completely be nothing difficult. We usually use VS to develop debugging on windows, and release it to Linux on the time of release. ET framework also provides a key synchronization tool, open unity->tools->rsync synchronization, you can synchronize the code to the linux  
```bash
./Run.sh Config/StartConfig/192.168.12.188.txt 
```
You can compile and start the server.  
### 4.Provide Coroutine support  
C# naturally supports asynchronous variable synchronous syntax async and await, much more powerful than Lua and python, and the new version of Python and JavaScript language even copy the C#'s co - operation grammar. There is no asynchronous syntax to support remote calls between distributed servers and a large number of servers, and development will be very troublesome. So Java does not have asynchronous syntax, doing single service is OK, not suitable for large-scale distributed game server. For example:  

```c#
// Send C2R_Ping and wait for response message R2C_Ping
R2C_Ping pong = await session.Call<R2C_Ping>(new C2R_Ping());
Log.Debug("recv R2C_Ping");

// Query mongodb for a ID of 1 Player and wait for return
Player player = await Game.Scene.GetComponent<DBProxyComponent>().Query<Player>(1);
Log.Debug($"print player name: {player.Name}")
```
It can be seen that with async await, asynchronous operations between all servers will become very coherent, without disassembling into multiple sections of logic. Greatly simplifies the development of distributed servers 
### 5.Provide actor message mechanism similar to Erlang  
One of the advantages of Erlang language is the location transparent message mechanism. The user does not care about which process the object is in, and when you get the ID, you can send the message to the object. The ET framework also provides a actor message mechanism, the entity object need only hang ActorComponent components, the object becomes a Actor, any server only needs to know the object ID can send a message to it, totally do not care about this entity in which server, in which physical machine. This principle is actually very simple, the ET framework provides a location server, all mounted ActorComoponet object will own ID with location registration to the location server, the other server when sending the message object if you don't know the real position of the object, will go to the location server query, query to the position to be transmitted.
### 6.Provide server with dynamic update logic function   
hotfix is an indispensable component of game server function, design using the ET framework, the design can be made to watch the pioneer, only component members, no way, all the way into an expansion method in hotfix DLL, when reload DLL can reload all logic more hot.
### 7.Client can hotfix
Because of the IOS restrictions, the previous unity hot update generally use Lua, leading to unity3d developers to write two kinds of code, trouble to death. Fortunately, the ILRuntime library comes out, using the ILRuntime library, unity3d can use the C# language to load the hot update DLL for thermal update. One drawback of ILRuntime is that it doesn't support VS debug at development time, which is a little uncomfortable. The ET framework uses a pre compiled instruction ILRuntime to seamlessly switch. ILRuntime is not used when developing, but using Assembly.Load to load the hot update dynamic library, so that it can be easily used VS single step debugging. At the time of release, defining the precompiled instruction ILRuntime can seamlessly switch to using ILRuntime to load the hot update dynamic library. So it's easy to develop and convenient
### 8.The client server uses the same language and shares the code  
Download the ET framework, open the server project, you can see that the server referenced a lot of client code, through the client code approach to achieve a double end shared code. For example, the network message between the client and server can share a file on both sides, adding a message only needs to be modified.  
### 9.The UDP TCP protocol seamlessly switches
The ET framework not only supports TCP, but also support the reliable UDP protocol, UDP support is a package of ENet library, using the ENet and hero alliance network library, its characteristic is rapid, and the performance of the network packet loss situation is also very good, that we tested TCP in packet loss 5%, MoBa game card no, but the use of ENet, 20% packet loss still don't feel a card. Very powerful.  

### 10.there are many, I will not detail
A. and its easy to check CPU occupancy and memory leak check, vs comes with analytical tools, no longer worry about performance and memory leak check  
B. uses NLog library, hits log and its convenience, when develops normally, may hit all the server log to a document, also does not need each document search log again  
C. unified the use of Mongodb bson serialization, the message and configuration files are all bson or JSON, and later use mongodb to do the database, and no longer need to format conversion.  
D. provides a powerful AI behavior tree tool  
E. provides a synchronization tool  
F. provides command line configuration tools, configuring the distribution is very simple
The server side of the ET framework is a powerful and flexible distributed server architecture, which can fully meet the needs of most large games. Using this framework, the client developer can complete the double end development by himself, save a lot of manpower and material resources, and save a lot of communication time.  

Usage method：  
[start-guide](https://github.com/egametang/Egametang/blob/master/Doc/start-guide.md)    
[component-design](https://github.com/egametang/Egametang/blob/master/Doc/component-design.md)   
[network-design](https://github.com/egametang/Egametang/blob/master/Doc/network-design.md) 

__Chinese Tencent QQ group : 474643097__  
email: egametang@qq.com
