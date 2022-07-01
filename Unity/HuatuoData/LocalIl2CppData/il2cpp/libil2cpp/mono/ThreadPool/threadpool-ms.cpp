/*
 * threadpool-ms.c: Microsoft threadpool runtime support
 *
 * Author:
 *	Ludovic Henry (ludovic.henry@xamarin.com)
 *
 * Copyright 2015 Xamarin, Inc (http://www.xamarin.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
// Files:
//  - src/vm/comthreadpool.cpp
//  - src/vm/win32threadpoolcpp
//  - src/vm/threadpoolrequest.cpp
//  - src/vm/hillclimbing.cpp
//
// Ported from C++ to C and adjusted to Mono runtime

#include "il2cpp-config.h"

#include <stdlib.h>
#define _USE_MATH_DEFINES // needed by MSVC to define math constants
#include <algorithm>
#include <cmath>
#include <complex>
#include "math.h"

#include "il2cpp-api.h"
#include "gc/GarbageCollector.h"
#include "gc/GCHandle.h"
#include "gc/WriteBarrier.h"
#include "icalls/mscorlib/System.Threading/ThreadPool.h"
#include "icalls/mscorlib/System.Runtime.Remoting.Messaging/MonoMethodMessage.h"
#include "mono/ThreadPool/threadpool-ms.h"
#include "mono/ThreadPool/threadpool-ms-io.h"
#include "mono/ThreadPool/ThreadPoolDataStructures.h"
#include "mono/ThreadPool/ThreadPoolMacros.h"
#include "mono/ThreadPool/ThreadPoolMonitorThread.h"
#include "mono/ThreadPool/ThreadPoolWorkerThread.h"
#include "il2cpp-object-internals.h"
#include "os/CpuInfo.h"
#include "os/Environment.h"
#include "os/Mutex.h"
#include "os/Time.h"
#include "utils/CallOnce.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Domain.h"
#include "vm/Exception.h"
#include "vm/Object.h"
#include "vm/Reflection.h"
#include "vm/Random.h"
#include "vm/Runtime.h"
#include "vm/String.h"
#include "vm/Thread.h"
#include "vm/ThreadPool.h"
#include "vm/WaitHandle.h"

#ifndef CLAMP
#define CLAMP(a,low,high) (((a) < (low)) ? (low) : (((a) > (high)) ? (high) : (a)))
#endif

ThreadPool* g_ThreadPool;

/* The exponent to apply to the gain. 1.0 means to use linear gain,
 * higher values will enhance large moves and damp small ones.
 * default: 2.0 */
#define HILL_CLIMBING_GAIN_EXPONENT 2.0

/* The 'cost' of a thread. 0 means drive for increased throughput regardless
 * of thread count, higher values bias more against higher thread counts.
 * default: 0.15 */
#define HILL_CLIMBING_BIAS 0.15

#define HILL_CLIMBING_WAVE_PERIOD 4
#define HILL_CLIMBING_MAX_WAVE_MAGNITUDE 20
#define HILL_CLIMBING_WAVE_MAGNITUDE_MULTIPLIER 1.0
#define HILL_CLIMBING_WAVE_HISTORY_SIZE 8
#define HILL_CLIMBING_TARGET_SIGNAL_TO_NOISE_RATIO 3.0
#define HILL_CLIMBING_MAX_CHANGE_PER_SECOND 4
#define HILL_CLIMBING_MAX_CHANGE_PER_SAMPLE 20
#define HILL_CLIMBING_SAMPLE_INTERVAL_LOW 10
#define HILL_CLIMBING_SAMPLE_INTERVAL_HIGH 200
#define HILL_CLIMBING_ERROR_SMOOTHING_FACTOR 0.01
#define HILL_CLIMBING_MAX_SAMPLE_ERROR_PERCENT 0.15

static il2cpp::utils::OnceFlag lazy_init_status;

static Il2CppMethodMessage *
mono_method_call_message_new(MethodInfo *method, void* *params, MethodInfo *invoke,
	Il2CppDelegate **cb, Il2CppObject **state)
{
	Il2CppDomain *domain = il2cpp::vm::Domain::GetCurrent();
	Il2CppMethodMessage *msg;
	int i, count;

	msg = (Il2CppMethodMessage *)il2cpp::vm::Object::New(il2cpp_defaults.mono_method_message_class);

	if (invoke) {
		Il2CppReflectionMethod *rm = il2cpp::vm::Reflection::GetMethodObject(invoke, NULL);
		il2cpp::icalls::mscorlib::System::Runtime::Remoting::Messaging::MonoMethodMessage::InitMessage(msg, rm, NULL);
		count = method->parameters_count - 2;
	}
	else {
		Il2CppReflectionMethod *rm = il2cpp::vm::Reflection::GetMethodObject(method, NULL);
		il2cpp::icalls::mscorlib::System::Runtime::Remoting::Messaging::MonoMethodMessage::InitMessage(msg, rm, NULL);
		count = method->parameters_count;
	}

	for (i = 0; i < count; i++) {
		void* vpos;
		Il2CppClass *klass;
		Il2CppObject *arg;

			vpos = params[i];

		klass = il2cpp_class_from_type(method->parameters[i].parameter_type);
		arg = (Il2CppObject*)vpos;

		il2cpp_array_setref(msg->args, i, arg);
	}

	if (cb != NULL && state != NULL) {
		*cb = (Il2CppDelegate *)params[i];
		i++;
		*state = (Il2CppObject *)params[i];
	}

	return msg;
}

