#include <boost/function.hpp>
#include <boost/bind.hpp>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "thread/thread_pool.h"

namespace hainan
{
	class ThreadPoolTest: public testing::Test
	{
		void SetUp()
		{
			thread_pool.SetNum(10);
			thread_pool.Start();
		}
		void TearDown()
		{
		}
	protected:
		ThreadPool thread_pool;
	public:
		ThreadPoolTest(): thread_pool()
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
		for(int i = 0; i < 100; ++i)
		{
			thread_pool.PushTask(
				bind(&ThreadPoolTest::Max,
					this, x[i], y[i], &z[i]));
		}
		thread_pool.Stop();
		for(int i = 0; i < 100; ++i)
		{
			ASSERT_EQ(9, z[i]) << "i = " << i;
		}
	}
}

int main(int argc, char* argv[])
{
	FLAGS_logtostderr = true;
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	testing::InitGoogleTest(&argc, argv);
	return RUN_ALL_TESTS();
}
