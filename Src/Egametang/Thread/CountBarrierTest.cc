#include <boost/bind.hpp>
#include <boost/ref.hpp>
#include <boost/detail/atomic_count.hpp>
#include <gtest/gtest.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include "Thread/ThreadPool.h"
#include "Thread/CountBarrier.h"

namespace Egametang {

class CountBarrierTest: public testing::Test
{
protected:
	boost::detail::atomic_count count_;
public:
	CountBarrierTest():
		count_(0)
	{
	}
	void Wait(CountBarrier& barrier)
	{
		barrier.Wait();
		++count_;
		VLOG(3) << "count barrier test wait end";
	}
	void Signal(CountBarrier& barrier)
	{
		barrier.Signal();
	}
};

TEST_F(CountBarrierTest, Count)
{
	CountBarrier barrier(10);
	ASSERT_EQ(10, barrier.Count());
}

TEST_F(CountBarrierTest, WaitAndSignal)
{
	CountBarrier barrier(10);
	ThreadPool pool(11);
	for (int i = 0; i < 10; ++i)
	{
		pool.PushTask(
				boost::bind(&CountBarrierTest::Wait,
						this, boost::ref(barrier)));
	}
	ASSERT_EQ(0, this->count_);
	for (int i = 0; i < 10; ++i)
	{
		pool.PushTask(
				boost::bind(&CountBarrierTest::Signal,
						this, boost::ref(barrier)));
	}
	pool.Stop();
	ASSERT_EQ(10, this->count_);
}

} // namespace Egametang

int main(int argc, char* argv[])
{
	FLAGS_logtostderr = true;
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}

