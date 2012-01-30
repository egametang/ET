#include "Thread/ThreadPool.h"

namespace Egametang {

ThreadPool::ThreadPool(int num) :
	threadNum(num), running(true), workNum(0)
{
	if (num == 0)
	{
		threadNum = boost::thread::hardware_concurrency();
	}
	for (int i = 0; i < threadNum; ++i)
	{
		ThreadPtr t(new boost::thread(
				boost::bind(&ThreadPool::Runner, this)));
		threads.push_back(t);
		t->detach();
		++workNum;
	}
}

ThreadPool::~ThreadPool()
{
	boost::mutex::scoped_lock lock(mutex);
	running = false;
	cond.notify_all();
	while (workNum > 0)
	{
		done.wait(lock);
	}
}

void ThreadPool::Wait()
{
	boost::mutex::scoped_lock lock(mutex);
	running = false;
	cond.notify_all();
	while (workNum > 0)
	{
		done.wait(lock);
	}
}

void ThreadPool::Runner()
{
	bool continued = true;
	while (continued)
	{
		boost::function<void (void)> task;
		{
			boost::mutex::scoped_lock lock(mutex);
			while (running && tasks.empty())
			{
				cond.wait(lock);
			}
			if (!tasks.empty())
			{
				task = tasks.front();
				tasks.pop_front();
			}
			continued = running || !tasks.empty();
		}

		if (task)
		{
			task();
		}
	}
	if (--workNum == 0)
	{
		done.notify_one();
	}
}

bool ThreadPool::Schedule(boost::function<void (void)> task)
{
	{
		boost::mutex::scoped_lock lock(mutex);
		if (!running)
		{
			return false;
		}
		tasks.push_back(task);
	}
	cond.notify_one();
	return true;
}

} // namespace Egametang
