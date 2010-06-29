#ifndef THREAD_THREAD_POOL_H
#define THREAD_THREAD_POOL_H

#include <vector>
#include <list>
#include <boost/thread.hpp>
#include <boost/function.hpp>
#include <boost/bind.hpp>
#include <boost/smart_ptr.hpp>

namespace hainan
{
	using namespace boost;
	using namespace std;
	class ThreadPool
	{
	private:
		int32_t num;
		volatile int32_t work_num;
		volatile bool running;
		mutex mtx;
		condition_variable cond;
		condition_variable done;
		list<shared_ptr<thread> > threads;
		list<function<void (void)> > tasks;

		void Loop();

		ThreadPool(ThreadPool const&);
		ThreadPool operator=(ThreadPool const&);
	public:
		ThreadPool();
		~ThreadPool();
		void Start();
		void Stop();
		void SetNum(int32_t n);
		bool PushTask(function<void(void)> task);
	};
}
#endif  // THREAD_THREAD_POOL_H
