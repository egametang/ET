#include "Thread/CountBarrier.h"

namespace Egametang {

CountBarrier::CountBarrier(int count):
	count_(count), mutex_(), condition_()
{
}

void CountBarrier::Wait()
{
	boost::mutex::scoped_lock lock(mutex_);
	while (count_ > 0)
	{
		condition_.wait(lock);
	}
}

void CountBarrier::Signal()
{
	boost::mutex::scoped_lock lock(mutex_);
	--count_;
	if (count_ == 0)
	{
		condition_.notify_all();
	}
}

int CountBarrier::Count() const
{
	boost::mutex::scoped_lock lock(mutex_);
	return count_;
}

} // namespace Egametang
