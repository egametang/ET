#ifndef THREAD_THREADPOOL_H
#define THREAD_THREADPOOL_H

#include <list>
#include <boost/thread.hpp>
#include <boost/function.hpp>
#include <boost/detail/atomic_count.hpp>
#include "Thread/ThreadTypedef.h"
#include "Base/Marcos.h"

namespace Egametang {

class ThreadPool: private boost::noncopyable
{
private:
	int thread_num_;
	boost::detail::atomic_count work_num_;
	volatile bool running_;
	boost::mutex mutex_;
	boost::condition_variable cond_;
	boost::condition_variable done_;
	std::list<ThreadPtr> threads_;
	std::list<boost::function<void (void)> > tasks_;

	void Runner();
public:
	ThreadPool(int num = 0);
	virtual ~ThreadPool();

	virtual void Wait();
	virtual bool PushTask(boost::function<void (void)> task);
};

} // namespace Egametang
#endif // THREAD_THREADPOOL_H
