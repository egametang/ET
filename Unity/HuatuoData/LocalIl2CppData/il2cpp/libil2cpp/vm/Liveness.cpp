#include "il2cpp-config.h"
#include "gc/GarbageCollector.h"
#include <utils/dynamic_array.h>
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/ClassInlines.h"
#include "vm/Field.h"
#include "vm/Liveness.h"
#include "vm/Type.h"
#include "il2cpp-tabledefs.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"

typedef il2cpp::utils::dynamic_array<Il2CppObject*> custom_growable_array;

#define MARK_OBJ(obj) \
    do { \
        (obj)->klass = (Il2CppClass*)(((size_t)(obj)->klass) | (size_t)1); \
    } while (0)

#define CLEAR_OBJ(obj) \
    do { \
        (obj)->klass = (Il2CppClass*)(((size_t)(obj)->klass) & ~(size_t)1); \
    } while (0)

#define IS_MARKED(obj) \
    (((size_t)(obj)->klass) & (size_t)1)

#define GET_CLASS(obj) \
    ((Il2CppClass*)(((size_t)(obj)->klass) & ~(size_t)1))

namespace il2cpp
{
namespace vm
{
    /* number of sub elements of an array to process before recursing
    * we take a depth first approach to use stack space rather than re-allocating
    * processing array which requires restarting world to ensure allocator lock is not held
    */
    const int kArrayElementsPerChunk = 256;

    /* how far we recurse processing array elements before we stop. Prevents stack overflow */
    const int kMaxTraverseRecursionDepth = 128;

    struct LivenessState
    {
        LivenessState(Il2CppClass* filter, uint32_t maxCount, Liveness::register_object_callback callback, void*callback_userdata, Liveness::WorldChangedCallback onWorldStarted, Liveness::WorldChangedCallback onWorldStopped);
        ~LivenessState();

        void Finalize();
        void Reset();
        void TraverseObjects();
        void FilterObjects();

        static void TraverseGenericObject(Il2CppObject* object, LivenessState* state);
        static void TraverseObject(Il2CppObject* object, LivenessState* state);
        static void TraverseGCDescriptor(Il2CppObject* object, LivenessState* state);
        static bool TraverseObjectInternal(Il2CppObject* object, bool isStruct, Il2CppClass* klass, LivenessState* state);
        static void TraverseArray(Il2CppArray* array, LivenessState* state);
        static bool AddProcessObject(Il2CppObject* object, LivenessState* state);
        static bool ShouldProcessValue(Il2CppObject* val, Il2CppClass* filter);
        static bool FieldCanContainReferences(FieldInfo* field);
        void SafeGrowArray(custom_growable_array* array);

        static bool ShouldTraverseObjects(size_t index, int32_t recursion_depth)
        {
            // Add kArrayElementsPerChunk objects at a time and then traverse
            return ((index + 1) & (kArrayElementsPerChunk - 1)) == 0 && recursion_depth < kMaxTraverseRecursionDepth;
        }

        int32_t                first_index_in_all_objects;
        custom_growable_array* all_objects;

        Il2CppClass*          filter;

        custom_growable_array* process_array;
        uint32_t               initial_alloc_count;

        void*               callback_userdata;

        Liveness::register_object_callback filter_callback;
        Liveness::WorldChangedCallback onWorldStarted;
        Liveness::WorldChangedCallback onWorldStopped;
        int32_t               traverse_depth; // track recursion. Prevent stack overflow by limiting recurion
    };

    LivenessState::LivenessState(Il2CppClass* filter, uint32_t maxCount, Liveness::register_object_callback callback, void*callback_userdata, Liveness::WorldChangedCallback onWorldStarted, Liveness::WorldChangedCallback onWorldStopped) :
        first_index_in_all_objects(0),
        all_objects(NULL),
        filter(NULL),
        process_array(NULL),
        initial_alloc_count(0),
        callback_userdata(NULL),
        filter_callback(NULL),
        onWorldStarted(onWorldStarted),
        onWorldStopped(onWorldStopped),
        traverse_depth(0)
    {
// construct liveness_state;
// allocate memory for the following structs
// all_objects: contains a list of all referenced objects to be able to clean the vtable bits after the traversal
// process_array. array that contains the objcets that should be processed. this should run depth first to reduce memory usage
// if all_objects run out of space, run through list, add objects that match the filter, clear bit in vtable and then clear the array.

        maxCount = maxCount < 1000 ? 1000 : maxCount;
        all_objects = new custom_growable_array();
        all_objects->reserve(maxCount * 4);
        process_array = new custom_growable_array();
        process_array->reserve(maxCount);

        first_index_in_all_objects = 0;
        this->filter = filter;

        this->callback_userdata = callback_userdata;
        this->filter_callback = callback;
    }

