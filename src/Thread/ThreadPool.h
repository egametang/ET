#ifndef THREAD_THREAD_POOL_H
#define THREAD_THREAD_POOL_H

#include <list>
#include <boost/thread.hpp>
#include <boost/function.hpp>
#include <boost/bind.hpp>
#include "base/base.h"

namespace Hainan {

class ThreadPool: private boost::noncopyable
{
private:
	int num;
	volatile int work_num;
	volatile bool running;
	boost::mutex mutex;
	boost::condition_variable cond;
	boost::condition_variable done;
	std::list<ThreadPtr> threads;
	std::list<boost::function<void (void)> > tasks;

	void Runner();
public:
	ThreadPool();
	~ThreadPool();
	void Start();
	void Stop();
	void SetNum(int num);
	bool PushTask(boost::function<void (void)> task);
};

typedef boost::shared_ptr<ThreadPool> ThreadPoolPtr;
} // namespace Hainan
#endif // THREAD_THREAD_POOL_H
