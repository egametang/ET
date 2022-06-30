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

using namespace huatuo::interpreter;

#if HUATUO_TARGET_ARM
//!!!{{INVOKE_STUB

//!!!}}INVOKE_STUB
#endif