    LivenessState::~LivenessState()
    {
        delete all_objects;
        delete process_array;
    }

    void LivenessState::Finalize()
    {
        for (size_t i = 0; i < all_objects->size(); i++)
        {
            Il2CppObject* object = (*all_objects)[i];
            CLEAR_OBJ(object);
        }
    }

    void LivenessState::Reset()
    {
        first_index_in_all_objects = (int32_t)all_objects->size();
        process_array->resize_uninitialized(0);
    }

    void LivenessState::TraverseObjects()
    {
        Il2CppObject* object = NULL;

        traverse_depth++;
        while (process_array->size() > 0)
        {
            object = process_array->back();
            process_array->pop_back();
            TraverseGenericObject(object, this);
        }
        traverse_depth--;
    }

    void LivenessState::FilterObjects()
    {
        Il2CppObject* filtered_objects[64];
        int32_t num_objects = 0;

        size_t i = (size_t)first_index_in_all_objects;
        for (; i < all_objects->size(); i++)
        {
            Il2CppObject* object = (*all_objects)[i];
            if (ShouldProcessValue(object, filter))
                filtered_objects[num_objects++] = object;
            if (num_objects == 64)
            {
                filter_callback(filtered_objects, 64, callback_userdata);
                num_objects = 0;
            }
        }

        if (num_objects != 0)
            filter_callback(filtered_objects, num_objects, callback_userdata);
    }

    void LivenessState::TraverseGenericObject(Il2CppObject* object, LivenessState* state)
    {
        IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(LivenessState::TraverseGenericObject, "Use GC bitmap when we have one");

#if IL2CPP_HAS_GC_DESCRIPTORS
        size_t gc_desc = (size_t)(GET_CLASS(object)->gc_desc);

        if (gc_desc & (size_t)1)
            TraverseGCDescriptor(object, state);
        else
#endif
        if (GET_CLASS(object)->rank)
            TraverseArray((Il2CppArray*)object, state);
        else
            TraverseObject(object, state);
    }

    void LivenessState::TraverseObject(Il2CppObject* object, LivenessState* state)
    {
        TraverseObjectInternal(object, false, GET_CLASS(object), state);
    }

    void LivenessState::TraverseGCDescriptor(Il2CppObject* object, LivenessState* state)
    {
#define WORDSIZE ((int)sizeof(size_t)*8)
        int i = 0;
        size_t mask = (size_t)(GET_CLASS(object)->gc_desc);

        IL2CPP_ASSERT(mask & (size_t)1);

        for (i = 0; i < WORDSIZE - 2; i++)
        {
            size_t offset = ((size_t)1 << (WORDSIZE - 1 - i));
            if (mask & offset)
            {
                Il2CppObject* val = *(Il2CppObject**)(((char*)object) + i * sizeof(void*));
                AddProcessObject(val, state);
            }
        }
    }

