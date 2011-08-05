#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/function.hpp>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include <google/protobuf/service.h>
#include "Thread/CountBarrier.h"
#include "Thread/ThreadPool.h"
#include "Rpc/RpcChannel.h"
#include "Rpc/RpcServer.h"
#include "Rpc/RpcSession.h"
#include "Rpc/RpcServerMock.h"
#include "Rpc/Echo.pb.h"
#include "Thread/ThreadPool.h"

namespace Egametang {

class MyEcho: public EchoService
{
public:
	virtual void Echo(
			google::protobuf::RpcController* controller,
			const EchoRequest* request,
			EchoResponse* response,
			google::protobuf::Closure* done)
	{
		int32 num = request->num();
		response->set_num(num);
		if (done)
		{
			done->Run();
		}
	}
};

static void IOServiceRun(boost::asio::io_service* io_service)
{
	VLOG(2) << __FUNCTION__;
	io_service->run();
}

class RpcServerTest: public testing::Test
{
protected:
	boost::asio::io_service io_client;
	int port;

public:
	RpcServerTest(): io_client(), port(10003)
	{
	}

	virtual ~RpcServerTest()
	{
	}
};

TEST_F(RpcServerTest, ChannelAndServer)
{
	ThreadPool thread_pool(2);

	RpcServicePtr echo_sevice(new MyEcho);
	RpcServer server(port);
	server.Register(echo_sevice);
	ASSERT_EQ(1U, server.methods.size());

	RpcChannel channel(io_client, "127.0.0.1", port);
	EchoService_Stub service(&channel);

	EchoRequest request;
	request.set_num(100);
	EchoResponse response;
	ASSERT_EQ(0U, response.num());

	thread_pool.Schedule(boost::bind(&RpcServer::Start, &server));
	thread_pool.Schedule(boost::bind(&IOServiceRun, &io_client));

	CountBarrier barrier;
	service.Echo(NULL, &request, &response,
			google::protobuf::NewCallback(&barrier, &CountBarrier::Signal));
	barrier.Wait();

	channel.Stop();
	server.Stop();
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
