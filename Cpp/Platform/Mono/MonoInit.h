// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef MONO_MONOINIT_H
#define MONO_MONOINIT_H

#include <string>
#include <boost/unordered_map.hpp>
#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>

namespace Egametang {

class MonoInit
{
private:
	MonoDomain *domain;
	boost::unordered_map<std::string, MonoImage*> imageMap;

public:
	MonoInit(const std::string& domainName);
	virtual ~MonoInit();

	void LoadAssembly(const std::string& assemblyName);
	void InvokeMethod(const std::string& className, const std::string& functionName);
	void InvokeMain(const char *file, int argc, char** argv);
};

} // namespace Egametang
#endif // MONO_MONOINIT_H