    bool LivenessState::TraverseObjectInternal(Il2CppObject* object, bool isStruct, Il2CppClass* klass, LivenessState* state)
    {
        FieldInfo *field;
        Il2CppClass *p;
        bool added_objects = false;

        IL2CPP_ASSERT(object);

        if (!klass->initialized)
        {
            IL2CPP_ASSERT(isStruct);
            return false;
        }

        // subtract the added offset for the vtable. This is added to the offset even though it is a struct
        if (isStruct)
            object--;

        for (p = klass; p != NULL; p = p->parent)
        {
            void* iter = NULL;
            while ((field = Class::GetFields(p, &iter)))
            {
                if (field->type->attrs & FIELD_ATTRIBUTE_STATIC)
                    continue;

                if (!FieldCanContainReferences(field))
                    continue;

                if (Type::IsStruct(field->type))
                {
                    char* offseted = (char*)object;
                    offseted += field->offset;
                    if (Type::IsGenericInstance(field->type))
                    {
                        IL2CPP_ASSERT(field->type->data.generic_class->cached_class);
                        added_objects |= TraverseObjectInternal((Il2CppObject*)offseted, true, field->type->data.generic_class->cached_class, state);
                    }
                    else
                        added_objects |= TraverseObjectInternal((Il2CppObject*)offseted, true, Type::GetClass(field->type), state);
                    continue;
                }

                if (field->offset == THREAD_STATIC_FIELD_OFFSET)
                {
                    IL2CPP_ASSERT(0);
                }
                else
                {
                    Il2CppObject* val = NULL;
                    Field::GetValue(object, field, &val);
                    added_objects |= AddProcessObject(val, state);
                }
            }
        }

        return added_objects;
    }

    void LivenessState::TraverseArray(Il2CppArray* array, LivenessState* state)
    {
        size_t i = 0;
        bool has_references;
        Il2CppObject* object = (Il2CppObject*)array;
        Il2CppClass* element_class;
        size_t elementClassSize;
        size_t array_length;

        IL2CPP_ASSERT(object);

        element_class = GET_CLASS(object)->element_class;
        has_references = !Class::IsValuetype(element_class);
        IL2CPP_ASSERT(element_class->size_inited != 0);

        FieldInfo* field;
        void* iter = NULL;
        while ((field = Class::GetFields(element_class, &iter)))
        {
            has_references |= FieldCanContainReferences(field);
            if (has_references)
                break;
        }

        if (!has_references)
            return;

        array_length = Array::GetLength(array);
        if (element_class->valuetype)
        {
            size_t items_processed = 0;
            elementClassSize = Class::GetArrayElementSize(element_class);
            for (i = 0; i < array_length; i++)
            {
                Il2CppObject* object = (Il2CppObject*)il2cpp_array_addr_with_size(array, (int32_t)elementClassSize, i);
                if (TraverseObjectInternal(object, 1, element_class, state))
                    items_processed++;

                // Add 64 objects at a time and then traverse
                if (ShouldTraverseObjects(items_processed, state->traverse_depth))
                    state->TraverseObjects();
            }
        }
        else
        {
            size_t items_processed = 0;
            for (i = 0; i < array_length; i++)
            {
                Il2CppObject* val =  il2cpp_array_get(array, Il2CppObject*, i);
                if (AddProcessObject(val, state))
                    items_processed++;

                // Add 64 objects at a time and then traverse
                if (ShouldTraverseObjects(items_processed, state->traverse_depth))
                    state->TraverseObjects();
            }
        }
    }

    bool LivenessState::AddProcessObject(Il2CppObject* object, LivenessState* state)
    {
        if (!object || IS_MARKED(object))
            return false;

        bool has_references = GET_CLASS(object)->has_references;
        if (has_references || ShouldProcessValue(object, state->filter))
        {
            // TODO
            if (state->all_objects->size() == state->all_objects->capacity())
                state->SafeGrowArray(state->all_objects);
            state->all_objects->push_back(object);
            MARK_OBJ(object);
        }
        // Check if klass has further references - if not skip adding
        if (has_references)
        {
            if (state->process_array->size() == state->process_array->capacity())
                state->SafeGrowArray(state->process_array);
            state->process_array->push_back(object);
            return true;
        }

        return false;
    }

    bool LivenessState::ShouldProcessValue(Il2CppObject* val, Il2CppClass* filter)
    {
        Il2CppClass* val_class = GET_CLASS(val);
        if (filter && !ClassInlines::HasParentUnsafe(val_class, filter))
            return false;

        return true;
    }

    bool LivenessState::FieldCanContainReferences(FieldInfo* field)
    {
        if (Type::IsStruct(field->type))
            return true;
        if (field->type->attrs & FIELD_ATTRIBUTE_LITERAL)
            return false;
        if (field->type->type == IL2CPP_TYPE_STRING)
            return false;
        return Type::IsReference(field->type);
    }

