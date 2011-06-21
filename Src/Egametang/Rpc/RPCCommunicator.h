#ifndef RPC_RPC_COMMUNICATOR_H
#define RPC_RPC_COMMUNICATOR_H

#include <google/protobuf/service.h>
#include <boost/unordered_map.hpp>
#include <boost/asio.hpp>
#include "Base/Marcos.h"
#include "Base/Typedef.h"
#include "Rpc/RpcTypedef.h"

namespace Egametang {

class RPCCommunicator
{
protected:
	boost::asio::io_service& io_service_;
	boost::asio::ip::tcp::socket socket_;

public:
	explicit RPCCommunicator(boost::asio::io_service& io_service);

	boost::asio::ip::tcp::socket& Socket();

	// recieve response
	void RecvSize();
	void RecvMessage(IntPtr size, const boost::system::error_code& err);
	void RecvDone(StringPtr ss, const boost::system::error_code& err);

	// send request
	void SendSize(int size, std::string message);
	void SendMessage(std::string message, const boost::system::error_code& err);
	void SendDone(const boost::system::error_code& err);

	virtual void OnRecvMessage(StringPtr ss);
	virtual void OnSendMessage();
};

} // namespace Egametang

#endif // RPC_RPC_COMMUNICATOR_H
