#ifndef THREAD_THREAD_POOL_IF_H
#define THREAD_THREAD_POOL_IF_H

namespace Hainan {

class ThreadPoolIf
{
public:
    virtual void Start()
    {};
    virtual void Stop()
    {};
    virtual bool PushTask(boost::function<void (void)> task)
    {};
};

} // namespace Hainan

#endif // THREAD_THREAD_POOL_IF_H
