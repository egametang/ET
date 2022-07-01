#include "il2cpp-config.h"
#include "gc/GCHandle.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-vm-support.h"
#include "GarbageCollector.h"
#include "os/Mutex.h"
#include "utils/Memory.h"
#include <memory>

namespace il2cpp
{
namespace gc
{
    typedef struct
    {
        uint32_t  *bitmap;
        void* *entries;
        uint32_t   size;
        uint8_t    type;
        uint32_t     slot_hint : 24;/* starting slot for search */
        /* 2^16 appdomains should be enough for everyone (though I know I'll regret this in 20 years) */
        /* we alloc this only for weak refs, since we can get the domain directly in the other cases */
        uint16_t  *domain_ids;
    } HandleData;

/* weak and weak-track arrays will be allocated in malloc memory
 */
    static HandleData gc_handles[] =
    {
        {NULL, NULL, 0, HANDLE_WEAK, 0},
        {NULL, NULL, 0, HANDLE_WEAK_TRACK, 0},
        {NULL, NULL, 0, HANDLE_NORMAL, 0},
        {NULL, NULL, 0, HANDLE_PINNED, 0}
    };


    static int
    find_first_unset(uint32_t bitmap)
    {
        int i;
        for (i = 0; i < 32; ++i)
        {
            if (!(bitmap & (1 << i)))
                return i;
        }
        return -1;
    }

    static baselib::ReentrantLock g_HandlesMutex;

#define lock_handles(handles) g_HandlesMutex.Acquire ()
#define unlock_handles(handles) g_HandlesMutex.Release ()

    static uint32_t
    alloc_handle(HandleData *handles, Il2CppObject *obj, bool track)
    {
        uint32_t slot;
        int i;
        lock_handles(handles);
        if (!handles->size)
        {
            handles->size = 32;
            if (handles->type > HANDLE_WEAK_TRACK)
            {
                handles->entries = (void**)GarbageCollector::AllocateFixed(sizeof(void*) * handles->size, NULL);
            }
            else
            {
                handles->entries = (void**)IL2CPP_MALLOC_ZERO(sizeof(void*) * handles->size);
                handles->domain_ids = (uint16_t*)IL2CPP_MALLOC_ZERO(sizeof(uint16_t) * handles->size);
            }
            handles->bitmap = (uint32_t*)IL2CPP_MALLOC_ZERO(handles->size / 8);
        }
        i = -1;
        for (slot = handles->slot_hint; slot < handles->size / 32; ++slot)
        {
            if (handles->bitmap[slot] != 0xffffffff)
            {
                i = find_first_unset(handles->bitmap[slot]);
                handles->slot_hint = slot;
                break;
            }
        }
        if (i == -1 && handles->slot_hint != 0)
        {
            for (slot = 0; slot < handles->slot_hint; ++slot)
            {
                if (handles->bitmap[slot] != 0xffffffff)
                {
                    i = find_first_unset(handles->bitmap[slot]);
                    handles->slot_hint = slot;
                    break;
                }
            }
        }
        if (i == -1)
        {
            uint32_t *new_bitmap;
            uint32_t new_size = handles->size * 2; /* always double: we memset to 0 based on this below */

            /* resize and copy the bitmap */
            new_bitmap = (uint32_t*)IL2CPP_MALLOC_ZERO(new_size / 8);
            memcpy(new_bitmap, handles->bitmap, handles->size / 8);
            IL2CPP_FREE(handles->bitmap);
            handles->bitmap = new_bitmap;

            /* resize and copy the entries */
            if (handles->type > HANDLE_WEAK_TRACK)
            {
                void* *entries;
                entries = (void**)GarbageCollector::AllocateFixed(sizeof(void*) * new_size, NULL);
                memcpy(entries, handles->entries, sizeof(void*) * handles->size);

                GarbageCollector::SetWriteBarrier(entries, sizeof(void*) * handles->size);

                void** previous_entries = handles->entries;
                handles->entries = entries;
                GarbageCollector::FreeFixed(previous_entries);
            }
            else
            {
                void* *entries;
                uint16_t *domain_ids;
                domain_ids = (uint16_t*)IL2CPP_MALLOC_ZERO(sizeof(uint16_t) * new_size);
                entries = (void**)IL2CPP_MALLOC(sizeof(void*) * new_size);
                /* we disable GC because we could lose some disappearing link updates */
                GarbageCollector::Disable();
                memcpy(entries, handles->entries, sizeof(void*) * handles->size);
                memset(entries + handles->size, 0, sizeof(void*) * handles->size);
                memcpy(domain_ids, handles->domain_ids, sizeof(uint16_t) * handles->size);
                for (i = 0; i < (int32_t)handles->size; ++i)
                {
                    Il2CppObject *obj = GarbageCollector::GetWeakLink(&(handles->entries[i]));
                    if (handles->entries[i])
                        GarbageCollector::RemoveWeakLink(&(handles->entries[i]));
                    /*g_print ("reg/unreg entry %d of type %d at %p to object %p (%p), was: %p\n", i, handles->type, &(entries [i]), obj, entries [i], handles->entries [i]);*/
                    if (obj)
                    {
                        GarbageCollector::AddWeakLink(&(entries[i]), obj, track);
                    }
                }
                IL2CPP_FREE(handles->entries);
                IL2CPP_FREE(handles->domain_ids);
                handles->entries = entries;
                handles->domain_ids = domain_ids;
                GarbageCollector::Enable();
            }

            /* set i and slot to the next free position */
            i = 0;
            slot = (handles->size + 1) / 32;
            handles->slot_hint = handles->size + 1;
            handles->size = new_size;
        }
        handles->bitmap[slot] |= 1 << i;
        slot = slot * 32 + i;
        handles->entries[slot] = obj;
        GarbageCollector::SetWriteBarrier(handles->entries + slot);

        if (handles->type <= HANDLE_WEAK_TRACK)
        {
            if (obj)
                GarbageCollector::AddWeakLink(&(handles->entries[slot]), obj, track);
        }

        //mono_perfcounters->gc_num_handles++;
        unlock_handles(handles);
        /*g_print ("allocated entry %d of type %d to object %p (in slot: %p)\n", slot, handles->type, obj, handles->entries [slot]);*/
        return (slot << 3) | (handles->type + 1);
    }

