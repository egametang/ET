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
		RecvMeta();
	}

	void Stop()
	{
		acceptor_.close();
		socket_.close();
	}

	virtual void OnRecvMessage(RpcMetaPtr meta, StringPtr message)
	{
		// 接收消息
		EchoRequest request;
		request.ParseFromString(*message);

		num_ = request.num();

		// 回一个消息
		EchoResponse response;
		response.set_num(num_);
		std::string send_string = response.SerializeAsString();

		RpcMeta response_meta;
		response_meta.id = meta->id;
		response_meta.size = send_string.size();
		SendMeta(response_meta, send_string);
	}
	virtual void OnSendMessage()
	{
		barrier_.Signal();
	}
};

class RpcChannelTest: public testing::Test
{
};

static void IOServiceRun(boost::asio::io_service& io_service)
{
	io_service.run();
}

TEST_F(RpcChannelTest, Echo)
{
	boost::asio::io_service io_server;
	boost::asio::io_service io_client;

	CountBarrier barrier(2);
	RpcServerTest rpc_server(io_server, global_port, barrier);

	RpcChannel rpc_channel(io_client, "127.0.0.1", global_port);
	EchoService_Stub service(&rpc_channel);

	ThreadPool thread_pool(2);
	thread_pool.PushTask(boost::bind(&IOServiceRun, boost::ref(io_server)));
	thread_pool.PushTask(boost::bind(&IOServiceRun, boost::ref(io_client)));

	EchoRequest request;
	request.set_num(100);

	EchoResponse response;

	ASSERT_EQ(0, response.num());
	service.Echo(NULL, &request, &response,
			google::protobuf::NewCallback(&barrier, &CountBarrier::Signal));
	barrier.Wait();
	rpc_channel.Stop();
	rpc_server.Stop();
	io_server.stop();
	io_client.stop();
	// rpc_channel是个无限循环的操作, 必须主动让channel和server stop才能wait线程
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
