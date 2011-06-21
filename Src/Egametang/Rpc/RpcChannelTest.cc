#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Rpc/RpcChannel.h"

namespace Egametang {

static int port = 10001;

class RPCServerTest: public RPCCommunicator
{
public:
	CountBarrier& barrier_;
	std::string recv_string_;
	boost::asio::ip::tcp::acceptor acceptor_;

public:
	RPCServerTest(boost::asio::io_service& io_service, int port, CountBarrier& barrier):
		RPCCommunicator(io_service), acceptor_(io_service),
		barrier_(barrier)
	{
		boost::asio::ip::address address;
		address.from_string("127.0.0.1");
		boost::asio::ip::tcp::endpoint endpoint(address, port);
		acceptor_.open(endpoint.protocol());
		acceptor_.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
		acceptor_.bind(endpoint);
		acceptor_.listen();
		acceptor_.async_accept(socket_,
				boost::bind(&RPCServerTest::OnAsyncAccept, this,
						boost::asio::placeholders::error));
	}

	void OnAsyncAccept(const boost::system::error_code& err)
	{
		if (err)
		{
			LOG(ERROR) << "async accept failed: " << err.message();
			return;
		}
		RecvSize();
	}

	void Start()
	{
		VLOG(2) << "Start Server";
		io_service_.run();
	}

	void Stop()
	{
		acceptor_.close();
		socket_.close();
	}

	virtual void OnRecvMessage(StringPtr ss)
	{
		VLOG(2) << "Server Recv string: " << *ss;
		recv_string_ = *ss;
		std::string send_string("response test rpc communicator string");
		SendSize(send_string.size(), send_string);
		barrier_.Signal();
	}
	virtual void OnSendMessage()
	{
	}
};

class RpcChannelTest: public testing::Test
{
private:
	boost::asio::io_service io_server_;
	boost::asio::io_service io_client_;
	RPCServerTest rpc_server_;

public:
	RpcChannelTest(): rpc_server_()
	{
	}
};


TEST_F(RpcChannelTest, CallMethod)
{
	RpcServerTest server(io_service_, port);
	ASSERT_EQ(0, server.size);

	RpcChannel channel(io_service_, "localhost", port);
	channel.CallMethod(NULL, NULL, request, response_, done_);

	ASSERT_EQ(request.ByteSize(), server.size);
}

} // namespace Egametang
