#include <boost/python.hpp>
#include "Python/PythonInit.h"

namespace Egametang {

PythonInit::PythonInit()
{
	Py_InitializeEx(0);
}

PythonInit::~PythonInit()
{
	Py_Finalize();
}

bool PythonInit::IsInitialized()
{
	return Py_IsInitialized();
}

const char* PythonInit::Version()
{
	return Py_GetVersion();
}

void PythonInit::PrintError()
{
	PyErr_Print();
}

} // namespace Egametang
