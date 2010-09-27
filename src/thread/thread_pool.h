#ifndef THREAD_THREAD_POOL_H
#define THREAD_THREAD_POOL_H

#include <vector>
#include <list>
#include <boost/thread.hpp>
#include <boost/function.hpp>
#include <boost/bind.hpp>
#include <boost/smart_ptr.hpp>

namespace hainan {

using namespace boost;
using namespace std;
class ThreadPool
{
private:
	int num;
	volatile int work_num;
	volatile bool running;
	mutex mtx;
	condition_variable cond;
	condition_variable done;
	list<shared_ptr<thread> > threads;
	list<function<void(void)> > tasks;

	void Runner();

	ThreadPool(ThreadPool const&);
	ThreadPool operator=(ThreadPool const&);
public:
	ThreadPool();
	~ThreadPool();
	void Start();
	void Stop();
	void SetNum(int n);
	bool PushTask(function<void(void)> task);
};
} // namespace hainan
#endif  // THREAD_THREAD_POOL_H
