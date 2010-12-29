#include <boost/function.hpp>
#include <boost/bind.hpp>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Thread/ThreadPool.h"

namespace Hainan {

class ThreadPoolTest: public testing::Test
{
protected:
	ThreadPool pool;
public:
	ThreadPoolTest() : pool(10)
	{
	}
	void Max(int a, int b, int* z)
	{
		*z = a > b? a : b;
	}
};

TEST_F(ThreadPoolTest, Test1)
{
	pool.Start();
	std::vector<int> x(100, 8);
	std::vector<int> y(100, 9);
	std::vector<int> z(100, 0);
	for (int i = 0; i < 100; ++i)
	{
		pool.PushTask(
				boost::bind(&ThreadPoolTest::Max, this, x[i], y[i], &z[i]));
	}
	pool.Stop();
	for (int i = 0; i < 100; ++i)
	{
		ASSERT_EQ(9, z[i])<< "i = " << i;
	}
}
} // namespace Hainan

int main(int argc, char* argv[])
{
	FLAGS_logtostderr = true;
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
