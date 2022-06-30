#include "MetadataModule.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"
#include "os/Atomic.h"
#include "os/Mutex.h"
#include "os/File.h"
#include "vm/Exception.h"
#include "vm/String.h"
#include "vm/Assembly.h"
#include "vm/Class.h"
#include "vm/Object.h"
#include "vm/Image.h"
#include "vm/MetadataLock.h"
#include "utils/Logging.h"
#include "utils/MemoryMappedFile.h"
#include "utils/Memory.h"

#include "InterpreterImage.h"
#include "AOTHomologousImage.h"

using namespace il2cpp;

namespace huatuo
{

namespace metadata
{

    void MetadataModule::Initialize()
    {
        InterpreterImage::Initialize();
    }

}
}
