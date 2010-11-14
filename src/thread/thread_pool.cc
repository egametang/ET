#include <glog/logging.h>
#include "thread/thread_pool.h"

namespace hainan {

thread_pool::thread_pool() :
	num_(0), running_(false), work_num_(0)
{
}
thread_pool::~thread_pool()
{
}

void thread_pool::start()
{
	running_ = true;
	for (int i = 0; i < num_; ++i)
	{
		thread_ptr t(new boost::thread(
				boost::bind(&thread_pool::runner, this)));
		threads_.push_back(t);
		t->detach();
	}
	work_num_ = num_;
}

void thread_pool::stop()
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

void thread_pool::runner()
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
	if (__sync_sub_and_fetch(&work_num_, 1) == 0)
	{
		VLOG(3) << "work_num = " << work_num_;
		done_.notify_one();
	}
}

bool thread_pool::push_task(boost::function<void (void)> task)
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

void thread_pool::set_num(int n)
{
	num_ = n;
}

} // namespace hainan
