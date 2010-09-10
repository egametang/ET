#include <glog/logging.h>
#include "thread/thread_pool.h"

namespace hainan
{
	ThreadPool::ThreadPool():
			num(0), running(false), work_num(0)
	{
	}
	ThreadPool::~ThreadPool()
	{
	}

	void ThreadPool::Start()
	{
		running = true;
		for(int i = 0; i < num; ++i)
		{
			shared_ptr<thread> t(new thread(bind(&ThreadPool::Runner, this)));
			threads.push_back(t);
			t->detach();
		}
		work_num = num;
	}

	void ThreadPool::Stop()
	{
		VLOG(3) << "Stop";
		mutex::scoped_lock lock(mtx);
		running = false;
		cond.notify_all();
		while(work_num > 0)
		{
			VLOG(3) << "done tasks size = " << tasks.size();
			done.wait(lock);
		}
	}

	void ThreadPool::Runner()
	{
		VLOG(3) << "thread start";
		bool continued = true;
		while(continued)
		{
			function<void (void)> task;
			{
				VLOG(3) << "loop lock";
				mutex::scoped_lock lock(mtx);
				VLOG(3) << "loop lock ok";
				while(running && tasks.empty())
				{
					cond.wait(lock);
					VLOG(3) << "cond";
				}
				if(!tasks.empty())
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

			if(task)
			{
				task();
			}
		}
		if(__sync_sub_and_fetch(&work_num, 1) == 0)
		{
			VLOG(3) << "work_num = " << work_num;
			done.notify_one();
		}
	}

	bool ThreadPool::PushTask(function<void (void)> task)
	{
		VLOG(3) << "push task";
		{
			mutex::scoped_lock lock(mtx);
			if(!running)
			{
				return false;
			}
			tasks.push_back(task);
		}
		VLOG(3) << "push task unlock";
		cond.notify_one();
		return true;
	}

	void ThreadPool::SetNum(int n)
	{
		num = n;
	}
}
