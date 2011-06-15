#ifndef RPC_RPC_COMMUNICATOR_H
#define RPC_RPC_COMMUNICATOR_H

#include <google/protobuf/service.h>
#include <boost/unordered_map.hpp>
#include <boost/asio.hpp>
#include "Base/Typedef.h"
#include "Rpc/RpcTypedef.h"

namespace Egametang {

class RPCCommunicator
{
protected:
	boost::asio::ip::tcp::socket socket_;

public:
	RPCCommunicator();
	// recieve response
	void RecvMessegeSize();
	void RecvMessage(IntPtr size, const boost::system::error_code& err);

	// send request
	void SendMessageSize(const RpcRequestPtr request, RpcHandlerPtr handler);
	void SendMessage(const RpcRequestPtr request, RpcHandlerPtr handler,
			const boost::system::error_code& err);

	virtual void OnRecvMessage(StringPtr ss, const boost::system::error_code& err) = 0;
	virtual void OnSendMessage(int32 id, RpcHandlerPtr handler,
			const boost::system::error_code& err) = 0;
};

} // namespace Egametang

#endif // RPC_RPC_COMMUNICATOR_H
