#pragma once

#include <atomic>
#include <stdint.h>

struct Il2CppRuntimeStats
{
    std::atomic<uint64_t> new_object_count;
    std::atomic<uint64_t> initialized_class_count;
    // uint64_t generic_vtable_count;
    // uint64_t used_class_count;
    std::atomic<uint64_t> method_count;
    // uint64_t class_vtable_size;
    std::atomic<uint64_t> class_static_data_size;
    std::atomic<uint64_t> generic_instance_count;
    std::atomic<uint64_t> generic_class_count;
    std::atomic<uint64_t> inflated_method_count;
    std::atomic<uint64_t> inflated_type_count;
    // uint64_t delegate_creations;
    // uint64_t minor_gc_count;
    // uint64_t major_gc_count;
    // uint64_t minor_gc_time_usecs;
    // uint64_t major_gc_time_usecs;
    bool enabled;
};

extern Il2CppRuntimeStats il2cpp_runtime_stats;
