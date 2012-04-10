#ifndef RPC_RPCCOMMUNICATOR_H
#define RPC_RPCCOMMUNICATOR_H

#include <google/protobuf/service.h>
#include <boost/unordered_map.hpp>
#include <boost/asio.hpp>
#include <boost/format.hpp>
#include <boost/noncopyable.hpp>
#include "Base/Marcos.h"
#include "Base/Typedef.h"
#include "Rpc/Typedef.h"

namespace Egametang {

class RpcMeta
{
public:
	std::size_t size;

	std::size_t id;

	std::size_t method;

	RpcMeta(): size(0), id(0), method(0)
	{
	}

	std::string ToString()
	{
		boost::format format("size: %1%, id: %2%, method: %3%\n");
		return boost::str(format % size % id % method);
	}
};

class RpcCommunicator: public boost::noncopyable
{
protected:
	bool isStopped;
	boost::asio::io_service& ioService;
	boost::asio::ip::tcp::socket socket;

	virtual void Stop();
public:
	explicit RpcCommunicator(boost::asio::io_service& io_service);
	virtual ~RpcCommunicator();
	boost::asio::ip::tcp::socket& Socket();

	// recieve response
	void RecvMeta(RpcMetaPtr meta, StringPtr message);
	void RecvMessage(RpcMetaPtr meta, StringPtr message, const boost::system::error_code& err);
	void RecvDone(RpcMetaPtr meta, StringPtr message, const boost::system::error_code& err);
	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message);

	// send request
	void SendMeta(RpcMetaPtr meta, StringPtr message);
	void SendMessage(RpcMetaPtr meta, StringPtr message, const boost::system::error_code& err);
	void SendDone(RpcMetaPtr meta, StringPtr message, const boost::system::error_code& err);
	virtual void OnSendMessage(RpcMetaPtr meta, StringPtr message);
};

} // namespace Egametang

#endif // RPC_RPCCOMMUNICATOR_H
