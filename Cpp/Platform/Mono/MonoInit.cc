// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-gc.h>
#include <mono/metadata/class.h>
#include "Mono/MonoInit.h"

namespace Egametang {

MonoInit::MonoInit(const std::string & domainName)
{
	domain = mono_jit_init(domainName.c_str());
}

MonoInit::~MonoInit()
{
    mono_jit_cleanup(domain);
}

void MonoInit::LoadAssembly(const std::string& fileName)
{
	MonoAssembly* assembly = mono_domain_assembly_open(domain, fileName.c_str());
	MonoImage* image = mono_assembly_get_image(assembly);
	imageMap[fileName] = image;
}

void MonoInit::InvokeMethod(const std::string& className, const std::string& methodName)
{
	MonoImage* image = imageMap[className];
	std::string fullName = className + ":" + methodName;
	MonoMethodDesc* desc = mono_method_desc_new(fullName.c_str(), 1);
	MonoMethod* monoMethod = mono_method_desc_search_in_image(desc, image);
	MonoClass* monoClass = mono_method_get_class(monoMethod);
	MonoObject* newInstance = mono_object_new(domain, monoClass);
	MonoObject* exc;
	MonoObject* ret = mono_runtime_invoke(monoMethod, newInstance, 0, &exc);
}

void MonoInit::InvokeMain(const char *file, int argc, char** argv)
{

}

} // namespace Egametang
