#include <boost/function.hpp>
#include <boost/bind.hpp>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "thread/thread_pool.h"

namespace hainan {

class thread_pool_test: public testing::Test
{
	void SetUp()
	{
		pool.set_num(10);
		pool.start();
	}
	void TearDown()
	{
	}
protected:
	thread_pool pool;
public:
	thread_pool_test() :
		pool()
	{
	}
	void max(int a, int b, int* z)
	{
		*z = a > b? a : b;
	}
};

TEST_F(thread_pool_test, Test1)
{
	vector<int> x(100, 8);
	vector<int> y(100, 9);
	vector<int> z(100, 0);
	for (int i = 0; i < 100; ++i)
	{
		pool.push_task(
				bind(&thread_pool_test::max, this, x[i], y[i], &z[i]));
	}
	pool.stop();
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
