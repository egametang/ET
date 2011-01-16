#ifndef THREAD_THREAD_POOL_H
#define THREAD_THREAD_POOL_H

#include <list>
#include <boost/thread.hpp>
#include <boost/function.hpp>
#include <boost/detail/atomic_count.hpp>
#include "Base/Base.h"

namespace Hainan {

class ThreadPool: private boost::noncopyable
{
private:
	int thread_num;
	boost::detail::atomic_count work_num;
	volatile bool running;
	boost::mutex mutex;
	boost::condition_variable cond;
	boost::condition_variable done;
	std::list<ThreadPtr> threads;
	std::list<boost::function<void (void)> > tasks;

	void Runner();
public:
	ThreadPool(int num = 0);
	~ThreadPool();
	void Start();
	void Stop();
	bool PushTask(boost::function<void (void)> task);
};

} // namespace Hainan
#endif // THREAD_THREAD_POOL_H
