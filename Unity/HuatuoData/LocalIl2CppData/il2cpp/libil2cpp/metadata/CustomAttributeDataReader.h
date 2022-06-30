#pragma once

#include <stdint.h>
#include <vector>
#include "il2cpp-object-internals.h"
#include "gc/Allocator.h"

namespace il2cpp
{
namespace metadata
{
    // This union is large enough to store any type
    // that can be serialized by an attribute argument
    // bool, byte, char, double, float, int, long, sbyte, short, string, uint, ulong, ushort, System.Object, System.Type, or an enum
    // Or an szarray/vector of the previous types
    union CustomAttributeDataStorage
    {
        Il2CppObject* obj;
        int64_t i;
        double d;
    };

    struct CustomAttributeArgument
    {
        Il2CppClass* klass;
        CustomAttributeDataStorage data;
    };

    struct CustomAttributeFieldArgument
    {
        CustomAttributeArgument arg;
        const FieldInfo* field;
    };

    struct CustomAttributePropertyArgument
    {
        CustomAttributeArgument arg;
        const PropertyInfo* prop;
    };

    struct LazyCustomAttributeData
    {
        const MethodInfo* ctor;
        const void* dataStart;
        uint32_t dataLength;
    };

    struct CustomAttributeData
    {
        const MethodInfo* ctor;
    };

    class CustomAttributeReaderVisitor
    {
    public:
        // This Visitor methods will be called in the defined order
        virtual void VisitArgumentSizes(uint32_t argumentCount, uint32_t fieldCount, uint32_t propertyCount) {}
        virtual void VisitArgument(const CustomAttributeArgument& argument, uint32_t index) {}
        virtual void VisitCtor(const MethodInfo* ctor, CustomAttributeArgument args[], uint32_t argumentCount) {}
        virtual void VisitField(const CustomAttributeFieldArgument& field, uint32_t index) {}
        virtual void VisitProperty(const CustomAttributePropertyArgument& prop, uint32_t index) {}
    };


    class CustomAttributeDataIterator;

    class CustomAttributeCtorIterator
    {
    private:
        CustomAttributeCtorIterator(const char* ctorBuffer) : ctorBuffer(ctorBuffer)
        {}

        const char* ctorBuffer;

        friend class CustomAttributeDataReader;
        friend class CustomAttributeDataIterator;
    };

    class CustomAttributeDataIterator
    {
    private:
        CustomAttributeDataIterator(const char* ctorBuffer, const char* dataBuffer) : dataBuffer(dataBuffer), ctorIter(ctorBuffer)
        {}

        const char* dataBuffer;
        CustomAttributeCtorIterator ctorIter;

        friend class CustomAttributeDataReader;
    };


    class CustomAttributeDataReader
    {
    public:

        // Creates a CustomAttributeDataReader pointing into the metadata buffer start and end
        // This range must be for a single metadata member
        CustomAttributeDataReader(const void* buffer, const void* bufferEnd);

        // Returns the number of custom attributes stored for the member
        uint32_t GetCount();

        // Iterate through all of the custom attribute constructors
        // Call GetCtorIterator to get the iterator and call this method until it returns false
        bool IterateAttributeCtors(const Il2CppImage* image, const MethodInfo** attributeCtor, CustomAttributeCtorIterator* iter);

        // Iterate through all of the custom attribute data, but only return the attribute type and data range.
        // This method does not allocate
        // On each call LazyCustomAttributeData will be filled with new custom attribute data
        // Call GetDataIterator to get the iterator and call this method until it returns false
        // Call the static VisitCustomAttributeData function to get the attribute arguments from the LazyCustomAttributeData
        // If this function returns false *exc may be non-null if an exception occured
        bool ReadLazyCustomAttributeData(const Il2CppImage* image, LazyCustomAttributeData* data, CustomAttributeDataIterator* iter, Il2CppException** exc);

        // Iterate through all of the custom attribute data and get all of the custom attriubte ctor arguments, fields, and parameter info
        // On each call the CustomAttributeReaderVisitor will be called with the information for the current custom attribute
        // If any of the arguments are managed types (e.g. string, object, arrays) this method will allocate them on the GC heap
        // Call GetDataIterator to get the iterator and call this function until it returns false
        // If this function returns false *exc may be non-null if an exception occured
        bool VisitCustomAttributeData(const Il2CppImage* image, CustomAttributeDataIterator* iter, CustomAttributeReaderVisitor* visitor, Il2CppException** exc);

        // Get the custom attribute ctor arguments, fields, and parameter info for a single custom attribute
        // The CustomAttributeReaderVisitor will be called with the information for this custom attribute
        // Call ReadLazyCustomAttributeData to get the dataStart & dataLength parameters
        // If any of the arguments are managed types (e.g. string, object, arrays) this method will allocate them on the GC heap
        // This method returns false on error
        // If this function returns false *exc may be non-null if an exception occured
        static bool VisitCustomAttributeData(const Il2CppImage* image, const MethodInfo* ctor, const void* dataStart, uint32_t dataLength, CustomAttributeReaderVisitor* visitor, Il2CppException** exc);

        CustomAttributeCtorIterator GetCtorIterator();
        CustomAttributeDataIterator GetDataIterator();

    private:

        const char* GetDataBufferStart();
        CustomAttributeDataReader(const char* dataStart, uint32_t dataLength);
        bool VisitCustomAttributeDataImpl(const Il2CppImage* image, const MethodInfo* ctor, CustomAttributeDataIterator* iter, CustomAttributeReaderVisitor* visitor, Il2CppException** exc, bool deserializedManagedObjects);

        const char* bufferStart;
        const char* bufferEnd;
        uint32_t count;
    };
}         /* namespace vm */
} /* namespace il2cpp */
