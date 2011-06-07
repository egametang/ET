#ifndef PYTHON_PYTHON_ENTRY_H
#define PYTHON_PYTHON_ENTRY_H

#include <boost/noncopyable.hpp>
#include <boost/unordered_set.hpp>
#include <boost/shared_ptr.hpp>
#include "Base/Marcos.h"
#include "Python/PythonInit.h"

namespace Egametang {

class PythonEntry: private boost::noncopyable
{
private:
	PythonInit python_init_;

	boost::python::object main_ns_;

	boost::unordered_set<std::string> python_paths_;

	boost::unordered_set<std::string> python_modules_;

private:
	bool GetExecString(const std::string& main_fun, std::string& exec_string);

public:
	PythonEntry();

	void ImportPath(std::string path);

	void ImportModule(std::string module);

	template <typename T>
	void RegisterObjectPtr(std::string name, boost::shared_ptr<T> object_ptr)
	{
		main_ns_[name.c_str()] = object_ptr;
	}

	void Execute(std::string main_fun);
};

} // namespace Egametang

#endif // PYTHON_PYTHON_ENTRY_H
