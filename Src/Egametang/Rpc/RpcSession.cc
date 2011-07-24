#include <boost/bind.hpp>
#include "Rpc/RpcSession.h"

namespace Egametang {

RpcSession::RpcSession(RpcServer& rpc_server):
		rpc_server_(rpc_server), RpcCommunicator(rpc_server_.IOService())
{
}

void RpcSession::OnRecvMessage(RpcMetaPtr meta, StringPtr message)
{
	rpc_server_.RunService(shared_from_this(), meta, message,
			boost::bind(&RpcSession::SendMeta, shared_from_this(), _1, _2));

	// 可以循环利用
	RecvMeta(meta, message);
}

void RpcSession::OnSendMessage(RpcMetaPtr meta, StringPtr message)
{
}

void RpcSession::Start()
{
	RpcMetaPtr meta(new RpcMeta());
	StringPtr message(new std::string);
	RecvMeta(meta, message);
}

void RpcSession::Stop()
{
	RpcCommunicator::Stop();
	rpc_server_.RemoveSession(shared_from_this());
}

} // namespace Egametang