    uint32_t GCHandle::New(Il2CppObject *obj, bool pinned)
    {
        return alloc_handle(&gc_handles[pinned ? HANDLE_PINNED : HANDLE_NORMAL], obj, false);
    }

    uint32_t GCHandle::NewWeakref(Il2CppObject *obj, bool track_resurrection)
    {
        uint32_t handle = alloc_handle(&gc_handles[track_resurrection ? HANDLE_WEAK_TRACK : HANDLE_WEAK], obj, track_resurrection);

#ifndef HAVE_SGEN_GC
        if (track_resurrection)
            IL2CPP_VM_NOT_SUPPORTED(GCHandle::NewWeakref, "IL2CPP does not support resurrection for weak references. Pass the trackResurrection with a value of false.");
#endif

        return handle;
    }

    GCHandleType GCHandle::GetHandleType(uint32_t gchandle)
    {
        return static_cast<GCHandleType>((gchandle & 7) - 1);
    }

    static inline uint32_t GetHandleSlot(uint32_t gchandle)
    {
        return gchandle >> 3;
    }

    Il2CppObject* GCHandle::GetTarget(uint32_t gchandle)
    {
        uint32_t slot = GetHandleSlot(gchandle);
        uint32_t type = GetHandleType(gchandle);
        HandleData *handles = &gc_handles[type];
        Il2CppObject *obj = NULL;
        if (type > 3)
            return NULL;
        lock_handles(handles);
        if (slot < handles->size && (handles->bitmap[slot / 32] & (1 << (slot % 32))))
        {
            if (handles->type <= HANDLE_WEAK_TRACK)
            {
                obj = GarbageCollector::GetWeakLink(&handles->entries[slot]);
            }
            else
            {
                obj = (Il2CppObject*)handles->entries[slot];
            }
        }
        else
        {
            /* print a warning? */
        }
        unlock_handles(handles);
        /*g_print ("get target of entry %d of type %d: %p\n", slot, handles->type, obj);*/
        return obj;
    }

