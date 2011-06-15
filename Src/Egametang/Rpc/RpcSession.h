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
		public RPCCommunicator,
		public boost::enable_shared_from_this<RpcSession>
{
private:
	RpcServer& rpc_server_;

	virtual void OnRecvMessage(StringPtr ss, const boost::system::error_code& err);
	virtual void OnSendMessage(int32 id, RpcHandlerPtr handler,
			const boost::system::error_code& err);

public:
	RpcSession(RpcServer& server);
	~RpcSession();
	boost::asio::ip::tcp::socket& Socket();
	void Start();
	void Stop();
};

} // namespace Egametang

#endif // RPC_RPC_SESSION_H
