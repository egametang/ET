/*
 * threadpool-ms-io.c: Microsoft IO threadpool runtime support
 *
 * Author:
 *	Ludovic Henry (ludovic.henry@xamarin.com)
 *
 * Copyright 2015 Xamarin, Inc (http://www.xamarin.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#include "il2cpp-config.h"
#include "gc/WriteBarrier.h"

#ifndef DISABLE_SOCKETS

#ifndef IL2CPP_USE_PIPES_FOR_WAKEUP
#define IL2CPP_USE_PIPES_FOR_WAKEUP !(IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_XBOXONE || IL2CPP_TARGET_PS4 || IL2CPP_TARGET_PSP2)
#endif

#ifndef IL2CPP_USE_EVENTFD_FOR_WAKEUP
#define IL2CPP_USE_EVENTFD_FOR_WAKEUP (0)
#endif

#if !IL2CPP_USE_PIPES_FOR_WAKEUP && !IL2CPP_USE_EVENTFD_FOR_WAKEUP
#include "os/Win32/WindowsHeaders.h"
#else
#include <errno.h>
#include <fcntl.h>
#include <unistd.h>
#if IL2CPP_USE_EVENTFD_FOR_WAKEUP
#include <sys/eventfd.h>
#endif
#endif

#include <vector>

#include "gc/Allocator.h"
#include "mono/ThreadPool/threadpool-ms.h"
#include "mono/ThreadPool/threadpool-ms-io.h"
#include "mono/ThreadPool/threadpool-ms-io-poll.h"
#include "il2cpp-object-internals.h"
#include "os/ConditionVariable.h"
#include "os/Mutex.h"
#include "os/Socket.h"
#include "utils/CallOnce.h"
#include "utils/Il2CppHashMap.h"
#include "vm/Domain.h"
#include "vm/Runtime.h"
#include "vm/Thread.h"

#define UPDATES_CAPACITY 128

typedef std::vector<Il2CppObject*, il2cpp::gc::Allocator<Il2CppObject*> > ManagedList;

struct ThreadPoolStateHasher
{
	size_t operator()(int thread) const
	{
		return thread;
	}
};

typedef Il2CppHashMap<int, ManagedList*, ThreadPoolStateHasher> ThreadPoolStateHash;

typedef enum {
	UPDATE_EMPTY = 0,
	UPDATE_ADD,
	UPDATE_REMOVE_SOCKET,
	UPDATE_REMOVE_DOMAIN,
} ThreadPoolIOUpdateType;

typedef struct {
	int fd;
	Il2CppIOSelectorJob *job;
} ThreadPoolIOUpdate_Add;

typedef struct {
	int fd;
} ThreadPoolIOUpdate_RemoveSocket;

typedef struct {
	Il2CppDomain *domain;
} ThreadPoolIOUpdate_RemoveDomain;

typedef struct {
	ThreadPoolIOUpdateType type;
	union {
		ThreadPoolIOUpdate_Add add;
		ThreadPoolIOUpdate_RemoveSocket remove_socket;
		ThreadPoolIOUpdate_RemoveDomain remove_domain;
	} data;
} ThreadPoolIOUpdate;

typedef struct
{
    bool(*init)(int wakeup_pipe_fd);
    void(*register_fd)(int fd, int events, bool is_new);
    void(*remove_fd)(int fd);
    int(*event_wait)(void(*callback)(int fd, int events, void* user_data), void* user_data);
} ThreadPoolIOBackend;

typedef struct {
	ThreadPoolIOBackend backend;

	ThreadPoolIOUpdate* updates;
	int updates_size;
	il2cpp::os::FastMutex updates_lock;
	il2cpp::os::ConditionVariable updates_cond;
#if IL2CPP_USE_PIPES_FOR_WAKEUP || IL2CPP_USE_EVENTFD_FOR_WAKEUP
	int32_t wakeup_pipes [2];
#else
	il2cpp::os::Socket* wakeup_pipes [2];
#endif
} ThreadPoolIO;

static il2cpp::utils::OnceFlag lazy_init_io_status;

static bool io_selector_running = false;

static ThreadPoolIO* threadpool_io;

static ThreadPoolIOBackend backend_poll = { poll_init, poll_register_fd, poll_remove_fd, poll_event_wait };

static Il2CppIOSelectorJob* get_job_for_event (ManagedList *list, int32_t event)
{
	IL2CPP_ASSERT(list);
	Il2CppIOSelectorJob* foundJob = NULL;
	int matchIndex = -1;
	for (size_t i = 0; i < list->size(); i++)
	{
		Il2CppIOSelectorJob *job = (Il2CppIOSelectorJob*)(*list)[i];
		if (job->operation == event)
		{
			foundJob = job;
			matchIndex = (int)i;
			break;
		}
	}

	if (foundJob == NULL)
		return NULL;

	list->erase(list->begin() + matchIndex);

	return foundJob;
}

static int get_operations_for_jobs (ManagedList *list)
{
	int operations = 0;

	for (size_t i = 0; i < list->size(); i++)
	{
		operations |= ((Il2CppIOSelectorJob*)(*list)[i])->operation;
	}

	return operations;
}

static void selector_thread_wakeup (void)
{
	const char msg = 'c';

	for (;;)
	{
#if IL2CPP_USE_PIPES_FOR_WAKEUP
		int32_t written = write (threadpool_io->wakeup_pipes [1], &msg, 1);
		if (written == 1)
			break;
		if (written == -1)
			break;
#elif IL2CPP_USE_EVENTFD_FOR_WAKEUP
		eventfd_t val = 1;
		int32_t written = eventfd_write(threadpool_io->wakeup_pipes[0], val);
		if (written == 0)
			break;
		if (written == -1)
			break;
#else
		int32_t written = 0;
		const il2cpp::os::WaitStatus status = threadpool_io->wakeup_pipes[1]->Send((const uint8_t*)&msg, 1, il2cpp::os::kSocketFlagsNone, &written);
		if (written == 1)
			break;
		if (written == -1)
		{
			//g_warning ("selector_thread_wakeup: write () failed, error (%d)\n", WSAGetLastError ());
			break;
		}

		if (status == kWaitStatusFailure)
			break;
#endif
	}
}

static void selector_thread_wakeup_drain_pipes (void)
{
	uint8_t buffer [128];
	int32_t received;

	for (;;) {
#if IL2CPP_USE_PIPES_FOR_WAKEUP
		received = read (threadpool_io->wakeup_pipes [0], buffer, sizeof (buffer));
		if (received == 0)
			break;
		if (received == -1) {
			if (errno != EINTR && errno != EAGAIN)
				IL2CPP_ASSERT(0 && "selector_thread_wakeup_drain_pipes: read () failed");
			break;
		}
#elif IL2CPP_USE_EVENTFD_FOR_WAKEUP
		eventfd_t val;
		received = eventfd_read(threadpool_io->wakeup_pipes[0], &val);
		if (received == 0)
			break;
		if (received == -1) {
			if (errno != EINTR && errno != EAGAIN)
				IL2CPP_ASSERT(0 && "selector_thread_wakeup_drain_pipes: read () failed");
			break;
		}
#else
		il2cpp::os::WaitStatus status = threadpool_io->wakeup_pipes[0]->Receive(buffer, 128, il2cpp::os::kSocketFlagsNone, &received);
		if (received == 0)
			break;
		if (status == kWaitStatusFailure)
			break;
#endif
	}
}

typedef struct {
	Il2CppDomain *domain;
	ThreadPoolStateHash *states;
} FilterSockaresForDomainData;

static void filter_jobs_for_domain (void* key, void* value, void* user_data)
{
	//FilterSockaresForDomainData *data;
	//MonoMList *list = (MonoMList *)value, *element;
	//MonoDomain *domain;
	//MonoGHashTable *states;

	//IL2CPP_ASSERT(user_data);
	//data = (FilterSockaresForDomainData *)user_data;
	//domain = data->domain;
	//states = data->states;

	//for (element = list; element; element = mono_mlist_next (element)) {
	//	Il2CppIOSelectorJob *job = (Il2CppIOSelectorJob*) mono_mlist_get_data (element);
	//	if (il2cpp::vm::Domain::GetCurrent() == domain)
	//		mono_mlist_set_data (element, NULL);
	//}

	///* we skip all the first elements which are NULL */
	//for (; list; list = mono_mlist_next (list)) {
	//	if (mono_mlist_get_data (list))
	//		break;
	//}

	//if (list) {
	//	IL2CPP_ASSERT(mono_mlist_get_data (list));

	//	/* we delete all the NULL elements after the first one */
	//	for (element = list; element;) {
	//		MonoMList *next;
	//		if (!(next = mono_mlist_next (element)))
	//			break;
	//		if (mono_mlist_get_data (next))
	//			element = next;
	//		else
	//			mono_mlist_set_next (element, mono_mlist_next (next));
	//	}
	//}

	//mono_g_hash_table_replace (states, key, list);
	IL2CPP_NOT_IMPLEMENTED("TODO");
}

