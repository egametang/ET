#include <boost/foreach.hpp>
#include "thread/thread_pool.h"

namespace hainan
{
	ThreadPool::ThreadPool():
			num(0), work_num(0),
			running(false)
	{
	}
	ThreadPool::~ThreadPool()
	{
		Stop();
	}

	void ThreadPool::Start()
	{
		running = true;
		for(int i = 0; i < num; ++i)
		{
			shared_ptr<thread> t(new thread(bind(&ThreadPool::Loop, ref(this))));
			threads.push_back(t);
			t->detach();
		}
		work_num = num;
	}

	void ThreadPool::Stop()
	{
		mutex::scoped_lock lock(mtx);
		running = false;
		cond.notify_all();
		while(work_num != 0)
		{
			done.wait(lock);
		}
	}

	void ThreadPool::Loop()
	{
		while(running)
		{
			function<void (void)> task;
			{
				mutex::scoped_lock lock(mtx);
				while(tasks.empty())
				{
					cond.wait(lock);
				}
				task = tasks.front();
				tasks.pop_front();
				cond.notify_one();
			}
			task();
		}
		if(__sync_sub_and_fetch(&work_num, 1) == 0)
		{
			done.notify_one();
		}
	}

	bool ThreadPool::PushTask(function<void (void)> task)
	{
		mutex::scoped_lock lock(mtx);
		if(!running)
		{
			return false;
		}
		tasks.push_back(task);
		cond.notify_one();
		return true;
	}

	void ThreadPool::SetNum(int32_t n)
	{
		num = n;
	}
}
