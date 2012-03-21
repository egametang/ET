#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/function.hpp>
#include <boost/threadpool.hpp>
#include <boost/make_shared.hpp>
#include <gtest/gtest.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include <google/protobuf/service.h>
#include "Thread/CountBarrier.h"
#include "Rpc/RpcClient.h"
#include "Rpc/RpcServer.h"
#include "Rpc/RpcSession.h"
#include "Rpc/RpcServerMock.h"
#include "Rpc/Echo.pb.h"

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

static void IOServiceRun(boost::asio::io_service* ioService)
{
	ioService->run();
}

class RpcServerTest: public testing::Test
{
protected:
	boost::asio::io_service ioClient;
	boost::asio::io_service ioServer;
	int port;

public:
	RpcServerTest(): ioClient(), ioServer(), port(10003)
	{
	}

	virtual ~RpcServerTest()
	{
	}

	MethodMap& GetMethodMap(RpcServerPtr server)
	{
		return server->methods;
	}
};

TEST_F(RpcServerTest, ClientAndServer)
{
	boost::threadpool::fifo_pool threadPool(2);

	auto echoSevice = boost::make_shared<MyEcho>();

	auto server = boost::make_shared<RpcServer>(ioServer, port);
	// 注册service
	server->Register(echoSevice);
	ASSERT_EQ(1U, GetMethodMap(server).size());

	auto client = boost::make_shared<RpcClient>(ioClient, "127.0.0.1", port);
	EchoService_Stub service(client.get());

	// 定义消息
	EchoRequest request;
	request.set_num(100);
	EchoResponse response;
	ASSERT_EQ(0U, response.num());

	// server和client分别在两个不同的线程
	threadPool.schedule(boost::bind(&IOServiceRun, &ioServer));
	// 等待server OK
	boost::this_thread::sleep(boost::posix_time::milliseconds(100));
	threadPool.schedule(boost::bind(&IOServiceRun, &ioClient));

	CountBarrier barrier;
	service.Echo(NULL, &request, &response,
			google::protobuf::NewCallback(&barrier, &CountBarrier::Signal));
	barrier.Wait();

	// 加入到io线程
	ioClient.post(boost::bind(&RpcClient::Stop, client));
	ioServer.post(boost::bind(&RpcServer::Stop, server));

	// 加入任务队列,等client和server stop,io_service才stop
	ioClient.post(boost::bind(&boost::asio::io_service::stop, &ioClient));
	ioServer.post(boost::bind(&boost::asio::io_service::stop, &ioServer));

	// 必须主动让client和server stop才能wait线程
	threadPool.wait();

	ASSERT_EQ(100, response.num());
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}
