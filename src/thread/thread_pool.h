#include <boost/thread.hpp>
#include <boost/function.hpp>
#include <boost/bind.hpp>

namespace hainan
{
	class ThreadPool
	{
	private:
		int32_t num;
		volatile bool running;
		boost::mutex mutex;
		boost::condition_variable cond;
		std::vector<boost::thread> threads;
		std::list<boost::function<void(void)>> tasks;

		void Loop();

		ThreadPool(ThreadPool const&);
		ThreadPool operator=(ThreadPool const&);
	public:
		ThreadPool();
		~ThreadPool();
		void Start();
		void Stop();
		bool PushTask(boost::function<void(void)> task);
	};
}
