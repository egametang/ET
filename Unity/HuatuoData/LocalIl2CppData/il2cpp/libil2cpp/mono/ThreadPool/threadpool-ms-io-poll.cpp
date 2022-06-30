#include "il2cpp-config.h"

#include "gc/GarbageCollector.h"
#include "mono/ThreadPool/threadpool-ms-io-poll.h"
#include "os/Socket.h"
#include "utils/Memory.h"
#include "vm/Thread.h"

#include <vector>

static std::vector<il2cpp::os::PollRequest> *poll_fds;
static unsigned int poll_fds_capacity;
static unsigned int poll_fds_size;

static inline void
POLL_INIT_FD(il2cpp::os::PollRequest *poll_fd, int fd, il2cpp::os::PollFlags events)
{
    poll_fd->fd = fd;
    poll_fd->events = events;
    poll_fd->revents = il2cpp::os::kPollFlagsNone;
}

bool poll_init(int wakeup_pipe_fd)
{
    IL2CPP_ASSERT(wakeup_pipe_fd >= 0);

    poll_fds_size = 1;
    poll_fds_capacity = 64;

    poll_fds = new std::vector<il2cpp::os::PollRequest>(poll_fds_capacity);

    POLL_INIT_FD(&(*poll_fds)[0], wakeup_pipe_fd, il2cpp::os::kPollFlagsIn);

    return true;
}

void poll_register_fd(int fd, int events, bool is_new)
{
    unsigned int i;
    il2cpp::os::PollFlags poll_event;

    IL2CPP_ASSERT(fd >= 0);
    IL2CPP_ASSERT(poll_fds_size <= poll_fds_capacity);

    IL2CPP_ASSERT((events & ~(EVENT_IN | EVENT_OUT)) == 0);

    poll_event = il2cpp::os::kPollFlagsNone;
    if (events & EVENT_IN)
        poll_event |= il2cpp::os::kPollFlagsIn;
    if (events & EVENT_OUT)
        poll_event |= il2cpp::os::kPollFlagsOut;

    for (i = 0; i < poll_fds_size; ++i)
    {
        if ((*poll_fds)[i].fd == fd)
        {
            IL2CPP_ASSERT(!is_new);
            POLL_INIT_FD(&(*poll_fds)[i], fd, poll_event);
            return;
        }
    }

    IL2CPP_ASSERT(is_new);

    for (i = 0; i < poll_fds_size; ++i)
    {
        if ((*poll_fds)[i].fd == -1)
        {
            POLL_INIT_FD(&(*poll_fds)[i], fd, poll_event);
            return;
        }
    }

    poll_fds_size += 1;

    if (poll_fds_size > poll_fds_capacity)
    {
        poll_fds_capacity *= 2;
        IL2CPP_ASSERT(poll_fds_size <= poll_fds_capacity);

        poll_fds->resize(poll_fds_capacity, il2cpp::os::PollRequest(-1));
    }

    POLL_INIT_FD(&(*poll_fds)[poll_fds_size - 1], fd, poll_event);
}

void poll_remove_fd(int fd)
{
    unsigned int i;

    IL2CPP_ASSERT(fd >= 0);

    for (i = 0; i < poll_fds_size; ++i)
    {
        if ((*poll_fds)[i].fd == fd)
        {
            POLL_INIT_FD(&(*poll_fds)[i], -1, il2cpp::os::kPollFlagsNone);
            break;
        }
    }

    /* if we don't find the fd in poll_fds,
     * it means we try to delete it twice */
    IL2CPP_ASSERT(i < poll_fds_size);

    /* if we find it again, it means we added
     * it twice */
    for (; i < poll_fds_size; ++i)
        IL2CPP_ASSERT((*poll_fds)[i].fd != fd);

    /* reduce the value of poll_fds_size so we
     * do not keep it too big */
    while (poll_fds_size > 1 && (*poll_fds)[poll_fds_size - 1].fd == -1)
        poll_fds_size -= 1;
}

static inline int
poll_mark_bad_fds(std::vector<il2cpp::os::PollRequest> *poll_fds, int poll_fds_size)
{
    int i, ready = 0;
    int32_t result, error = 0;

    for (i = 0; i < poll_fds_size; i++)
    {
        if ((*poll_fds)[i].fd == -1)
            continue;

        il2cpp::os::WaitStatus status = il2cpp::os::Socket::Poll((*poll_fds)[i], 0, &result, &error);

        if (status == kWaitStatusFailure)
        {
            if ((il2cpp::os::SocketError)error == il2cpp::os::kInvalidHandle)
            {
                (*poll_fds)[i].revents |= il2cpp::os::kPollFlagsNVal;
                ready++;
            }
        }
        else if (result > 0)
            ready++;
    }

    return ready;
}

int poll_event_wait(void (*callback)(int fd, int events, void* user_data), void* user_data)
{
    unsigned int i;

    for (i = 0; i < poll_fds_size; ++i)
        (*poll_fds)[i].revents = il2cpp::os::kPollFlagsNone;

    il2cpp::gc::GarbageCollector::SetSkipThread(true);

    int32_t ready;
    int32_t error;
    il2cpp::os::WaitStatus status = il2cpp::os::Socket::Poll((*poll_fds), poll_fds_size , -1, &ready, &error);

    il2cpp::gc::GarbageCollector::SetSkipThread(false);

    if (ready == -1 || status == kWaitStatusFailure)
    {
        /*
         * Apart from EINTR, we only check EBADF, for the rest:
         *  EINVAL: mono_poll() 'protects' us from descriptor
         *      numbers above the limit if using select() by marking
         *      then as POLLERR.  If a system poll() is being
         *      used, the number of descriptor we're passing will not
         *      be over sysconf(_SC_OPEN_MAX), as the error would have
         *      happened when opening.
         *
         *  EFAULT: we own the memory pointed by pfds.
         *  ENOMEM: we're doomed anyway
         *
         */

        if ((il2cpp::os::SocketError)error == il2cpp::os::kInterrupted)
        {
            il2cpp::vm::Thread::CheckCurrentThreadForInterruptAndThrowIfNecessary();
            ready = 0;
        }
        else if ((il2cpp::os::SocketError)error == il2cpp::os::kInvalidHandle)
        {
            ready = poll_mark_bad_fds(poll_fds, poll_fds_size);
        }
    }

    if (ready == -1)
        return -1;
    if (ready == 0)
        return 0;

    IL2CPP_ASSERT(ready > 0);

    for (i = 0; i < poll_fds_size; ++i)
    {
        int fd, events = 0;

        if ((*poll_fds)[i].fd == -1)
            continue;
        if ((*poll_fds)[i].revents == 0)
            continue;

        fd = (int)(*poll_fds)[i].fd;
        if ((*poll_fds)[i].revents & (il2cpp::os::kPollFlagsIn | il2cpp::os::kPollFlagsErr | il2cpp::os::kPollFlagsHup | il2cpp::os::kPollFlagsNVal))
            events |= EVENT_IN;
        if ((*poll_fds)[i].revents & (il2cpp::os::kPollFlagsOut | il2cpp::os::kPollFlagsErr | il2cpp::os::kPollFlagsHup | il2cpp::os::kPollFlagsNVal))
            events |= EVENT_OUT;
        if ((*poll_fds)[i].revents & (il2cpp::os::kPollFlagsErr | il2cpp::os::kPollFlagsHup | il2cpp::os::kPollFlagsNVal))
            events |= EVENT_ERR;

        callback(fd, events, user_data);

        if (--ready == 0)
            break;
    }

    return 0;
}
