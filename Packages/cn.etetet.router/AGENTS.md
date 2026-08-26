# cn.etetet.router

## 概述

软路由包，负责外网与内网传输转发及连接状态维护。

## 规范入口

- 开发、构建与测试统一遵守仓库根目录和 `Packages/cn.etetet.harness/AGENTS.md`。
- 包依赖以 `package.json` 为源，禁止引入反向依赖或未声明的跨包访问。

## 网络接收契约

- 接收循环使用 `IKcpTransport.Available()` 的 bool 结果判断是否可读。
- `Recv()` 返回的消息长度必须先通过协议最小长度检查，再读取缓存或转发消息。
