#ifndef RPC_RPC_SESSION_H
#define RPC_RPC_SESSION_H

#include <boost/asio.hpp>
#include <boost/noncopyable.hpp>
#include <boost/enable_shared_from_this.hpp>
#include "Rpc/RpcTypedef.h"

namespace Egametang {

class RpcServer;

class RpcSession:
		private boost::noncopyable,
		public RpcCommunicator,
		public boost::enable_shared_from_this<RpcSession>
{
private:
	RpcServer& rpc_server_;

	void SendResponse(RpcMetaPtr meta, StringPtr message);

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message);
	virtual void OnSendMessage(RpcMetaPtr meta, StringPtr message);

public:
	RpcSession(RpcServer& server);
	~RpcSession();
	void Start();
	void Stop();
};

} // namespace Egametang

#endif // RPC_RPC_SESSION_H
