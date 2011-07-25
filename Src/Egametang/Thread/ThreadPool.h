#ifndef THREAD_THREADPOOL_H
#define THREAD_THREADPOOL_H

#include <list>
#include <boost/thread.hpp>
#include <boost/function.hpp>
#include <boost/noncopyable.hpp>
#include <boost/detail/atomic_count.hpp>
#include "Thread/ThreadTypedef.h"
#include "Base/Marcos.h"

namespace Egametang {

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
	virtual ~ThreadPool();

	virtual void Wait();
	virtual bool Schedule(boost::function<void (void)> task);
};

} // namespace Egametang
#endif // THREAD_THREADPOOL_H
