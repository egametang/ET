#ifndef RPC_RPC_SESSION_H
#define RPC_RPC_SESSION_H

#include <boost/asio.hpp>
#include <boost/noncopyable.hpp>
#include <boost/enable_shared_from_this.hpp>
#include "Rpc/RpcTypedef.h"

namespace Egametang {

class RpcServer;

class RpcSession: private boost::noncopyable, public boost::enable_shared_from_this<RpcSession>
{
private:
	boost::asio::ip::tcp::socket socket_;
	RpcServer& rpc_server_;

	void RecvMessegeSize();
	void RecvMessage(IntPtr size, const boost::system::error_code& err);
	void RecvMessageHandler(StringPtr ss, const boost::system::error_code& err);

	void SendMessageSize(RpcResponsePtr response);
	void SendMessage(const RpcResponsePtr response, const boost::system::error_code& err);
	void SendMessageHandler(int32 id, RpcHandlerPtr handler, const boost::system::error_code& err);

public:
	RpcSession(RpcServer& server);
	~RpcSession();
	boost::asio::ip::tcp::socket& Socket();
	void Start();
	void Stop();
};

} // namespace Egametang

#endif // RPC_RPC_SESSION_H