static void* cpu_info_create()
{
	return il2cpp::os::CpuInfo::Create();
}


ThreadPool::ThreadPool() :
    parked_threads_count(0),
    worker_creation_current_second(-1),
    worker_creation_current_count(0),
    heuristic_completions(0),
    heuristic_sample_start(0),
    heuristic_last_dequeue(0),
    heuristic_last_adjustment(0),
    heuristic_adjustment_interval(10),
    limit_worker_min(0),
    limit_worker_max(0),
    limit_io_min(0),
    limit_io_max(0),
    cpu_usage(0),
    suspended(false)
{
    counters.as_int64_t = 0;
    cpu_usage_state = cpu_info_create();
}

static void initialize(void* arg)
{
	ThreadPoolHillClimbing *hc;
	//const char *threads_per_cpu_env;
	int threads_per_cpu;
	int threads_count;

	IL2CPP_ASSERT(!g_ThreadPool);
    g_ThreadPool = new ThreadPool();
	IL2CPP_ASSERT(g_ThreadPool);

	il2cpp::vm::Random::Open();

	hc = &g_ThreadPool->heuristic_hill_climbing;

	hc->wave_period = HILL_CLIMBING_WAVE_PERIOD;
	hc->max_thread_wave_magnitude = HILL_CLIMBING_MAX_WAVE_MAGNITUDE;
	hc->thread_magnitude_multiplier = (double) HILL_CLIMBING_WAVE_MAGNITUDE_MULTIPLIER;
	hc->samples_to_measure = hc->wave_period * HILL_CLIMBING_WAVE_HISTORY_SIZE;
	hc->target_throughput_ratio = (double) HILL_CLIMBING_BIAS;
	hc->target_signal_to_noise_ratio = (double) HILL_CLIMBING_TARGET_SIGNAL_TO_NOISE_RATIO;
	hc->max_change_per_second = (double) HILL_CLIMBING_MAX_CHANGE_PER_SECOND;
	hc->max_change_per_sample = (double) HILL_CLIMBING_MAX_CHANGE_PER_SAMPLE;
	hc->sample_interval_low = HILL_CLIMBING_SAMPLE_INTERVAL_LOW;
	hc->sample_interval_high = HILL_CLIMBING_SAMPLE_INTERVAL_HIGH;
	hc->throughput_error_smoothing_factor = (double) HILL_CLIMBING_ERROR_SMOOTHING_FACTOR;
	hc->gain_exponent = (double) HILL_CLIMBING_GAIN_EXPONENT;
	hc->max_sample_error = (double) HILL_CLIMBING_MAX_SAMPLE_ERROR_PERCENT;
	hc->current_control_setting = 0;
	hc->total_samples = 0;
	hc->last_thread_count = 0;
	hc->average_throughput_noise = 0;
	hc->elapsed_since_last_change = 0;
	hc->accumulated_completion_count = 0;
	hc->accumulated_sample_duration = 0;
	hc->samples = (double*)IL2CPP_MALLOC_ZERO (sizeof(double) * hc->samples_to_measure);
	hc->thread_counts = (double*)IL2CPP_MALLOC_ZERO(sizeof(double) * hc->samples_to_measure);
	hc->random_interval_generator = il2cpp::vm::Random::Create ();
	hc->current_sample_interval = il2cpp::vm::Random::Next (&hc->random_interval_generator, hc->sample_interval_low, hc->sample_interval_high);

	//std::string threads_per_cpu_env = il2cpp::os::Environment::GetEnvironmentVariable("IL2CPP_THREADS_PER_CPU");
	//if (threads_per_cpu_env.empty())
	threads_per_cpu = 1;
	/*else
		threads_per_cpu = CLAMP (atoi (threads_per_cpu_env.c_str()), 1, 50);*/

	threads_count = il2cpp::os::Environment::GetProcessorCount() * threads_per_cpu;

	g_ThreadPool->limit_worker_min = g_ThreadPool->limit_io_min = threads_count;

#if IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_IOS
	g_ThreadPool->limit_worker_max = g_ThreadPool->limit_io_max = CLAMP (threads_count * 100, std::min (threads_count, 200), std::max (threads_count, 200));
#else
	g_ThreadPool->limit_worker_max = g_ThreadPool->limit_io_max = threads_count * 100;
#endif

	g_ThreadPool->counters._.max_working = g_ThreadPool->limit_worker_min;
}

