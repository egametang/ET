#include "thread/thread_pool.h"

namespace hainan
{
	ThreadPool::ThreadPool(int32_t n):
			num(n), running(false)
	{
	}

	void ThreadPool::Start()
	{
		for(int i = 0; i < num; ++i)
		{
			boost::thread t(boost::function(&ThreadPool::Loop, boost::ref(this)));
			threads.push_back(t);
			t.detach();
		}
		running = true;
	}

	void ThreadPool::Stop()
	{
		running = false;
		cond.notify_all();
	}

	void ThreadPool::Loop()
	{
		while(running)
		{
			mutex.lock();
			while(tasks.empty())
			{
				cond.wait(mutex);
			}
			boost::function& t = tasks.front();
			tasks.pop_front();
			cond.notify_one();
			mutex.unlock();
			t();
		}
	}

	bool ThreadPool::PushTask(boost::function<void(void)> task)
	{
		boost::mutex::scoped_lock(&mutex);
		if(!running)
		{
			return false;
		}
		tasks.push_back(task);
		cond.notify_one();
		return true;
	}
}
