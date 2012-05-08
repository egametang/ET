#ifndef PYTHON_PYTHONINTERPRETER_H
#define PYTHON_PYTHONINTERPRETER_H

#include <boost/noncopyable.hpp>
#include <unordered_set>
#include <boost/shared_ptr.hpp>
#include "Base/Marcos.h"
#include "Python/PythonInit.h"

namespace Egametang {

class PythonInterpreter: private boost::noncopyable
{
private:
	PythonInit python;
	boost::python::object mainNS;
	std::unordered_set<std::string> paths;
	std::unordered_set<std::string> modules;

private:
	bool GetExecString(const std::string& main_fun, std::string& exec_string);

public:
	PythonInterpreter();

	void ImportPath(std::string path);

	void ImportModule(std::string module);

	template <typename T>
	void RegisterObjectPtr(std::string name, std::shared_ptr<T> object_ptr)
	{
		mainNS[name.c_str()] = object_ptr;
	}

	void Execute(std::string main_fun);
};

} // namespace Egametang

#endif // PYTHON_PYTHONINTERPRETER_H
