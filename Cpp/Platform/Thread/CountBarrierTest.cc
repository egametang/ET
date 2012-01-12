#include <boost/bind.hpp>
#include <boost/ref.hpp>
#include <boost/detail/atomic_count.hpp>
#include <boost/date_time.hpp>
#include <gtest/gtest.h>
#include "Thread/ThreadPool.h"
#include "Thread/CountBarrier.h"

namespace Egametang {

class CountBarrierTest: public testing::Test
{
protected:
	boost::detail::atomic_count count;
public:
	CountBarrierTest(): count(0)
	{
	}
	virtual ~CountBarrierTest()
	{
	}

	void Wait(CountBarrier& barrier)
	{
		barrier.Wait();
	}
	void Signal(CountBarrier& barrier)
	{
		boost::xtime xt;
		boost::xtime_get(&xt, boost::TIME_UTC);
		xt.sec += 1;
		boost::thread::sleep(xt);
		++count;
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
	ThreadPool pool(10);
	for (int i = 0; i < 10; ++i)
	{
		pool.Schedule(
				boost::bind(&CountBarrierTest::Signal,
						this, boost::ref(barrier)));
	}
	ASSERT_EQ(0, this->count);
	barrier.Wait();
	ASSERT_EQ(10, this->count);
	pool.Wait();
}

} // namespace Egametang

int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	return RUN_ALL_TESTS();
}

