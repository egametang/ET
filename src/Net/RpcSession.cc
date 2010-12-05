#include "Net/RpcSession.h"

namespace Hainan {

RpcSession::RpcSession()
{
}

boost::asio::ip::tcp::socket& RpcSession::Socket()
{
	return socket;
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
		sessions.erase(shared_from_this());
	}
}

void RpcSession::Start()
{
	boost::asio::async_read(boost::asio::buffer(buffer),
			boost::bind(&RpcSession::HandleAsyncRead, shared_from_this(),
					boost::asio::placeholders::bytes_transferred,
					boost::asio::placeholders::error));
}

void RpcSession::Stop()
{
	socket.close();
}

}
