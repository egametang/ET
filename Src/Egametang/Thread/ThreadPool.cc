#include <glog/logging.h>
#include "Thread/ThreadPool.h"

namespace Egametang {

ThreadPool::ThreadPool(int num) :
	thread_num_(num), running_(true), work_num_(0)
{
	if (num == 0)
	{
		thread_num_ = boost::thread::hardware_concurrency();
	}
	for (int i = 0; i < thread_num_; ++i)
	{
		ThreadPtr t(new boost::thread(
				boost::bind(&ThreadPool::Runner, this)));
		threads_.push_back(t);
		t->detach();
		++work_num_;
	}
}

ThreadPool::~ThreadPool()
{
}

void ThreadPool::Wait()
{
	boost::mutex::scoped_lock lock(mutex_);
	running_ = false;
	cond_.notify_all();
	while (work_num_ > 0)
	{
		done_.wait(lock);
	}
	running_ = true;
}

void ThreadPool::Runner()
{
	bool continued = true;
	while (continued)
	{
		boost::function<void (void)> task;
		{
			boost::mutex::scoped_lock lock(mutex_);
			while (running_ && tasks_.empty())
			{
				cond_.wait(lock);
			}
			if (!tasks_.empty())
			{
				task = tasks_.front();
				tasks_.pop_front();
			}
			continued = running_ || !tasks_.empty();
		}

		if (task)
		{
			task();
		}
	}
	if (--work_num_ == 0)
	{
		done_.notify_one();
	}
}

bool ThreadPool::Schedule(boost::function<void (void)> task)
{
	{
		boost::mutex::scoped_lock lock(mutex_);
		if (!running_)
		{
			return false;
		}
		tasks_.push_back(task);
	}
	cond_.notify_one();
	return true;
}

} // namespace Egametang
