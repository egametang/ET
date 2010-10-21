#ifndef THREAD_THREAD_POOL_H
#define THREAD_THREAD_POOL_H

#include <vector>
#include <list>
#include <boost/thread.hpp>
#include <boost/function.hpp>
#include <boost/bind.hpp>
#include <boost/smart_ptr.hpp>

namespace hainan {

using namespace std;
using namespace boost;

typedef shared_ptr<thread> thread_ptr;

class thread_pool: private noncopyable
{
private:
	int num;
	volatile int work_num;
	volatile bool running;
	mutex mtx;
	condition_variable cond;
	condition_variable done;
	list<thread_ptr> threads;
	list<function<void(void)> > tasks;

	void runner();
public:
	thread_pool();
	~thread_pool();
	void start();
	void stop();
	void set_num(int n);
	bool push_task(function<void(void)> task);
};
} // namespace hainan
#endif  // THREAD_THREAD_POOL_H
