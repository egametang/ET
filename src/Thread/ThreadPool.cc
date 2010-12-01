#include <boost/detail/atomic_count.hpp>
#include <glog/logging.h>
#include "Thread/ThreadPool.h"

namespace Hainan {

ThreadPool::ThreadPool(int num) :
	thread_num(num? num : boost::thread::hardware_concurrency()),
	work_num(thread_num), running(false), work_num(0)
{
}

ThreadPool::~ThreadPool()
{
}

void ThreadPool::Start()
{
	running = true;
	for (int i = 0; i < thread_num; ++i)
	{
		thread_ptr t(new boost::thread(
				boost::bind(&ThreadPool::Runner, this)));
		threads.push_back(t);
		t->detach();
	}
}

void ThreadPool::Stop()
{
	VLOG(3)<< "Stop";
	boost::mutex::scoped_lock lock(mutex);
	running = false;
	cond.notify_all();
	while (work_num > 0)
	{
		VLOG(3) << "done tasks size = " << tasks.size();
		done.wait(lock);
	}
}

void ThreadPool::Runner()
{
	VLOG(3) << "thread start";
	bool continued = true;
	while (continued)
	{
		boost::function<void (void)> task;
		{
			VLOG(3) << "loop lock";
			boost::mutex::scoped_lock lock(mutex);
			VLOG(3) << "loop lock ok";
			while (running && tasks.empty())
			{
				cond.wait(lock);
				VLOG(3) << "cond";
			}
			if (!tasks.empty())
			{
				VLOG(3) << "fetch task";
				task = tasks.front();
				tasks.pop_front();
			}
			continued = running || !tasks.empty();
			VLOG(3) << "continued = " << continued
			<< "running = " << running
			<< " tasks size = " << tasks.size();
			VLOG(3) << "loop unlock";
		}

		if (task)
		{
			task();
		}
	}
	if (--work_num == 0)
	{
		VLOG(3) << "work_num = " << work_num;
		done.notify_one();
	}
}

bool ThreadPool::PushTask(boost::function<void (void)> task)
{
	VLOG(3) << "push task";
	{
		boost::mutex::scoped_lock lock(mutex);
		if (!running)
		{
			return false;
		}
		tasks.push_back(task);
	}
	VLOG(3) << "push task unlock";
	cond.notify_one();
	return true;
}

} // namespace Hainan
