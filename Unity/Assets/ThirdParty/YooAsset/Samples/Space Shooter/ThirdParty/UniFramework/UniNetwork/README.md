# UniFramework.Network

一个高效的基于IOCP模型的网络系统。

```c#
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UniFramework.Network;

// 登录请求消息
class LoginRequestMessage
{
    public string Name;
    public string Password;
}

// 登录反馈消息
class LoginResponseMessage
{
    public string Result;
}

// TCP客户端
UniFramework.Network.TcpClient _client = null;

// 创建TCP客户端
void CreateClient()
{
    // 初始化网络系统
    UniNetwork.Initalize();

    // 创建TCP客户端
    int packageMaxSize = short.MaxValue;
    var encoder = new DefaultNetPackageEncoder();
    var decoder = new DefaultNetPackageDecoder();
    _client = UniNetwork.CreateTcpClient(packageMaxSize, encoder, decoder);

    // 连接服务器
    var remote = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
    _client.ConnectAsync(remote, OnConnectServer);
}

// 关闭TCP客户端
void CloseClient()
{
    if(_client != null)
    {
        _client.Dispose();
        _client = null; 
    }
}

void OnConnectServer(SocketError error)
{
    Debug.Log($"Server connect result : {error}");
    if (error == SocketError.Success)
        Debug.Log("服务器连接成功！");
    else
        Debug.Log("服务器连接失败！");
}

void Update()
{
    // 每帧去获取解析的网络包
    DefaultNetPackage networkPackage = client.PickPackage() as DefaultNetPackage;
    if(networkPackage != null)
    {
        string json = Encoding.UTF8.GetString(networkPackage.BodyBytes);
        LoginResponseMessage message = JsonUtility.FromJson<LoginResponseMessage>(json);
        Debug.Log(message.Result);
    }
}

// 发送登录请求消息
void SendLoginMessage()
{
    LoginRequestMessage message = new LoginRequestMessage();
    message.Name = "hevinci";
    message.Password = "1234567";

    DefaultNetPackage networkPackage = new DefaultNetPackage();
    networkPackage.MsgID = 10001;
    networkPackage.BodyBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(message));
    _client.SendPackage(networkPackage);
}
```