static void lazy_initialize()
{
	il2cpp::utils::CallOnce(lazy_init_status, initialize, NULL);
}

static void worker_kill(Il2CppInternalThread* thread)
{
	if (thread == il2cpp::vm::Thread::CurrentInternal())
		return;

	il2cpp::vm::Thread::Stop(thread);
}

static void cleanup (void)
{
	unsigned int i;

	/* we make the assumption along the code that we are
	 * cleaning up only if the runtime is shutting down */
	IL2CPP_ASSERT(il2cpp::vm::Runtime::IsShuttingDown ());

	while (GetMonitorStatus() != MONITOR_STATUS_NOT_RUNNING)
		il2cpp::vm::Thread::Sleep(1);

	std::vector<Il2CppInternalThread*> working_threads;

	g_ThreadPool->active_threads_lock.Lock();
	working_threads = g_ThreadPool->working_threads;
	g_ThreadPool->active_threads_lock.Unlock();

	/* stop all threadpool->working_threads */
	for (i = 0; i < working_threads.size(); ++i)
		worker_kill (working_threads[i]);

	/* unpark all g_ThreadPool->parked_threads */
	g_ThreadPool->parked_threads_cond.Broadcast();
}

bool threadpool_ms_enqueue_work_item (Il2CppDomain *domain, Il2CppObject *work_item)
{
	static Il2CppClass *threadpool_class = NULL;
	static MethodInfo *unsafe_queue_custom_work_item_method = NULL;
	//Il2CppDomain *current_domain;
	bool f;
	void* args [2];

	IL2CPP_ASSERT(work_item);

	if (!threadpool_class)
		threadpool_class = il2cpp::vm::Class::FromName(il2cpp_defaults.corlib, "System.Threading", "ThreadPool");

	if (!unsafe_queue_custom_work_item_method)
		unsafe_queue_custom_work_item_method = (MethodInfo*)il2cpp::vm::Class::GetMethodFromName(threadpool_class, "UnsafeQueueCustomWorkItem", 2);
	IL2CPP_ASSERT(unsafe_queue_custom_work_item_method);

	f = false;

	args [0] = (void*) work_item;
	args [1] = (void*) &f;

	Il2CppObject *result = il2cpp::vm::Runtime::InvokeWithThrow(unsafe_queue_custom_work_item_method, NULL, args);
	return true;
}

/* LOCKING: threadpool->domains_lock must be held */
static ThreadPoolDomain* domain_get(Il2CppDomain *domain, bool create)
{
	ThreadPoolDomain *tpdomain = NULL;
	unsigned int i;

	IL2CPP_ASSERT(domain);

	for (i = 0; i < g_ThreadPool->domains.size(); ++i) {
		tpdomain = g_ThreadPool->domains[i];
		if (tpdomain->domain == domain)
			return tpdomain;
	}

	if (create) {
		tpdomain = new ThreadPoolDomain();
		tpdomain->domain = domain;
		g_ThreadPool->domains.push_back(tpdomain);
	}

	return tpdomain;
}

bool worker_try_unpark()
{
	il2cpp::os::FastAutoLockOld lock(&g_ThreadPool->active_threads_lock);

	if (g_ThreadPool->parked_threads_count == 0)
		return false;

	g_ThreadPool->parked_threads_cond.Signal();
	return true;
}

static bool worker_request (Il2CppDomain *domain)
{
	ThreadPoolDomain *tpdomain;

	IL2CPP_ASSERT(domain);
	IL2CPP_ASSERT(g_ThreadPool);

	if (il2cpp::vm::Runtime::IsShuttingDown ())
		return false;

	g_ThreadPool->domains_lock.Acquire();

	/* synchronize check with worker_thread */
	//if (mono_domain_is_unloading (domain)) {
		//mono_coop_mutex_unlock (&threadpool->domains_lock);
		/*return false;
	}*/

	tpdomain = domain_get (domain, true);
	IL2CPP_ASSERT(tpdomain);
	tpdomain->outstanding_request ++;

	/*mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_THREADPOOL, "[%p] request worker, domain = %p, outstanding_request = %d",
		mono_native_thread_id_get (), tpdomain->domain, tpdomain->outstanding_request);*/

	g_ThreadPool->domains_lock.Release();

	if (g_ThreadPool->suspended)
		return false;

	monitor_ensure_running ();

	if (worker_try_unpark ()) {
		//mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_THREADPOOL, "[%p] request worker, unparked", mono_native_thread_id_get ());
		return true;
	}

	if (worker_try_create ()) {
		//mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_THREADPOOL, "[%p] request worker, created", mono_native_thread_id_get ());
		return true;
	}

	//mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_THREADPOOL, "[%p] request worker, failed", mono_native_thread_id_get ());
	return false;
}

