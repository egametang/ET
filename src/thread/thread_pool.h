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
	int num;
	volatile int work_num;
	volatile bool running;
	boost::mutex mutex;
	boost::condition_variable cond;
	boost::condition_variable done;
	std::list<thread_ptr> threads;
	std::list<boost::function<void (void)> > tasks;

	void runner();
public:
	thread_pool();
	~thread_pool();
	void start();
	void stop();
	void set_num(int num);
	bool push_task(boost::function<void (void)> task);
};

typedef boost::shared_ptr<thread_pool> thread_pool_ptr;
} // namespace hainan
#endif // THREAD_THREAD_POOL_H
