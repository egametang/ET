#pragma once

#include "metadata/CustomAttributeDataReader.h"
#include "il2cpp-object-internals.h"
#include "vm-utils/VmThreadUtils.h"

namespace il2cpp
{
namespace metadata
{
    class CustomAttributeCreator : public CustomAttributeReaderVisitor
    {
    private:
        Il2CppObject* attr;
        Il2CppException* exc;

    public:

        CustomAttributeCreator() : attr(NULL), exc(NULL)
        {
            // As currently implemented CustomAttributeCreator must be stack allocated
            // It directly stores managed objects attr & exc so it must be either on the stack
            // or in GC allocated memory
            IL2CPP_ASSERT_STACK_PTR(this);
        }

        virtual void VisitCtor(const MethodInfo* ctor, CustomAttributeArgument args[], uint32_t argumentCount);
        virtual void VisitField(const CustomAttributeFieldArgument& field, uint32_t index);
        virtual void VisitProperty(const CustomAttributePropertyArgument& prop, uint32_t index);

        Il2CppObject* GetAttribute(Il2CppException** exc);

    private:
    };
} /* namespace vm */
} /* namespace il2cpp */