static void hill_climbing_change_thread_count (int16_t new_thread_count, ThreadPoolHeuristicStateTransition transition)
{
	ThreadPoolHillClimbing *hc;

	IL2CPP_ASSERT(g_ThreadPool);

	hc = &g_ThreadPool->heuristic_hill_climbing;

	//mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_THREADPOOL, "[%p] hill climbing, change max number of threads %d", mono_native_thread_id_get (), new_thread_count);

	hc->last_thread_count = new_thread_count;
	hc->current_sample_interval = il2cpp::vm::Random::Next(&hc->random_interval_generator, hc->sample_interval_low, hc->sample_interval_high);
	hc->elapsed_since_last_change = 0;
	hc->completions_since_last_change = 0;
}

void hill_climbing_force_change (int16_t new_thread_count, ThreadPoolHeuristicStateTransition transition)
{
	ThreadPoolHillClimbing *hc;

	IL2CPP_ASSERT(g_ThreadPool);

	hc = &g_ThreadPool->heuristic_hill_climbing;

	if (new_thread_count != hc->last_thread_count) {
		hc->current_control_setting += new_thread_count - hc->last_thread_count;
		hill_climbing_change_thread_count (new_thread_count, transition);
	}
}

static std::complex<double> hill_climbing_get_wave_component (double *samples, unsigned int sample_count, double period)
{
	ThreadPoolHillClimbing *hc;
	double w, cosine, sine, coeff, q0, q1, q2;
	unsigned int i;

	IL2CPP_ASSERT(g_ThreadPool);
	IL2CPP_ASSERT(sample_count >= period);
	IL2CPP_ASSERT(period >= 2);

	hc = &g_ThreadPool->heuristic_hill_climbing;

	w = 2.0 * M_PI / period;
	cosine = cos (w);
	sine = sin (w);
	coeff = 2.0 * cosine;
	q0 = q1 = q2 = 0;

	for (i = 0; i < sample_count; ++i) {
		q0 = coeff * q1 - q2 + samples [(hc->total_samples - sample_count + i) % hc->samples_to_measure];
		q2 = q1;
		q1 = q0;
	}

	return (std::complex<double> (q1 - q2 * cosine, (q2 * sine)) / ((double)sample_count));
}

