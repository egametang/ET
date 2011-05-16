#ifndef THREAD_THREAD_POOL_IF_H
#define THREAD_THREAD_POOL_IF_H

namespace Egametang {

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

} // namespace Egametang

#endif // THREAD_THREAD_POOL_IF_H
