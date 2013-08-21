#include <boost/detail/atomic_count.hpp>
#include <boost/date_time.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/make_shared.hpp>
#include <boost/ref.hpp>
#include <boost/foreach.hpp>
#include <boost/bind.hpp>
#include <gtest/gtest.h>
#include "Base/Marcos.h"
#include "Thread/CountBarrier.h"

namespace Egametang {

class CountBarrierTest: public testing::Test
{
};

TEST_F(CountBarrierTest, Count)
{
	CountBarrier barrier(10);
	ASSERT_EQ(10, barrier.Count());
}

static void Signal(CountBarrier& barrier, boost::detail::atomic_count& count)
{
	boost::this_thread::sleep(boost::posix_time::milliseconds(1000));
	++count;
	barrier.Signal();
}

TEST_F(CountBarrierTest, WaitAndSignal)
{
	boost::detail::atomic_count count(0);

	CountBarrier barrier(10);
	std::vector< boost::shared_ptr<boost::thread> > v(10);
	for (int i = 0; i < 10; ++i)
	{
		v[i].reset(new boost::thread(boost::bind(&Signal, boost::ref(barrier), boost::ref(count))));
	}
	barrier.Wait();
	ASSERT_EQ(10, count);
}

} // namespace Egametang

int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	return RUN_ALL_TESTS();
}

