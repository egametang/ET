#include "Net/RpcSession.h"

namespace Hainan {

typedef boost::unordered_set<RpcSessionPtr> RpcSessionSet;

struct RpcSession::RpcSessionFeild
{
	boost::array<char, 8192> buffer;
	boost::asio::ip::tcp::socket socket;
	RpcSessionSet& sessions;

	RpcSessionFeild();
	~RpcSessionFeild();
};

RpcSession::RpcSession(): rpc_session(new RpcSessionFeild())
{
}

boost::asio::ip::tcp::socket& RpcSession::Socket()
{
	return rpc_session->socket;
}

void RpcSession::Start()
{
	boost::asio::async_read(boost::asio::buffer(rpc_session->buffer),
			boost::bind(&RpcSession::HandleAsyncRead, shared_from_this(),
					boost::asio::placeholders::bytes_transferred,
					boost::asio::placeholders::error));
}

void RpcSession::HandleAsyncRead(std::size_t bytes_transferred,
		const boost::system::error_code& err)
{
	if (!err)
	{
		bool result;
		result = request_parser.parse();

		if (result)
		{

		}
		else
		{
			boost::asio::async_read(boost::asio::buffer(buffer),
					boost::bind(&RpcSession::HandleAsyncRead, shared_from_this(),
							boost::asio::placeholders::bytes_transferred,
							boost::asio::placeholders::error));
		}
	}
	else if (err != boost::asio::error::operation_aborted)
	{
		rpc_session->sessions.erase(shared_from_this());
	}
}

void RpcSession::Stop()
{
	rpc_session->socket.close();
}

}
