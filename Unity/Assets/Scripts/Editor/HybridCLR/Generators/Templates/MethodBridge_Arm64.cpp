#include "MethodBridge.h"

#include <codegen/il2cpp-codegen-metadata.h>
#include "vm/ClassInlines.h"
#include "vm/Object.h"
#include "vm/Class.h"

#include "../metadata/MetadataModule.h"
#include "../metadata/MetadataUtil.h"

#include "Interpreter.h"
#include "MemoryUtil.h"
#include "InstrinctDef.h"

using namespace hybridclr::interpreter;
using hybridclr::GetInterpreterDirectlyCallMethodPointer;

#if HYBRIDCLR_ABI_ARM_64
//!!!{{INVOKE_STUB

//!!!}}INVOKE_STUB
#endif