static int16_t hill_climbing_update (int16_t current_thread_count, uint32_t sample_duration, int32_t completions, int64_t *adjustment_interval)
{
	ThreadPoolHillClimbing *hc;
	ThreadPoolHeuristicStateTransition transition;
	double throughput;
	double throughput_error_estimate;
	double confidence;
	double move;
	double gain;
	int sample_index;
	int sample_count;
	int new_thread_wave_magnitude;
	int new_thread_count;
	std::complex<double> thread_wave_component;
	std::complex<double> throughput_wave_component;
	std::complex<double> ratio;

	IL2CPP_ASSERT(g_ThreadPool);
	IL2CPP_ASSERT(adjustment_interval);

	hc = &g_ThreadPool->heuristic_hill_climbing;

	/* If someone changed the thread count without telling us, update our records accordingly. */
	if (current_thread_count != hc->last_thread_count)
		hill_climbing_force_change (current_thread_count, TRANSITION_INITIALIZING);

	/* Update the cumulative stats for this thread count */
	hc->elapsed_since_last_change += sample_duration;
	hc->completions_since_last_change += completions;

	/* Add in any data we've already collected about this sample */
	sample_duration += (uint32_t)hc->accumulated_sample_duration;
	completions += hc->accumulated_completion_count;

	/* We need to make sure we're collecting reasonably accurate data. Since we're just counting the end
	 * of each work item, we are goinng to be missing some data about what really happened during the
	 * sample interval. The count produced by each thread includes an initial work item that may have
	 * started well before the start of the interval, and each thread may have been running some new
	 * work item for some time before the end of the interval, which did not yet get counted. So
	 * our count is going to be off by +/- threadCount workitems.
	 *
	 * The exception is that the thread that reported to us last time definitely wasn't running any work
	 * at that time, and the thread that's reporting now definitely isn't running a work item now. So
	 * we really only need to consider threadCount-1 threads.
	 *
	 * Thus the percent error in our count is +/- (threadCount-1)/numCompletions.
	 *
	 * We cannot rely on the frequency-domain analysis we'll be doing later to filter out this error, because
	 * of the way it accumulates over time. If this sample is off by, say, 33% in the negative direction,
	 * then the next one likely will be too. The one after that will include the sum of the completions
	 * we missed in the previous samples, and so will be 33% positive. So every three samples we'll have
	 * two "low" samples and one "high" sample. This will appear as periodic variation right in the frequency
	 * range we're targeting, which will not be filtered by the frequency-domain translation. */
	if (hc->total_samples > 0 && ((current_thread_count - 1.0) / completions) >= hc->max_sample_error) {
		/* Not accurate enough yet. Let's accumulate the data so
		 * far, and tell the ThreadPool to collect a little more. */
		hc->accumulated_sample_duration = sample_duration;
		hc->accumulated_completion_count = completions;
		*adjustment_interval = 10;
		return current_thread_count;
	}

	/* We've got enouugh data for our sample; reset our accumulators for next time. */
	hc->accumulated_sample_duration = 0;
	hc->accumulated_completion_count = 0;

	/* Add the current thread count and throughput sample to our history. */
	throughput = ((double) completions) / sample_duration;

	sample_index = hc->total_samples % hc->samples_to_measure;
	hc->samples [sample_index] = throughput;
	hc->thread_counts [sample_index] = current_thread_count;
	hc->total_samples ++;

	/* Set up defaults for our metrics. */
	throughput_error_estimate = 0;
	confidence = 0;

	transition = TRANSITION_WARMUP;

	/* How many samples will we use? It must be at least the three wave periods we're looking for, and it must also
	 * be a whole multiple of the primary wave's period; otherwise the frequency we're looking for will fall between
	 * two frequency bands in the Fourier analysis, and we won't be able to measure it accurately. */
	sample_count = ((int) std::min (hc->total_samples - 1, (int64_t)hc->samples_to_measure) / hc->wave_period) * hc->wave_period;

	if (sample_count > hc->wave_period) {
		int i;
		double average_throughput;
		double average_thread_count;
		double sample_sum = 0;
		double thread_sum = 0;

		/* Average the throughput and thread count samples, so we can scale the wave magnitudes later. */
		for (i = 0; i < sample_count; ++i) {
			unsigned int j = (hc->total_samples - sample_count + i) % hc->samples_to_measure;
			sample_sum += hc->samples [j];
			thread_sum += hc->thread_counts [j];
		}

		average_throughput = sample_sum / sample_count;
		average_thread_count = thread_sum / sample_count;

		if (average_throughput > 0 && average_thread_count > 0) {
			double noise_for_confidence, adjacent_period_1, adjacent_period_2;

			/* Calculate the periods of the adjacent frequency bands we'll be using to
			 * measure noise levels. We want the two adjacent Fourier frequency bands. */
			adjacent_period_1 = sample_count / (((double) sample_count) / ((double) hc->wave_period) + 1);
			adjacent_period_2 = sample_count / (((double) sample_count) / ((double) hc->wave_period) - 1);

			/* Get the the three different frequency components of the throughput (scaled by average
			 * throughput). Our "error" estimate (the amount of noise that might be present in the
			 * frequency band we're really interested in) is the average of the adjacent bands. */

			throughput_wave_component = hill_climbing_get_wave_component(hc->samples, sample_count, hc->wave_period) / average_throughput;
			//throughput_wave_component = mono_double_complex_scalar_div (hill_climbing_get_wave_component (hc->samples, sample_count, hc->wave_period), average_throughput);

			throughput_error_estimate = std::abs(hill_climbing_get_wave_component(hc->samples, sample_count, adjacent_period_1) / average_throughput);
			//throughput_error_estimate = cabs (mono_double_complex_scalar_div (hill_climbing_get_wave_component (hc->samples, sample_count, adjacent_period_1), average_throughput));

			if (adjacent_period_2 <= sample_count) {
				throughput_error_estimate = std::max (throughput_error_estimate, std::abs (hill_climbing_get_wave_component (
					hc->samples, sample_count, adjacent_period_2) / average_throughput));
			}

			/* Do the same for the thread counts, so we have something to compare to. We don't
			 * measure thread count noise, because there is none; these are exact measurements. */
			thread_wave_component = hill_climbing_get_wave_component (hc->thread_counts, sample_count, hc->wave_period) / average_thread_count;

			/* Update our moving average of the throughput noise. We'll use this
			 * later as feedback to determine the new size of the thread wave. */
			if (hc->average_throughput_noise == 0) {
				hc->average_throughput_noise = throughput_error_estimate;
			} else {
				hc->average_throughput_noise = (hc->throughput_error_smoothing_factor * throughput_error_estimate)
					+ ((1.0 + hc->throughput_error_smoothing_factor) * hc->average_throughput_noise);
			}

			if (std::abs (thread_wave_component) > 0) {
				/* Adjust the throughput wave so it's centered around the target wave,
				 * and then calculate the adjusted throughput/thread ratio. */
				ratio = ((throughput_wave_component - (thread_wave_component * hc->target_throughput_ratio)) / thread_wave_component);
				transition = TRANSITION_CLIMBING_MOVE;
			} else {
				//ratio = mono_double_complex_make (0, 0);
				transition = TRANSITION_STABILIZING;
			}

			noise_for_confidence = std::max (hc->average_throughput_noise, throughput_error_estimate);
			if (noise_for_confidence > 0) {
				confidence = std::abs (thread_wave_component) / noise_for_confidence / hc->target_signal_to_noise_ratio;
			} else {
				/* there is no noise! */
				confidence = 1.0;
			}
		}
	}

	/* We use just the real part of the complex ratio we just calculated. If the throughput signal
	 * is exactly in phase with the thread signal, this will be the same as taking the magnitude of
	 * the complex move and moving that far up. If they're 180 degrees out of phase, we'll move
	 * backward (because this indicates that our changes are having the opposite of the intended effect).
	 * If they're 90 degrees out of phase, we won't move at all, because we can't tell wether we're
	 * having a negative or positive effect on throughput. */
	move = std::real (ratio);
	move = CLAMP (move, -1.0, 1.0);

	/* Apply our confidence multiplier. */
	move *= CLAMP (confidence, -1.0, 1.0);

	/* Now apply non-linear gain, such that values around zero are attenuated, while higher values
	 * are enhanced. This allows us to move quickly if we're far away from the target, but more slowly
	* if we're getting close, giving us rapid ramp-up without wild oscillations around the target. */
	gain = hc->max_change_per_second * sample_duration;
	move = pow (fabs (move), hc->gain_exponent) * (move >= 0.0 ? 1 : -1) * gain;
	move = std::min (move, hc->max_change_per_sample);

	/* If the result was positive, and CPU is > 95%, refuse the move. */
	if (move > 0.0 && g_ThreadPool->cpu_usage > CPU_USAGE_HIGH)
		move = 0.0;

	/* Apply the move to our control setting. */
	hc->current_control_setting += move;

	/* Calculate the new thread wave magnitude, which is based on the moving average we've been keeping of the
	 * throughput error.  This average starts at zero, so we'll start with a nice safe little wave at first. */
	new_thread_wave_magnitude = (int)(0.5 + (hc->current_control_setting * hc->average_throughput_noise
		* hc->target_signal_to_noise_ratio * hc->thread_magnitude_multiplier * 2.0));
	new_thread_wave_magnitude = CLAMP (new_thread_wave_magnitude, 1, hc->max_thread_wave_magnitude);

	/* Make sure our control setting is within the ThreadPool's limits. */
	hc->current_control_setting = CLAMP (hc->current_control_setting, g_ThreadPool->limit_worker_min, g_ThreadPool->limit_worker_max - new_thread_wave_magnitude);

	/* Calculate the new thread count (control setting + square wave). */
	new_thread_count = (int)(hc->current_control_setting + new_thread_wave_magnitude * ((hc->total_samples / (hc->wave_period / 2)) % 2));

	/* Make sure the new thread count doesn't exceed the ThreadPool's limits. */
	new_thread_count = CLAMP (new_thread_count, g_ThreadPool->limit_worker_min, g_ThreadPool->limit_worker_max);

	if (new_thread_count != current_thread_count)
		hill_climbing_change_thread_count (new_thread_count, transition);

	if (std::real (ratio) < 0.0 && new_thread_count == g_ThreadPool->limit_worker_min)
		*adjustment_interval = (int)(0.5 + hc->current_sample_interval * (10.0 * std::max (-1.0 * std::real (ratio), 1.0)));
	else
		*adjustment_interval = hc->current_sample_interval;

	return new_thread_count;
}