    void LivenessState::SafeGrowArray(custom_growable_array* array)
    {
        // if all_objects run out of space, run through list
        // clear bit in vtable, start the world, reallocate, stop the world and continue
        for (size_t i = 0; i < all_objects->size(); i++)
        {
            Il2CppObject* object = (*all_objects)[i];
            CLEAR_OBJ(object);
        }
        Liveness::StartWorld(onWorldStarted);
        array->reserve(array->capacity() * 2);
        Liveness::StopWorld(onWorldStopped);
        for (size_t i = 0; i < all_objects->size(); i++)
        {
            Il2CppObject* object = (*all_objects)[i];
            MARK_OBJ(object);
        }
    }

    void* Liveness::Begin(Il2CppClass* filter, int max_object_count, register_object_callback callback, void* userdata, WorldChangedCallback onWorldStarted, WorldChangedCallback onWorldStopped)
    {
        // ensure filter is initialized so we can do fast (and lock free) check HasParentUnsafe
        Class::SetupTypeHierarchy(filter);
        LivenessState* state = new LivenessState(filter, max_object_count, callback, userdata, onWorldStarted, onWorldStopped);
        StopWorld(onWorldStopped);
        // no allocations can happen beyond this point
        return state;
    }

    void Liveness::End(void* state)
    {
        LivenessState* lstate = (LivenessState*)state;
        lstate->Finalize();

        WorldChangedCallback onWorldStarted = lstate->onWorldStarted;
        StartWorld(onWorldStarted);

        delete lstate;
    }

    void Liveness::FromRoot(Il2CppObject* root, void* state)
    {
        LivenessState* liveness_state = (LivenessState*)state;
        liveness_state->Reset();

        liveness_state->process_array->push_back(root);

        liveness_state->TraverseObjects();

        //Filter objects and call callback to register found objects
        liveness_state->FilterObjects();
    }

    void Liveness::FromStatics(void* state)
    {
        LivenessState* liveness_state = (LivenessState*)state;
        const il2cpp::utils::dynamic_array<Il2CppClass*>& classesWithStatics = Class::GetStaticFieldData();

        liveness_state->Reset();

        for (il2cpp::utils::dynamic_array<Il2CppClass*>::const_iterator iter = classesWithStatics.begin();
             iter != classesWithStatics.end();
             iter++)
        {
            Il2CppClass* klass = *iter;
            FieldInfo *field;
            if (!klass)
                continue;
            if (klass->image == il2cpp_defaults.corlib)
                continue;
            if (klass->size_inited == 0)
                continue;

            void* fieldIter = NULL;
            while ((field = Class::GetFields(klass, &fieldIter)))
            {
                if (!(field->type->attrs & FIELD_ATTRIBUTE_STATIC))
                    continue;
                if (!LivenessState::FieldCanContainReferences(field))
                    continue;
                // shortcut check for thread-static field
                if (field->offset == THREAD_STATIC_FIELD_OFFSET)
                    continue;

                if (Type::IsStruct(field->type))
                {
                    char* offseted = (char*)klass->static_fields;
                    offseted += field->offset;
                    if (Type::IsGenericInstance(field->type))
                    {
                        IL2CPP_ASSERT(field->type->data.generic_class->cached_class);
                        LivenessState::TraverseObjectInternal((Il2CppObject*)offseted, true, field->type->data.generic_class->cached_class, liveness_state);
                    }
                    else
                    {
                        LivenessState::TraverseObjectInternal((Il2CppObject*)offseted, true, Type::GetClass(field->type), liveness_state);
                    }
                }
                else
                {
                    Il2CppObject* val = NULL;

                    Field::StaticGetValue(field, &val);

                    if (val)
                    {
                        LivenessState::AddProcessObject(val, liveness_state);
                    }
                }
            }
        }
        liveness_state->TraverseObjects();
        //Filter objects and call callback to register found objects
        liveness_state->FilterObjects();
    }

    void Liveness::StopWorld(WorldChangedCallback onWorldStopped)
    {
        onWorldStopped();
        il2cpp::gc::GarbageCollector::StopWorld();
    }

    void Liveness::StartWorld(WorldChangedCallback onWorldStarted)
    {
        il2cpp::gc::GarbageCollector::StartWorld();
        onWorldStarted();
    }
} /* namespace vm */
} /* namespace il2cpp */