    static void
    il2cpp_gchandle_set_target(uint32_t gchandle, Il2CppObject *obj)
    {
        uint32_t slot = GetHandleSlot(gchandle);
        uint32_t type = GCHandle::GetHandleType(gchandle);
        HandleData *handles = &gc_handles[type];
        Il2CppObject *old_obj = NULL;

        if (type > 3)
            return;
        lock_handles(handles);
        if (slot < handles->size && (handles->bitmap[slot / 32] & (1 << (slot % 32))))
        {
            if (handles->type <= HANDLE_WEAK_TRACK)
            {
                old_obj = (Il2CppObject*)handles->entries[slot];
                if (handles->entries[slot])
                    GarbageCollector::RemoveWeakLink(&handles->entries[slot]);
                if (obj)
                    GarbageCollector::AddWeakLink(&handles->entries[slot], obj, handles->type == HANDLE_WEAK_TRACK);
            }
            else
            {
                handles->entries[slot] = obj;
            }
        }
        else
        {
            /* print a warning? */
        }
        unlock_handles(handles);

#ifndef HAVE_SGEN_GC
        if (type == HANDLE_WEAK_TRACK)
            IL2CPP_NOT_IMPLEMENTED(il2cpp_gchandle_set_target);
#endif
    }

    void GCHandle::Free(uint32_t gchandle)
    {
        uint32_t slot = GetHandleSlot(gchandle);
        uint32_t type = GetHandleType(gchandle);
        HandleData *handles = &gc_handles[type];
        if (type > 3)
            return;
#ifndef HAVE_SGEN_GC
        if (type == HANDLE_WEAK_TRACK)
            IL2CPP_NOT_IMPLEMENTED(GCHandle::Free);
#endif

        lock_handles(handles);
        if (slot < handles->size && (handles->bitmap[slot / 32] & (1 << (slot % 32))))
        {
            if (handles->type <= HANDLE_WEAK_TRACK)
            {
                if (handles->entries[slot])
                    GarbageCollector::RemoveWeakLink(&handles->entries[slot]);
            }
            else
            {
                handles->entries[slot] = NULL;
            }
            handles->bitmap[slot / 32] &= ~(1 << (slot % 32));
        }
        else
        {
            /* print a warning? */
        }
        //mono_perfcounters->gc_num_handles--;
        /*g_print ("freed entry %d of type %d\n", slot, handles->type);*/
        unlock_handles(handles);
    }

    int32_t GCHandle::GetTargetHandle(Il2CppObject * obj, int32_t handle, int32_t type)
    {
        if (type == -1)
        {
            il2cpp_gchandle_set_target(handle, obj);
            /* the handle doesn't change */
            return handle;
        }
        switch (type)
        {
            case HANDLE_WEAK:
                return NewWeakref(obj, false);
            case HANDLE_WEAK_TRACK:
                return NewWeakref(obj, true);
            case HANDLE_NORMAL:
                return New(obj, false);
            case HANDLE_PINNED:
                return New(obj, true);
            default:
                IL2CPP_ASSERT(0);
        }
        return 0;
    }

    void GCHandle::WalkStrongGCHandleTargets(WalkGCHandleTargetsCallback callback, void* context)
    {
        lock_handles(handles);
        const GCHandleType types[] = { HANDLE_NORMAL, HANDLE_PINNED };

        for (int gcHandleTypeIndex = 0; gcHandleTypeIndex < 2; gcHandleTypeIndex++)
        {
            const HandleData& handles = gc_handles[types[gcHandleTypeIndex]];

            for (uint32_t i = 0; i < handles.size; i++)
            {
                if (handles.entries[i] != NULL)
                    callback(static_cast<Il2CppObject*>(handles.entries[i]), context);
            }
        }
        unlock_handles(handles);
    }
} /* gc */
} /* il2cpp */
