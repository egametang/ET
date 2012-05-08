#include <functional>
#include <boost/threadpool.hpp>
#include <gtest/gtest.h>
#include <glog/logging.h>
#include <gflags/gflags.h>

namespace Egametang {

class ThreadPoolTest: public testing::Test
{
};

static void Max(int a, int b, int* z)
{
	*z = a > b? a : b;
}

TEST_F(ThreadPoolTest, Test1)
{
	boost::threadpool::fifo_pool pool(10);
	std::vector<int> x(100, 8);
	std::vector<int> y(100, 9);
	std::vector<int> z(100, 0);
	for (int i = 0; i < 100; ++i)
	{
		pool.schedule(std::bind(&Max, x[i], y[i], &z[i]));
	}
	pool.wait();
	for (int i = 0; i < 100; ++i)
	{
		ASSERT_EQ(9, z[i]);
	}
}

} // namespace Egametang

int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}
