#ifndef THREAD_THREAD_POOL_H
#define THREAD_THREAD_POOL_H

#include <vector>
#include <list>
#include <boost/thread.hpp>
#include <boost/function.hpp>
#include <boost/bind.hpp>
#include <boost/smart_ptr.hpp>

namespace hainan {

typedef boost::shared_ptr<boost::thread> thread_ptr;

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
	std::list<boost::function<void(void)> > tasks_;

	void runner();
public:
	thread_pool();
	~thread_pool();
	void start();
	void stop();
	void set_num(int n);
	bool push_task(boost::function<void(void)> task);
};
} // namespace hainan
#endif  // THREAD_THREAD_POOL_H
