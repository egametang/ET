#include <boost/bind.hpp>
#include "Rpc/RpcSession.h"
#include "Rpc/RpcServer.h"

namespace Egametang {

RpcSession::RpcSession(RpcServer& server):
		rpc_server(server), RpcCommunicator(rpc_server.IOService())
{
}

void RpcSession::OnRecvMessage(RpcMetaPtr meta, StringPtr message)
{
	RpcSessionPtr session = shared_from_this();
	rpc_server.RunService(session, meta, message,
			boost::bind(&RpcSession::SendMeta, session, _1, _2));

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
	RpcSessionPtr session = shared_from_this();
	rpc_server.RemoveSession(session);
}

} // namespace Egametang