static void heuristic_notify_work_completed (void)
{
	IL2CPP_ASSERT(g_ThreadPool);

	g_ThreadPool->heuristic_completions++;
	g_ThreadPool->heuristic_last_dequeue = il2cpp::os::Time::GetTicksMillisecondsMonotonic();
}

static bool heuristic_should_adjust (void)
{
	IL2CPP_ASSERT(g_ThreadPool);

	if (g_ThreadPool->heuristic_last_dequeue > g_ThreadPool->heuristic_last_adjustment + g_ThreadPool->heuristic_adjustment_interval) {
		ThreadPoolCounter counter;
		counter.as_int64_t = COUNTER_READ();
		if (counter._.working <= counter._.max_working)
			return true;
	}

	return false;
}

static void heuristic_adjust (void)
{
	IL2CPP_ASSERT(g_ThreadPool);

	if (g_ThreadPool->heuristic_lock.TryAcquire()) {
		int32_t completions = g_ThreadPool->heuristic_completions.exchange(0);
		int64_t sample_end = il2cpp::os::Time::GetTicksMillisecondsMonotonic();
		int64_t sample_duration = sample_end - g_ThreadPool->heuristic_sample_start;

		if (sample_duration >= g_ThreadPool->heuristic_adjustment_interval / 2) {
			ThreadPoolCounter counter;
			int16_t new_thread_count;

			counter.as_int64_t = COUNTER_READ ();
			new_thread_count = hill_climbing_update (counter._.max_working, (uint32_t)sample_duration, completions, &g_ThreadPool->heuristic_adjustment_interval);

			COUNTER_ATOMIC (counter, { counter._.max_working = new_thread_count; });

			if (new_thread_count > counter._.max_working)
				worker_request (il2cpp::vm::Domain::GetCurrent());

			g_ThreadPool->heuristic_sample_start = sample_end;
			g_ThreadPool->heuristic_last_adjustment = il2cpp::os::Time::GetTicksMillisecondsMonotonic();
		}

		g_ThreadPool->heuristic_lock.Release();
	}
}

