#include "Thread/CountBarrier.h"

namespace Egametang {

CountBarrier::CountBarrier(int count):
	count(count), mutex(), condition()
{
}

void CountBarrier::Wait()
{
	boost::mutex::scoped_lock lock(mutex);
	while (count > 0)
	{
		condition.wait(lock);
	}
}

void CountBarrier::Signal()
{
	boost::mutex::scoped_lock lock(mutex);
	--count;
	if (count == 0)
	{
		condition.notify_all();
	}
}

int CountBarrier::Count() const
{
	boost::mutex::scoped_lock lock(mutex);
	return count;
}

void CountBarrier::Reset(int count)
{
	this->count = count;
}

} // namespace Egametang
