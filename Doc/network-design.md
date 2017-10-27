The design of # network layer
The ET framework provides a more powerful network messaging layer, sending messages, subscribing to messages, and making it easy, very clear and simple.
 
#### 1. send common message
There are two main components, NetOuterComponent handles the connection of the client, and NetInnerComponent handles the connection within the server
The two components can access the connection according to the address, each connection is encapsulated into a Session object, and the Session object has two methods for sending the message:
``` C#
// Create or access a connection according to the address
Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(innerAddress);

// Send only and no return
session.Send(new R2G_GetLoginKey());

// Send the R2G_GetLoginKey message and wait for the message to return a G2R_GetLoginKey message
G2R_GetLoginKey g2RGetLoginKey = await session.Call<G2R_GetLoginKey>(new R2G_GetLoginKey() {Account = "zhangsan"});
Log.Debug("print response message content: " + g2RGetLoginKey.ToJson())
```
Because of C#'s powerful async await syntax, ET framework RPC message is very simple, after sending logic to be coherent, without dismantling the two logic, because of this characteristic, C# distributed framework for writing very much, because it is nothing more than a distributed inter process network news. If you don't have this function, think about it, send the message in one place, you have to subscribe to a return message, and the two piece of code is disconnected. Even more frightening is the continuous multiple RPC requests:
``` C#
// The client sends an account password to the login server to verify, and waits for the login response message to return, login will assign a gateway to the client
R2C_Login r2CLogin = await session.Call<R2C_Login>(new C2R_Login() { Account = "a", Password = "b" });
// Client connect gateway
Session gateSession = Game.Scene.GetComponent<NetOuterComponent>().Create(r2CLogin.Address);
// The client sends the message to the gateway and waits for the gateway to verify the return
G2C_LoginGate g2CLoginGate = await gateSession.Call<G2C_LoginGate>(new C2G_LoginGate(r2CLogin.Key));
Log.Info("login ok!");
// 获取玩家的物品信息
G2C_Items items = await gateSession.Call<G2C_Items>(new C2G_Items());
```
You can see the login LoginServer, immediately log on gateserver, after the completion of the query and login the game player items of information, the whole process looks very coherent, if there is no async await, this code will be split into at least 4 pieces in 4 functions. Distributed servers have many RPC calls, and there is no syntax support for async await, which is inconceivable. So some people use nodejs, java to write game server, I can not understand, write a single clothing can also, write distributed server, ha ha!
#### 2.  ordinary news subscription
The top is sending messages. How does the server subscribe to processing a message? Very concise:
```C#
// Processing login RPC messages and returning response
[MessageHandler(AppType.Login)]
public class C2R_LoginHandler : AMRpcHandler<C2R_Login, R2C_Login>
{
	protected override async void Run(Session session, C2R_Login message, Action<R2C_Login> reply)
	{
		R2C_Login response = new R2C_Login();
		try
		{
			Log.Debug(message.ToJson());
			reply(response);
		}
		catch (Exception e)
		{
			ReplyError(response, e, reply);
		}
	}
}
```
RPC message only need to add a hotfix class in DLL class, inherited from AMRpcHandler, virtual method, ET uses a declarative message subscription approach, a RPC message processing class, only need to add the MessageHandlerAttribute can automatically be found and registered to the frame frame, does not need to register with manual function. The class MessageHandlerAttribute above sets the AppType.Login, which indicates that only the Login server registers the RPC processing class. Is it very simple? Similarly, registering non RPC messages only needs to add a class that inherits from AMHandler. The entire message handling class does not contain any state, so it can be reload.

#### 3. send actor message
The ET framework also provides a distributed message mechanism similar to the Erlang language, regardless of any object in the process, only the ActorComponent mount components need, any process can take the object of ID, sending the object message, messages are sent to the object in the process and to the object processing. Sending Actor messages is different from ordinary messages. To send actor messages, server must hang on the ActorProxyComponent components:
```c#
// ActorProxyComponent get actorproxy
ActorProxy actorProxy = Game.Scene.GetComponent<ActorProxyComponent>().Get(id);
// Sending messages to actor
actorProxy.Send(new Actor_Test());
// Sending rpc messages to actor
ActorRpc_TestResponse response = await actorProxy.Call<ActorRpc_TestResponse>(ActorRpc_TestRequest());
```

#### 4.actor subscription processing
Subscribing to actor messages is just like regular messages, just inheriting AMActorHandler and adding ActorMessageHandler tags. The difference is that AMActorHandler needs to provide the type of Actor, such as the following actor message, which is sent to the Player object
```c#
[ActorMessageHandler(AppType.Map)]
public class Actor_TestHandler : AMActorHandler<Player, Actor_Test>
{
	protected override async Task<bool> Run(Player player, Actor_Test message)
	{
		Log.Debug(message.Info);

		player.GetComponent<UnitGateComponent>().GetActorProxy().Send(message);
		return true;
	}
}
```
Similarly, subscribing to ActorRpc messages requires inheritance of AMActorRpcHandler, and also using reply to return response messages.
```c#
[ActorMessageHandler(AppType.Map)]
public class ActorRpc_TestRequestHandler : AMActorRpcHandler<Player, ActorRpc_TestRequest, ActorRpc_TestResponse>
{
	protected override async Task<bool> Run(Player entity, ActorRpc_TestRequest message, Action<ActorRpc_TestResponse> reply)
	{
		reply(new ActorRpc_TestResponse() {response = "response actor rpc"});
		return true;
	}
}
```

#### 5.Exception handling rpc message
ET framework message layer provides a powerful exception handling mechanism, all RPC response messages are inherited with AResponse, AResponse with error and error information,
```
public abstract class AResponse: AMessage
{
	public uint RpcId;
	public int Error = 0;
	public string Message = "";
}
```
You can catch RpcException exceptions and do different exception handling through ErrorCode, such as client login:
```
try
{
	R2C_Login r2CLogin = await session.Call<R2C_Login>(new C2R_Login() { Account = "a", Password = "b" });
}
catch (RpcException e)
{
	if (e.Error == ErrorCode.ERR_AccountNotFound)
	{
		Log.Debug("account not exist");
		return;
	}
	if (e.Error == ErrorCode.PasswordError;)
	{
		Log.Debug("password error");
		return;
	}
}
```
The ET framework is the most convenient anomaly information will cross process transfer, for example, the A process to the B process launched a Rpc request, B process needs to request C in response to C request response process before D, before the B results, the D process in the process of occurrence of an exception, the exception will from D- >C->B->A. The A process in try catch to capture the exception, the exception will BCD with the whole process of the stack information, check distributed abnormal bug becomes very simple.
#### summary
This paper introduces the network layer usage of ET framework, and the ET framework provides a very perfect distributed network layer, a powerful distributed exception handling mechanism. Because of the use of collaboration, ET send messages and remote call and its simple and convenient, distributed development is as convenient as the development of stand-alone.



