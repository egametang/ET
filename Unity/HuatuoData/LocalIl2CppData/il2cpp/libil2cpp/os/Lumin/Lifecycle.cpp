#include "il2cpp-config.h"

#if IL2CPP_TARGET_LUMIN

#include "Lifecycle.h"

#include "ml_lifecycle.h"
/*!
    \brief Path to the writable dir of the application. This path is valid when the
    user is logged in and using the device, i.e the device is unlocked. This path
    is not available when device is locked.
   */
std::string s_WritableDirPath = "";
/*! Path to the application package dir. */
std::string s_PackageDirPath = "";
/*! Package name of the application. */
std::string s_PackageName = "";
/*! Component name of the application. */
std::string s_ComponentName = "";
/*! Path to the application tmp dir. */
std::string s_TmpDirPath = "";
/*! \brief Visible name of the application */
std::string s_VisibleName = "";


namespace il2cpp
{
namespace os
{
namespace lumin
{
    std::string GetPackageName() { return s_PackageName; }

    std::string GetPackageTempPath() { return s_TmpDirPath; }

    void LifecycleInit()
    {
        MLLifecycleSelfInfo* info = NULL;
        if (MLLifecycleGetSelfInfo(&info) == MLResult_Ok)
        {
            s_PackageName = info->package_name;
            s_TmpDirPath = info->tmp_dir_path;
            s_WritableDirPath = info->writable_dir_path;

            MLLifecycleFreeSelfInfo(&info);
        }
    }
}
}
}
#endif
