#ifndef PYTHON_PYTHON_INIT_H
#define PYTHON_PYTHON_INIT_H

#include <boost/noncopyable.hpp>

namespace Egametang {

class PythonInit: private boost::noncopyable
{
public:
	PythonInit();

	~PythonInit();

	bool IsInitialized();

	const char* Version();

	void PrintError();
};

} // namespace Egametang

#endif // PYTHON_PYTHON_INIT_H
