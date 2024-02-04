# ET-EUI
基于ET框架的封装的WebSocket，支持WebGL

#### 使用方式

-  在SceneFactory的CreateZoneScene上添加
zoneScene.AddComponent<NetWSJSComponent, int>(SessionStreamDispatcherType.SessionStreamDispatcherClientOuter);

-  在使用的时候先创建Session
Session gateSession = zoneScene.GetComponent<NetWSJSComponent>().Create($"ws://{r2CLoginZone.GateAddrss}");

-  拿到Session正常发送就行
G2C_Login2Gate g2CLogin2Gate = (G2C_Login2Gate)await gateSession.Call(new C2G_Login2Gate() { GateKey = r2CLoginZone.GateKey });

#### 插件地址
https://gitee.com/chen_fen_hui/web-socket-js.git