static void wait_callback (int fd, int events, void* user_data)
{
	//Il2CppError error;

	if (il2cpp::vm::Runtime::IsShuttingDown ())
		return;

#if IL2CPP_USE_PIPES_FOR_WAKEUP || IL2CPP_USE_EVENTFD_FOR_WAKEUP
	if (fd == threadpool_io->wakeup_pipes [0]) {
#else
	if (fd == threadpool_io->wakeup_pipes [0]->GetDescriptor()) {
#endif
		//mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_THREADPOOL, "io threadpool: wke");
		selector_thread_wakeup_drain_pipes ();
	} else {
		ThreadPoolStateHash *states;
		ManagedList *list = NULL;
		//void* k;
		bool remove_fd = false;
		int operations;

		IL2CPP_ASSERT(user_data);
		states = (ThreadPoolStateHash *)user_data;

		/*mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_THREADPOOL, "io threadpool: cal fd %3d, events = %2s | %2s | %3s",
			fd, (events & EVENT_IN) ? "RD" : "..", (events & EVENT_OUT) ? "WR" : "..", (events & EVENT_ERR) ? "ERR" : "...");*/

		ThreadPoolStateHash::iterator iter = states->find(fd);
		bool exists = iter != states->end();

		if (!exists)
			IL2CPP_ASSERT("wait_callback: fd not found in states table");
		else
			list = iter->second;

		if (list && (events & EVENT_IN) != 0) {
			Il2CppIOSelectorJob *job = get_job_for_event (list, EVENT_IN);
			if (job) {
				threadpool_ms_enqueue_work_item (il2cpp::vm::Domain::GetCurrent(), (Il2CppObject*) job);
			}

		}
		if (list && (events & EVENT_OUT) != 0) {
			Il2CppIOSelectorJob *job = get_job_for_event (list, EVENT_OUT);
			if (job) {
				threadpool_ms_enqueue_work_item (il2cpp::vm::Domain::GetCurrent(), (Il2CppObject*) job);
			}
		}

		remove_fd = (events & EVENT_ERR) == EVENT_ERR;
		if (!remove_fd) {
			//mono_g_hash_table_replace (states, int_TO_POINTER (fd), list);
			states->insert(ThreadPoolStateHash::value_type(fd, list));

			operations = get_operations_for_jobs (list);

			/*mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_THREADPOOL, "io threadpool: res fd %3d, events = %2s | %2s | %3s",
				fd, (operations & EVENT_IN) ? "RD" : "..", (operations & EVENT_OUT) ? "WR" : "..", (operations & EVENT_ERR) ? "ERR" : "...");*/

			threadpool_io->backend.register_fd (fd, operations, false);
		} else {
			//mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_THREADPOOL, "io threadpool: err fd %d", fd);
			states->erase(ThreadPoolStateHash::key_type(fd));
			//mono_g_hash_table_remove (states, int_TO_POINTER (fd));

			threadpool_io->backend.remove_fd (fd);
		}
	}
}

static void selector_thread (void* data)
{
	//Il2CppError error;
	ThreadPoolStateHash *states;

	io_selector_running = true;

	if (il2cpp::vm::Runtime::IsShuttingDown ()) {
		io_selector_running = false;
		return;
	}

	states = new ThreadPoolStateHash();
	//states = mono_g_hash_table_new_type (g_direct_hash, g_direct_equal, MONO_HASH_VALUE_GC, MONO_ROOT_SOURCE_THREAD_POOL, "i/o thread pool states table");

	for (;;) {
		int i, j;
		int res;

		threadpool_io->updates_lock.Lock();

		for (i = 0; i < threadpool_io->updates_size; ++i) {
			ThreadPoolIOUpdate *update = &threadpool_io->updates [i];

			switch (update->type) {
			case UPDATE_EMPTY:
				break;
			case UPDATE_ADD: {
				int fd;
				int operations;
				//void* k;
				bool exists;
				ManagedList *list = NULL;
				Il2CppIOSelectorJob *job;

				fd = update->data.add.fd;
				IL2CPP_ASSERT(fd >= 0);

				job = update->data.add.job;
				IL2CPP_ASSERT(job);

				ThreadPoolStateHash::iterator iter = states->find(fd);
				exists = iter != states->end();

				if (!exists)
					list = new ManagedList();
				else
					list = iter->second;

				//exists = mono_g_hash_table_lookup_extended (states, int_TO_POINTER (fd), &k, (void**) &list);
				list->push_back((Il2CppObject*)job);
				il2cpp::gc::GarbageCollector::SetWriteBarrier((void**)&(*list)[list->size()-1]);
				states->insert(ThreadPoolStateHash::value_type(fd, list));
				//mono_g_hash_table_replace (states, int_TO_POINTER (fd), list);

				operations = get_operations_for_jobs (list);

				/*mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_THREADPOOL, "io threadpool: %3s fd %3d, operations = %2s | %2s | %3s",
					exists ? "mod" : "add", fd, (operations & EVENT_IN) ? "RD" : "..", (operations & EVENT_OUT) ? "WR" : "..", (operations & EVENT_ERR) ? "ERR" : "...");*/

				threadpool_io->backend.register_fd (fd, operations, !exists);

				break;
			}
			case UPDATE_REMOVE_SOCKET: {
				int fd;
				//void* k;
				ManagedList *list = NULL;

				fd = update->data.remove_socket.fd;
				IL2CPP_ASSERT(fd >= 0);

				ThreadPoolStateHash::iterator iter = states->find(fd);
				bool exists = iter != states->end();

				/*if (mono_g_hash_table_lookup_extended (states, int_TO_POINTER (fd), &k, (void**) &list))*/
				if (exists)
				{
					states->erase(ThreadPoolStateHash::key_type(fd));
					//mono_g_hash_table_remove (states, int_TO_POINTER (fd));

					for (j = i + 1; j < threadpool_io->updates_size; ++j) {
						ThreadPoolIOUpdate *update = &threadpool_io->updates [j];
						if (update->type == UPDATE_ADD && update->data.add.fd == fd)
							memset (update, 0, sizeof (ThreadPoolIOUpdate));
					}

					for (size_t i = 0; i < list->size(); i++)
					{
						threadpool_ms_enqueue_work_item(il2cpp::vm::Domain::GetCurrent(), (*list)[i]);
					}

					list->clear();

					//mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_THREADPOOL, "io threadpool: del fd %3d", fd);
					threadpool_io->backend.remove_fd (fd);
				}

				break;
			}
			case UPDATE_REMOVE_DOMAIN: {
				Il2CppDomain *domain;

				domain = update->data.remove_domain.domain;
				IL2CPP_ASSERT(domain);

				FilterSockaresForDomainData user_data = { domain, states };
				//mono_g_hash_table_foreach (states, filter_jobs_for_domain, &user_data);

				for (j = i + 1; j < threadpool_io->updates_size; ++j) {
					ThreadPoolIOUpdate *update = &threadpool_io->updates [j];
					if (update->type == UPDATE_ADD && il2cpp::vm::Domain::GetCurrent() == domain)
						memset (update, 0, sizeof (ThreadPoolIOUpdate));
				}

				break;
			}
			default:
				IL2CPP_ASSERT(0 && "Should not be reached");
			}
		}

		threadpool_io->updates_cond.Broadcast();

		if (threadpool_io->updates_size > 0) {
			threadpool_io->updates_size = 0;
			memset (threadpool_io->updates, 0, UPDATES_CAPACITY * sizeof (ThreadPoolIOUpdate));
		}

		threadpool_io->updates_lock.Unlock();

		//mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_THREADPOOL, "io threadpool: wai");

		res = threadpool_io->backend.event_wait (wait_callback, states);

		if (res == -1 || il2cpp::vm::Runtime::IsShuttingDown ())
			break;
	}

	delete states;

	io_selector_running = false;
}

/* Locking: threadpool_io->updates_lock must be held */
static ThreadPoolIOUpdate* update_get_new (void)
{
	ThreadPoolIOUpdate *update = NULL;
	IL2CPP_ASSERT(threadpool_io->updates_size <= UPDATES_CAPACITY);

	while (threadpool_io->updates_size == UPDATES_CAPACITY) {
		/* we wait for updates to be applied in the selector_thread and we loop
		 * as long as none are available. if it happends too much, then we need
		 * to increase UPDATES_CAPACITY */
		threadpool_io->updates_cond.Wait(&threadpool_io->updates_lock);
	}

	IL2CPP_ASSERT(threadpool_io->updates_size < UPDATES_CAPACITY);

	update = &threadpool_io->updates [threadpool_io->updates_size ++];

	return update;
}

static void wakeup_pipes_init(void)
{
#if IL2CPP_USE_PIPES_FOR_WAKEUP
	if (pipe (threadpool_io->wakeup_pipes) == -1)
		IL2CPP_ASSERT(0 && "wakeup_pipes_init: pipe () failed");
	if (fcntl (threadpool_io->wakeup_pipes [0], F_SETFL, O_NONBLOCK) == -1)
		IL2CPP_ASSERT(0 && "wakeup_pipes_init: fcntl () failed");
#elif IL2CPP_USE_EVENTFD_FOR_WAKEUP
	threadpool_io->wakeup_pipes[0] = eventfd(0, EFD_NONBLOCK);
	threadpool_io->wakeup_pipes[1] = -1;
#else
	il2cpp::os::Socket serverSock(NULL);

	serverSock.Create(il2cpp::os::kAddressFamilyInterNetwork, il2cpp::os::kSocketTypeStream, il2cpp::os::kProtocolTypeTcp);

	threadpool_io->wakeup_pipes[1] = new il2cpp::os::Socket(NULL);
	il2cpp::os::WaitStatus status = threadpool_io->wakeup_pipes[1]->Create(il2cpp::os::kAddressFamilyInterNetwork, il2cpp::os::kSocketTypeStream, il2cpp::os::kProtocolTypeTcp);
	IL2CPP_ASSERT(status != kWaitStatusFailure);

	if (serverSock.Bind("127.0.0.1", 0) == kWaitStatusFailure)
	{
		serverSock.Close();
		IL2CPP_ASSERT(0 && "wakeup_pipes_init: bind () failed");
	}

	il2cpp::os::EndPointInfo info;
	memset(&info, 0x00, sizeof(il2cpp::os::EndPointInfo));
	if (serverSock.GetLocalEndPointInfo(info) == kWaitStatusFailure)
	{
		serverSock.Close();
		IL2CPP_ASSERT(0 && "wakeup_pipes_init: getsockname () failed");
	}

	if (serverSock.Listen(1024) == kWaitStatusFailure)
	{
		serverSock.Close();
		IL2CPP_ASSERT(0 && "wakeup_pipes_init: listen () failed");
	}

	if (threadpool_io->wakeup_pipes[1]->Connect(info.data.inet.address, info.data.inet.port) == kWaitStatusFailure)
	{
		serverSock.Close();
		IL2CPP_ASSERT(0 && "wakeup_pipes_init: connect () failed");
	}

	status = serverSock.Accept(&threadpool_io->wakeup_pipes[0]);
	IL2CPP_ASSERT(status != kWaitStatusFailure);

	status = threadpool_io->wakeup_pipes[0]->SetBlocking(false);

	if (status == kWaitStatusFailure)
	{
		threadpool_io->wakeup_pipes[0]->Close();
		IL2CPP_ASSERT(0 && "wakeup_pipes_init: SetBlocking () failed");
	}

	status = threadpool_io->wakeup_pipes[0]->SetSocketOption(il2cpp::os::kSocketOptionLevelTcp, il2cpp::os::kSocketOptionNameNoDelay, 1);
	if (status == kWaitStatusFailure)
	{
		threadpool_io->wakeup_pipes[0]->Close();
		IL2CPP_ASSERT(0 && "wakeup_pipes_init: SetSocketOption () failed");
	}

	status = threadpool_io->wakeup_pipes[1]->SetSocketOption(il2cpp::os::kSocketOptionLevelTcp, il2cpp::os::kSocketOptionNameNoDelay, 1);
	if (status == kWaitStatusFailure)
	{
		threadpool_io->wakeup_pipes[1]->Close();
		IL2CPP_ASSERT(0 && "wakeup_pipes_init: SetSocketOption () failed");
	}

	serverSock.Close();
#endif
}

static bool lazy_is_initialized()
{
	return lazy_init_io_status.IsSet();
}

static void threadpool_ms_io_initialize(void* args)
{
	IL2CPP_ASSERT(!threadpool_io);
	threadpool_io = new ThreadPoolIO();
	IL2CPP_ASSERT(threadpool_io);

	threadpool_io->updates = (ThreadPoolIOUpdate*)il2cpp::gc::GarbageCollector::AllocateFixed(sizeof(ThreadPoolIOUpdate) * UPDATES_CAPACITY, NULL);

	threadpool_io->updates_size = 0;

	threadpool_io->backend = backend_poll;
//	if (g_getenv ("MONO_ENABLE_AIO") != NULL) {
//#if defined(HAVE_EPOLL)
//		threadpool_io->backend = backend_epoll;
//#elif defined(HAVE_KQUEUE)
//		threadpool_io->backend = backend_kqueue;
//#endif
//	}

	wakeup_pipes_init ();

#if IL2CPP_USE_PIPES_FOR_WAKEUP || IL2CPP_USE_EVENTFD_FOR_WAKEUP
	if (!threadpool_io->backend.init ((int)threadpool_io->wakeup_pipes [0]))
#else
	if (!threadpool_io->backend.init ((int)threadpool_io->wakeup_pipes [0]->GetDescriptor()))
#endif
		IL2CPP_ASSERT(0 && "initialize: backend->init () failed");

	if (!il2cpp::vm::Thread::CreateInternal(selector_thread, NULL, true, SMALL_STACK))
		IL2CPP_ASSERT(0 && "initialize: vm::Thread::CreateInternal () failed ");
}

static void threadpool_ms_io_lazy_initialize()
{
	il2cpp::utils::CallOnce(lazy_init_io_status, threadpool_ms_io_initialize, NULL);
}

static void cleanup_ms_io (void)
{
	/* we make the assumption along the code that we are
	 * cleaning up only if the runtime is shutting down */
	IL2CPP_ASSERT(il2cpp::vm::Runtime::IsShuttingDown ());

	selector_thread_wakeup ();
	while (io_selector_running)
		il2cpp::vm::Thread::Sleep(1000);
}

void threadpool_ms_io_cleanup (void)
{
	if (lazy_init_io_status.IsSet())
		cleanup_ms_io();
}

void ves_icall_System_IOSelector_Add (intptr_t handle, Il2CppIOSelectorJob *job)
{
	ThreadPoolIOUpdate *update;

	IL2CPP_ASSERT(handle != 0);

	IL2CPP_ASSERT((job->operation == EVENT_IN) ^ (job->operation == EVENT_OUT));
	IL2CPP_ASSERT(job->callback);

	if (il2cpp::vm::Runtime::IsShuttingDown ())
		return;
	/*if (mono_domain_is_unloading (mono_object_domain (job)))
		return;*/

	threadpool_ms_io_lazy_initialize ();

	threadpool_io->updates_lock.Lock();

	update = update_get_new ();

	il2cpp::os::SocketHandleWrapper socketHandle(il2cpp::os::PointerToSocketHandle(reinterpret_cast<void*>(handle)));

	update->type = UPDATE_ADD;
	update->data.add.fd = (int)socketHandle.GetSocket()->GetDescriptor();
	il2cpp::gc::WriteBarrier::GenericStore(&update->data.add.job, job);
	il2cpp::os::Atomic::FullMemoryBarrier(); /* Ensure this is safely published before we wake up the selector */

	selector_thread_wakeup ();

	threadpool_io->updates_lock.Unlock();
}

void ves_icall_System_IOSelector_Remove (intptr_t handle)
{
	il2cpp::os::SocketHandleWrapper socketHandle(il2cpp::os::PointerToSocketHandle(reinterpret_cast<void*>(handle)));
	threadpool_ms_io_remove_socket ((int)socketHandle.GetSocket()->GetDescriptor());
}

void threadpool_ms_io_remove_socket (int fd)
{
	ThreadPoolIOUpdate *update;

	if (!lazy_is_initialized ())
		return;

	threadpool_io->updates_lock.Lock();

	update = update_get_new ();
	update->type = UPDATE_REMOVE_SOCKET;
	update->data.add.fd = fd;
	il2cpp::os::Atomic::FullMemoryBarrier(); /* Ensure this is safely published before we wake up the selector */

	selector_thread_wakeup ();

	threadpool_io->updates_cond.Wait(&threadpool_io->updates_lock);

	threadpool_io->updates_lock.Unlock();
}

#else

void ves_icall_System_IOSelector_Add (intptr_t handle, Il2CppIOSelectorJob *job)
{
	IL2CPP_ASSERT(0 && "Should not be called");
}

void ves_icall_System_IOSelector_Remove (intptr_t handle)
{
	IL2CPP_ASSERT(0 && "Should not be called");
}

void threadpool_ms_io_cleanup (void)
{
	IL2CPP_ASSERT(0 && "Should not be called");
}

void threadpool_ms_io_remove_socket (int fd)
{
	IL2CPP_ASSERT(0 && "Should not be called");
}

#endif
