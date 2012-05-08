#include <memory>
#include <functional>
#include "Rpc/RpcSession.h"
#include "Rpc/RpcServer.h"

namespace Egametang {

RpcSession::RpcSession(boost::asio::io_service& ioService, RpcServer& server):
		RpcCommunicator(ioService), rpcServer(server), isStopped(false)
{
}

RpcSession::~RpcSession()
{
}

void RpcSession::OnRecvMessage(RpcMetaPtr meta, StringPtr message)
{
	auto session = shared_from_this();
	rpcServer.RunService(session, meta, message,
			std::bind(&RpcSession::SendMeta, session,
					std::placeholders::_1, std::placeholders::_2));

	// RunService函数里读完就不使用了,可以循环利用
	RecvMeta(meta, message);
}

void RpcSession::OnSendMessage(RpcMetaPtr meta, StringPtr message)
{
}

void RpcSession::Start()
{
	auto meta = std::make_shared<RpcMeta>();
	auto message = std::make_shared<std::string>();
	RecvMeta(meta, message);
}

void RpcSession::Stop()
{
	if (isStopped)
	{
		return;
	}
	isStopped = true;
	// 延迟删除,必须等所有的bind执行完成后才能remove,
	// 否则会出现this指针失效的问题
	ioService.post(std::bind(&RpcServer::Remove, &rpcServer, shared_from_this()));
}

} // namespace Egametang
