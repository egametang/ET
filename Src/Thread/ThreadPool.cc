#include <glog/logging.h>
#include "Thread/ThreadPool.h"

namespace Egametang {

ThreadPool::ThreadPool(int num) :
	thread_num_(num), running_(false), work_num_(0)
{
	if (num == 0)
	{
		thread_num_ = boost::thread::hardware_concurrency();
	}
}

ThreadPool::~ThreadPool()
{
}

void ThreadPool::Start()
{
	running_ = true;
	for (int i = 0; i < thread_num_; ++i)
	{
		ThreadPtr t(new boost::thread(
				boost::bind(&ThreadPool::Runner, this)));
		threads_.push_back(t);
		t->detach();
		++work_num_;
	}
}

void ThreadPool::Stop()
{
	VLOG(3)<< "Stop";
	boost::mutex::scoped_lock lock(mutex_);
	running_ = false;
	cond_.notify_all();
	while (work_num_ > 0)
	{
		VLOG(3) << "done tasks size = " << tasks_.size();
		done_.wait(lock);
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
			boost::mutex::scoped_lock lock(mutex_);
			VLOG(3) << "loop lock ok";
			while (running_ && tasks_.empty())
			{
				cond_.wait(lock);
				VLOG(3) << "cond";
			}
			if (!tasks_.empty())
			{
				VLOG(3) << "fetch task";
				task = tasks_.front();
				tasks_.pop_front();
			}
			continued = running_ || !tasks_.empty();
			VLOG(3) << "continued = " << continued
			<< "running = " << running_
			<< " tasks size = " << tasks_.size();
			VLOG(3) << "loop unlock";
		}

		if (task)
		{
			task();
		}
	}
	if (--work_num_ == 0)
	{
		VLOG(3) << "work_num = " << work_num_;
		done_.notify_one();
	}
}

bool ThreadPool::PushTask(boost::function<void (void)> task)
{
	VLOG(3) << "push task";
	{
		boost::mutex::scoped_lock lock(mutex_);
		if (!running_)
		{
			return false;
		}
		tasks_.push_back(task);
	}
	VLOG(3) << "push task unlock";
	cond_.notify_one();
	return true;
}

} // namespace Egametang
