#include <boost/function.hpp>
#include <boost/bind.hpp>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "thread/thread_pool.h"

namespace hainan {

class ThreadPoolTest: public testing::Test
{
	void SetUp()
	{
		thread_pool.set_num(10);
		thread_pool.start();
	}
	void TearDown()
	{
	}
protected:
	thread_pool thread_pool;
public:
	ThreadPoolTest() :
		thread_pool()
	{
	}
	void Max(int a, int b, int* z)
	{
		*z = a > b? a : b;
	}
};

TEST_F(ThreadPoolTest, Test1)
{
	vector<int> x(100, 8);
	vector<int> y(100, 9);
	vector<int> z(100, 0);
	for (int i = 0; i < 100; ++i)
	{
		thread_pool.push_task(
				bind(&ThreadPoolTest::Max, this, x[i], y[i], &z[i]));
	}
	thread_pool.stop();
	for (int i = 0; i < 100; ++i)
	{
		ASSERT_EQ(9, z[i])<< "i = " << i;
	}
}
} // namespace hainan

int main(int argc, char* argv[])
{
	FLAGS_logtostderr = true;
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
