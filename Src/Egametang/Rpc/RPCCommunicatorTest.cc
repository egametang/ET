#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Rpc/RPCCommunicator.h"

namespace Egametang {

static int port = 10001;

class RPCServerTest: public RPCCommunicator
{
public:
	StringPtr recv_string_;

	int send_id_;
	RpcHandlerPtr send_handler_;

	boost::asio::io_service& io_service_;
	boost::asio::ip::tcp::acceptor acceptor_;

	boost::asio::ip::tcp::socket server_socket;

public:
	RPCServerTest(boost::asio::io_service& io_service, int port)
	{
		boost::asio::ip::address address;
		address.from_string("localhost");
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
	}

	void Start()
	{
		IntPtr size(new int);
		boost::asio::async_read(socket_,
				boost::asio::buffer(reinterpret_cast<char*>(size.get()), sizeof(int)),
				boost::bind(&RPCCommunicator::RecvMessage, this, size,
						boost::asio::placeholders::error));
	}

	virtual void OnRecvMessage(StringPtr ss)
	{
		recv_string_ = ss;
	}
	virtual void OnSendMessage(int32 id, RpcHandlerPtr handler)
	{
		send_id_ = id;
		send_handler_ = handler;
	}
};

class RPCClientTest: public RPCCommunicator
{
public:
	StringPtr recv_string_;
	int send_id_;
	RpcHandlerPtr send_handler_;

	boost::asio::io_service& io_service_;

public:
	RPCClientTest(boost::asio::io_service& io_service, std::string& host, int port):
		io_service_(io_service)
	{
		boost::asio::ip::address address;
		address.from_string(host);
		boost::asio::ip::tcp::endpoint endpoint(address, port_);
		socket_.async_connect(endpoint,
				boost::bind(&RPCClientTest::OnAsyncConnect, this,
						boost::asio::placeholders::error));
	}

	void Start()
	{
		io_service_.run();
	}

	void OnAsyncConnect()
	{
	}

	void SendString()
	{

		req->set_request(request->SerializeAsString());
		RpcHandlerPtr handler(new RpcHandler(controller, response, done));
		SendSize(req, handler);
	}

	virtual void OnRecvMessage(StringPtr ss)
	{
		recv_string_ = ss;
	}

	virtual void OnSendMessage(int32 id, RpcHandlerPtr handler)
	{
		send_id_ = id;
		send_handler_ = handler;
	}
};

class RPCCommunicatorTest: public testing::Test
{
protected:
	RPCServerTest rpc_server_;
	RPCClientTest rpc_client_;
};


TEST_F(RPCCommunicatorTest, CallMethod)
{
	RpcServerTest server(io_service_, port);
	ASSERT_EQ(0, server.size);

	RpcChannel channel(io_service_, "localhost", port);
	channel.CallMethod(NULL, NULL, request, response_, done_);

	ASSERT_EQ(request.ByteSize(), server.size);
}

} // namespace Egametang