void threadpool_ms_cleanup (void)
{
	#ifndef DISABLE_SOCKETS
		threadpool_ms_io_cleanup ();
	#endif

	if (lazy_init_status.IsSet())
		cleanup();
}

Il2CppAsyncResult* threadpool_ms_begin_invoke (Il2CppDomain *domain, Il2CppObject *target, MethodInfo *method, void* *params)
{
	Il2CppMethodMessage *message;
	Il2CppDelegate *async_callback = NULL;
	Il2CppObject *state = NULL;

	Il2CppAsyncCall* async_call = (Il2CppAsyncCall*)il2cpp::vm::Object::New(il2cpp_defaults.async_call_class);

	lazy_initialize ();

	MethodInfo *invoke = NULL;
	if (il2cpp::vm::Class::HasParent(method->klass, il2cpp_defaults.multicastdelegate_class))
		invoke = (MethodInfo*)il2cpp::vm::Class::GetMethodFromName(method->klass, "Invoke", -1);

	message = mono_method_call_message_new (method, params, invoke, (params != NULL) ? (&async_callback) : NULL, (params != NULL) ? (&state) : NULL);

	IL2CPP_OBJECT_SETREF (async_call, msg, message);
	IL2CPP_OBJECT_SETREF (async_call, state, state);

	if (async_callback)
	{
		IL2CPP_OBJECT_SETREF (async_call, cb_method, (MethodInfo*)il2cpp::vm::Runtime::GetDelegateInvoke(il2cpp::vm::Object::GetClass((Il2CppObject*)async_callback)));
		IL2CPP_OBJECT_SETREF (async_call, cb_target, async_callback);
	}

	Il2CppAsyncResult* async_result = (Il2CppAsyncResult*)il2cpp::vm::Object::New(il2cpp_defaults.asyncresult_class);

	IL2CPP_OBJECT_SETREF(async_result, async_delegate, (Il2CppDelegate*)target);

	IL2CPP_OBJECT_SETREF(async_result, object_data, async_call);
	IL2CPP_OBJECT_SETREF(async_result, async_state, async_call->state);

	threadpool_ms_enqueue_work_item (domain, (Il2CppObject*) async_result);

	return async_result;
}

Il2CppObject* threadpool_ms_end_invoke (Il2CppAsyncResult *ares, Il2CppArray **out_args, Il2CppObject **exc)
{
	Il2CppAsyncCall *ac;

	IL2CPP_ASSERT(exc);
	IL2CPP_ASSERT(out_args);

	*exc = NULL;
	*out_args = NULL;

	/* check if already finished */
	il2cpp_monitor_enter((Il2CppObject*) ares);

	if (ares->endinvoke_called)
	{
		il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("Cannot call EndInvoke() repeatedly or concurrently on the same AsyncResult!"));
		il2cpp_monitor_exit((Il2CppObject*) ares);
		return NULL;
	}

	ares->endinvoke_called = 1;

	/* wait until we are really finished */
	if (ares->completed)
	{
		il2cpp_monitor_exit((Il2CppObject *) ares);
	}
	else
	{

		if (!ares->handle)
		{
			Il2CppWaitHandle *wait_handle = il2cpp::vm::WaitHandle::NewManualResetEvent(false);
			IL2CPP_OBJECT_SETREF(ares, handle, wait_handle);
		}

		il2cpp::os::Handle* wait_event = il2cpp::vm::WaitHandle::GetPlatformHandle((Il2CppWaitHandle*)ares->handle);

		il2cpp_monitor_exit((Il2CppObject*) ares);

		//MONO_ENTER_GC_SAFE;
		wait_event->Wait();
		//MONO_EXIT_GC_SAFE;
	}

	ac = (Il2CppAsyncCall*) ares->object_data;
	IL2CPP_ASSERT(ac);

	il2cpp::gc::WriteBarrier::GenericStore(exc, ((Il2CppMethodMessage*)ac->msg)->exc);
	*out_args = ac->out_args;
	return ac->res;
}

