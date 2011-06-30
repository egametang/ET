#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Rpc/RpcChannel.h"
#include "Rpc/RpcProtobufData.pb.h"
#include "Rpc/RpcChannelTest.pb.h"

namespace Egametang {

static int global_port = 10002;

class RpcServerTest: public RpcCommunicator
{
public:
	CountBarrier& barrier_;
	int32 num_;
	boost::asio::ip::tcp::acceptor acceptor_;

public:
	RpcServerTest(boost::asio::io_service& io_service, int port, CountBarrier& barrier):
		RpcCommunicator(io_service), acceptor_(io_service),
		barrier_(barrier), num_(0)
	{
		boost::asio::ip::address address;
		address.from_string("127.0.0.1");
		boost::asio::ip::tcp::endpoint endpoint(address, port);
		acceptor_.open(endpoint.protocol());
		acceptor_.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
		acceptor_.bind(endpoint);
		acceptor_.listen();
		acceptor_.async_accept(socket_,
				boost::bind(&RpcServerTest::OnAsyncAccept, this,
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

	void Stop()
	{
		acceptor_.close();
		socket_.close();
	}

	virtual void OnRecvMessage(StringPtr ss)
	{
		VLOG(2) << "Server Recv string: " << *ss;

		// 接收消息
		RpcRequest rpc_request;
		rpc_request.ParseFromString(*ss);
		RpcChannelTestRequest request;
		request.ParseFromString(rpc_request.request());

		num_ = request.num();

		// 回一个消息
		RpcResponse rpc_response;
		rpc_response.set_id(rpc_request.id());
		rpc_response.set_type(ResponseType::RESPONSE_TYPE_OK);

		RpcChannelTestResponse response;
		response.set_num(num_);
		rpc_response.set_response(response.SerializeAsString());

		std::string send_string = rpc_response.SerializeAsString();
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
	CountBarrier barrier_;
	boost::asio::io_service io_service_;
	RpcServerTest rpc_server_;

public:
	RpcChannelTest():
		rpc_server_(io_service_, global_port, barrier_)
	{
	}
};


TEST_F(RpcChannelTest, CallMethod)
{
	RpcChannel rpc_channel(io_service_, "127.0.0.1", global_port);

	RpcChannelTestService service(rpc_channel);

	RpcChannelTestRequest request;
	request.set_num(100);
	RpcChannelTestResponse response;
	RpcController controller;

	ASSERT_EQ(0, response.num());
	service.Echo(&controller, &request, &response,
			google::protobuf::NewCallback(&barrier_, &CountBarrier::Signal));

	io_service_.run();
	barrier_.Wait();

	ASSERT_EQ(100, response.num());
}

} // namespace Egametang
