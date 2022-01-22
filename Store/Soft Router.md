# 使用步骤

1. 打开所有进程
  dotnet Server.dll --AppType=Watcher --Console=1
2. 使用Robot或者客户端直接进入地图
3. 关掉当前连接的路由进程.观察切换效果


# 关于软路由实现细节:
有一个socket连接专门用于寻找可用的路由地址.并且在寻找到地址的同时将自己的conn以及目标gate传给路由.这样可以做到完全不改动框架从连接到建立成功的任何细节
连接已经建立,超时切换路由的时候.很遗憾必须动到框架自带的KChannel.没有想到很优雅的实现.这里的处理方式是

1. 关闭超时监听组件.寻找到新的可用路由.并将客户端连接的地址指向新的地址.如果寻址失败直接断开链接
2. 将KChannel的IsRouterConnected 路由状态置为需要发送重连信息.这样会以300毫秒的间隔重复发送重连请求.
3. gate收到信息.验证成功后将地址改为新的地址.返还重连成功信息.
4. 到达client之后.修改IsRouterConnected为true.重连结束.重新添加超时监听组件

[视频教程](https://www.bilibili.com/video/BV1Aq4y1f7dA?spm_id_from=333.999.0.0)  