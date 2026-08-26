# UDP Transport 测试计划

## 目标

- `UdpTransport.Available()` 必须为已排队的零长度 UDP datagram 返回 `true`。
- 零长度 UDP datagram 必须能被 `Recv()` 消费，不能阻塞后续 datagram。

## 用例

### Core_UdpTransport_ZeroLengthDatagram_Test

1. 创建绑定到 loopback 随机端口的接收端 `UdpTransport`。
2. 向接收端依次发送一个零长度 UDP datagram 和一个一字节 datagram。
3. 验证 `Available()` 返回 `true`，表示接收循环应调用 `Recv()`。
4. 验证 `Recv()` 返回 0，并消费该 datagram。
5. 验证后续 datagram 仍然可读且可被完整消费。
6. 验证全部消费后 `Available()` 返回 `false`。
