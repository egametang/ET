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
			thread_pool.SetNum(4);
			thread_pool.Start();
		}
		void TearDown()
		{
		}
	protected:
		ThreadPool thread_pool;
		boost::function<int(int)> func;
	public:
		void Max(int a, int b, int* z)
		{
			LOG(INFO) << a << " " << b;
			*z = a > b? a : b;
		}
	};

	TEST_F(ThreadPoolTest, Test1)
	{
		int x = 5;
		int y = 6;
		int z = 0;
		int a = 7;
		int b = 8;
		int c = 0;
		thread_pool.PushTask(
				boost::bind(&ThreadPoolTest::Max, boost::ref(*this), x, y, &z));
		thread_pool.PushTask(
				boost::bind(&ThreadPoolTest::Max, boost::ref(*this), a, b, &c));
		thread_pool.Stop();
		ASSERT_EQ(6, z);
		ASSERT_EQ(8, c);
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
