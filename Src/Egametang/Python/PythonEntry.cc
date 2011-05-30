#include <boost/foreach.hpp>
#include <boost/format.hpp>
#include "base/Marcos.h"
#include "Python/PythonEntry.h"

namespace Egametang {

PythonEntry::PythonEntry():
		python_init_()
{
	main_ns_ = import("__main__").attr("__dict__");
}

void PythonEntry::ImportPath(std::string& path)
{
	python_paths_.insert(path);
}

void PythonEntry::ImportModule(std::string& module)
{
	python_modules_.insert(module);
}

template <typename T>
void PythonEntry::RegisterObjectPtr(std::string& name, T object_ptr)
{
	main_ns_[name.c_str()] = object_ptr;
}

bool PythonEntry::GetExecString(const std::string& main_fun, std::string& exec_string)
{
	exec_string = "import sys\n";
	boost::format format;
	if (python_paths_.size() == 0)
	{
		LOG(WARNNING) << "no python path";
		return false;
	}
	foreach(std::string& path, python_paths_)
	{
		exec_string += format("sys.path.append('%1%')\n") % path;
	}

	if (python_modules_.size() == 0)
	{
		LOG(WARNNING) << "no python module";
		return false;
	}
	foreach(std::string& module, python_modules_)
	{
		exec_string += format("import %1%\n") % module;
	}
	exec_string += main_fun;

	return true;
}

void PythonEntry::Exec(std::string& main_fun)
{
	std::string exec_string;
	if (!GetExecString(main_fun, exec_string))
	{
		return;
	}

	try
	{
		boost::python::exec(exec_string.c_str(), main_ns_);
	}
	catch
	{
		LOG(ERROR) << "run python exec error";
		python_init.PrintError();
	}
}

} // namespace Egametang
