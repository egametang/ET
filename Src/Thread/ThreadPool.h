#ifndef THREAD_THREAD_POOL_H
#define THREAD_THREAD_POOL_H

#include <list>
#include <boost/thread.hpp>
#include <boost/function.hpp>
#include <boost/detail/atomic_count.hpp>
#include "Base/Base.h"

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
	~ThreadPool();

	virtual void Start();
	virtual void Stop();
	virtual bool PushTask(boost::function<void (void)> task);
};

} // namespace Egametang
#endif // THREAD_THREAD_POOL_H
