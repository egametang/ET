#ifndef PYTHON_PYTHON_INIT_H
#define PYTHON_PYTHON_INIT_H

#include <boost/noncopyable.hpp>
#include <boost/python.hpp>

namespace Egametang {

class PythonInit: private boost::noncopyable
{
public:
	PythonInit()
	{
		Py_InitializeEx(0);
	}

	~PythonInit()
	{
		Py_Finalize();
	}

	bool IsInitialized()
	{
		return Py_IsInitialized();
	}
};

} // namespace Egametang

#endif // PYTHON_PYTHON_INIT_H
