#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Rpc/RpcChannel.h"
#include "Thread/CountBarrier.h"
#include "Thread/ThreadPool.h"
#include "Rpc/RpcController.h"
#include "Rpc/Echo.pb.h"

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
		// 接收消息
		RpcRequest rpc_request;
		rpc_request.ParseFromString(*ss);
		EchoRequest request;
		request.ParseFromString(rpc_request.request());

		num_ = request.num();
		VLOG(2) << "num: " << num_;

		// 回一个消息
		RpcResponse rpc_response;
		rpc_response.set_id(rpc_request.id());
		rpc_response.set_type(RESPONSE_TYPE_OK);

		EchoResponse response;
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

public:
	RpcChannelTest(): barrier_(2)
	{
	}
};


TEST_F(RpcChannelTest, CallMethod)
{
	boost::asio::io_service io_service;
	RpcServerTest rpc_server(io_service, global_port, barrier_);

	RpcChannel rpc_channel(io_service, "127.0.0.1", global_port);
	EchoService_Stub service(&rpc_channel);

	ThreadPool thread_pool(2);
	thread_pool.PushTask(boost::bind(&boost::asio::io_service::run, &io_service));

	EchoRequest request;
	request.set_num(100);
	EchoResponse response;

	ASSERT_EQ(0, response.num());
	service.Echo(NULL, &request, &response,
			google::protobuf::NewCallback(&barrier_, &CountBarrier::Signal));

	barrier_.Wait();
	rpc_channel.Stop();
	rpc_server.Stop();
	io_service.stop();
	thread_pool.Wait();

	ASSERT_EQ(100, response.num());
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
