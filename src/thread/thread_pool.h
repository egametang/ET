#ifndef THREAD_THREAD_POOL_H
#define THREAD_THREAD_POOL_H

#include <list>
#include <boost/thread.hpp>
#include <boost/function.hpp>
#include <boost/bind.hpp>
#include "base/base.h"

namespace hainan {

class thread_pool: private boost::noncopyable
{
private:
	int num_;
	volatile int work_num_;
	volatile bool running_;
	boost::mutex mutex_;
	boost::condition_variable cond_;
	boost::condition_variable done_;
	std::list<thread_ptr> threads_;
	std::list<boost::function<void (void)> > tasks_;

	void runner();
public:
	thread_pool();
	~thread_pool();
	void start();
	void stop();
	void set_num(int n);
	bool push_task(boost::function<void (void)> task);
};

typedef boost::shared_ptr<thread_pool> thread_pool_ptr;
} // namespace hainan
#endif // THREAD_THREAD_POOL_H
