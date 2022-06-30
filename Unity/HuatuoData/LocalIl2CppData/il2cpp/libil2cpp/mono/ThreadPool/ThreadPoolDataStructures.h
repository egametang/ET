#pragma once

#include <vector>

#include "Baselib.h"
#include "Cpp/Atomic.h"
#include "Cpp/ReentrantLock.h"
#include "Cpp/ConditionVariable.h"

#include "os/Mutex.h"

struct Il2CppDomain;
struct Il2CppInternalThread;

union ThreadPoolCounter
{
    struct
    {
        int16_t max_working; /* determined by heuristic */
        int16_t active; /* executing worker_thread */
        int16_t working; /* actively executing worker_thread, not parked */
        int16_t parked; /* parked */
    } _;
    int64_t as_int64_t;
};

struct ThreadPoolDomain
{
    Il2CppDomain* domain;
    int32_t outstanding_request;
};

struct ThreadPoolHillClimbing
{
    int32_t wave_period;
    int32_t samples_to_measure;
    double target_throughput_ratio;
    double target_signal_to_noise_ratio;
    double max_change_per_second;
    double max_change_per_sample;
    int32_t max_thread_wave_magnitude;
    int32_t sample_interval_low;
    double thread_magnitude_multiplier;
    int32_t sample_interval_high;
    double throughput_error_smoothing_factor;
    double gain_exponent;
    double max_sample_error;

    double current_control_setting;
    int64_t total_samples;
    int16_t last_thread_count;
    double elapsed_since_last_change;
    double completions_since_last_change;

    double average_throughput_noise;

    double *samples;
    double *thread_counts;

    uint32_t current_sample_interval;
    void* random_interval_generator;

    int32_t accumulated_completion_count;
    double accumulated_sample_duration;
};

struct ThreadPool
{
    ThreadPool();

    ThreadPoolCounter counters;

    std::vector<ThreadPoolDomain*> domains;
    baselib::ReentrantLock domains_lock;

    std::vector<Il2CppInternalThread*> working_threads;
    int32_t parked_threads_count;
    baselib::ConditionVariable parked_threads_cond;
    baselib::Lock active_threads_lock; /* protect access to working_threads and parked_threads */

    uint32_t worker_creation_current_second;
    uint32_t worker_creation_current_count;
    baselib::ReentrantLock worker_creation_lock;

    baselib::atomic<int32_t> heuristic_completions;
    int64_t heuristic_sample_start;
    int64_t heuristic_last_dequeue; // ms
    int64_t heuristic_last_adjustment; // ms
    int64_t heuristic_adjustment_interval; // ms
    ThreadPoolHillClimbing heuristic_hill_climbing;
    baselib::ReentrantLock heuristic_lock;

    int32_t limit_worker_min;
    int32_t limit_worker_max;
    int32_t limit_io_min;
    int32_t limit_io_max;

    void* cpu_usage_state;
    int32_t cpu_usage;

    /* suspended by the debugger */
    bool suspended;
};

enum ThreadPoolHeuristicStateTransition
{
    TRANSITION_WARMUP,
    TRANSITION_INITIALIZING,
    TRANSITION_RANDOM_MOVE,
    TRANSITION_CLIMBING_MOVE,
    TRANSITION_CHANGE_POINT,
    TRANSITION_STABILIZING,
    TRANSITION_STARVATION,
    TRANSITION_THREAD_TIMED_OUT,
    TRANSITION_UNDEFINED,
};
