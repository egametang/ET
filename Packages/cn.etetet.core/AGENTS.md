# cn.etetet.core

## 概述

核心框架包，承载 ET 基础能力以及 `ET.Core` 的 DotNet 构建入口。

## 详细文档

- **编译构建**：请查看 `/et-build` skill
- **架构规范**：请查看 `/et-code` skill

## 核心目录

| 路径 | 说明 |
|------|------|
| `DotNet~` | `ET.Core.csproj` 与相关生成脚本 |
| `Scripts` | Core 层共享与服务端代码 |
| `Runtime` | Unity 运行时代码 |

## UDP Transport 可读性契约

- `IKcpTransport.Available()` 是 bool 可读门控；已排队的零长度 UDP datagram 也必须返回 `true`，以便 `Recv()` 消费它并继续处理后续 datagram。
- `UdpTransport.Available()` 通过非阻塞 `Socket.Poll(0, SelectMode.SelectRead)` 判断是否可读，不提供 datagram 字节数语义。
- 回归测试入口：`Core_UdpTransport_ZeroLengthDatagram_Test`，位于 `Scripts/Hotfix/Test/Network/`。
