#ifndef RPC_RPCCOMMUNICATOR_H
#define RPC_RPCCOMMUNICATOR_H

#include <google/protobuf/service.h>
#include <boost/unordered_map.hpp>
#include <boost/asio.hpp>
#include "Base/Marcos.h"
#include "Base/Typedef.h"
#include "Rpc/RpcTypedef.h"

namespace Egametang {

struct RpcMeta
{
	// message长度
	std::size_t size;

	// 消息id, 用于处理异步回调
	std::size_t id;

	// 消息opcode, 是proto的full_path哈希值
	std::size_t method;

	RpcMeta(): size(0), id(0), method(0)
	{
	}

	std::string ToString()
	{
		return "";
	}
};

class RpcCommunicator
{
protected:
	boost::asio::io_service& io_service_;
	boost::asio::ip::tcp::socket socket_;

public:
	explicit RpcCommunicator(boost::asio::io_service& io_service);

	boost::asio::ip::tcp::socket& Socket();

	// recieve response
	void RecvMeta();
	void RecvMessage(RpcMetaPtr meta, const boost::system::error_code& err);
	void RecvDone(RpcMetaPtr meta, StringPtr message, const boost::system::error_code& err);

	// send request
	void SendMeta(RpcMeta meta, std::string message);
	void SendMessage(std::string message, const boost::system::error_code& err);
	void SendDone(const boost::system::error_code& err);

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message);
	virtual void OnSendMessage();

	virtual void Stop();
};

} // namespace Egametang

#endif // RPC_RPCCOMMUNICATOR_H