void threadpool_ms_suspend (void)
{
	if (g_ThreadPool)
		g_ThreadPool->suspended = true;
}

void threadpool_ms_resume (void)
{
	if (g_ThreadPool)
		g_ThreadPool->suspended = false;
}

void ves_icall_System_Threading_ThreadPool_GetAvailableThreadsNative (int32_t *worker_threads, int32_t *completion_port_threads)
{
	ThreadPoolCounter counter;

	if (!worker_threads || !completion_port_threads)
		return;

	lazy_initialize ();

	counter.as_int64_t = COUNTER_READ ();

	*worker_threads = std::max (0, g_ThreadPool->limit_worker_max - counter._.active);
	*completion_port_threads = g_ThreadPool->limit_io_max;
}

void ves_icall_System_Threading_ThreadPool_GetMinThreadsNative (int32_t *worker_threads, int32_t *completion_port_threads)
{
	if (!worker_threads || !completion_port_threads)
		return;

	lazy_initialize ();

	*worker_threads = g_ThreadPool->limit_worker_min;
	*completion_port_threads = g_ThreadPool->limit_io_min;
}

void ves_icall_System_Threading_ThreadPool_GetMaxThreadsNative (int32_t *worker_threads, int32_t *completion_port_threads)
{
	if (!worker_threads || !completion_port_threads)
		return;

	lazy_initialize ();

	*worker_threads = g_ThreadPool->limit_worker_max;
	*completion_port_threads = g_ThreadPool->limit_io_max;
}

bool ves_icall_System_Threading_ThreadPool_SetMinThreadsNative (int32_t worker_threads, int32_t completion_port_threads)
{
	lazy_initialize ();

	if (worker_threads <= 0 || worker_threads > g_ThreadPool->limit_worker_max)
		return false;
	if (completion_port_threads <= 0 || completion_port_threads > g_ThreadPool->limit_io_max)
		return false;

	g_ThreadPool->limit_worker_min = worker_threads;
	g_ThreadPool->limit_io_min = completion_port_threads;

	return true;
}

bool ves_icall_System_Threading_ThreadPool_SetMaxThreadsNative (int32_t worker_threads, int32_t completion_port_threads)
{
	int cpu_count = il2cpp::os::Environment::GetProcessorCount ();

	lazy_initialize ();

	if (worker_threads < g_ThreadPool->limit_worker_min || worker_threads < cpu_count)
		return false;
	if (completion_port_threads < g_ThreadPool->limit_io_min || completion_port_threads < cpu_count)
		return false;

	g_ThreadPool->limit_worker_max = worker_threads;
	g_ThreadPool->limit_io_max = completion_port_threads;

	return true;
}

void ves_icall_System_Threading_ThreadPool_InitializeVMTp (bool *enable_worker_tracking)
{
	if (enable_worker_tracking) {
		// TODO implement some kind of switch to have the possibily to use it
		*enable_worker_tracking = false;
	}

	lazy_initialize ();
}

bool ves_icall_System_Threading_ThreadPool_NotifyWorkItemComplete (void)
{
	ThreadPoolCounter counter;

	if (il2cpp::vm::Runtime::IsShuttingDown ())
		return false;

	heuristic_notify_work_completed ();

	if (heuristic_should_adjust ())
		heuristic_adjust ();

	counter.as_int64_t = COUNTER_READ ();
	return counter._.working <= counter._.max_working;
}

void ves_icall_System_Threading_ThreadPool_NotifyWorkItemProgressNative (void)
{
	heuristic_notify_work_completed ();

	if (heuristic_should_adjust ())
		heuristic_adjust ();
}

void ves_icall_System_Threading_ThreadPool_ReportThreadStatus (bool is_working)
{
	// Mono raises a not implemented exception
	IL2CPP_NOT_IMPLEMENTED_ICALL(ves_icall_System_Threading_ThreadPool_PostQueuedCompletionStatus);
	IL2CPP_UNREACHABLE;
}

bool ves_icall_System_Threading_ThreadPool_RequestWorkerThread (void)
{
	return worker_request (il2cpp::vm::Domain::GetCurrent());
}

bool ves_icall_System_Threading_ThreadPool_PostQueuedCompletionStatus (Il2CppNativeOverlapped *native_overlapped)
{
	// Mono raises a not implemented exception
	IL2CPP_NOT_IMPLEMENTED_ICALL(ves_icall_System_Threading_ThreadPool_PostQueuedCompletionStatus);
	IL2CPP_UNREACHABLE;
}

bool  ves_icall_System_Threading_ThreadPool_BindIOCompletionCallbackNative (void* file_handle)
{
	/* This copy the behavior of the current Mono implementation */
	return true;
}

bool ves_icall_System_Threading_ThreadPool_IsThreadPoolHosted (void)
{
	return false;
}
