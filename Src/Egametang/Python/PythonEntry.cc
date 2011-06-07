#include <boost/foreach.hpp>
#include <boost/format.hpp>
#include <boost/python.hpp>
#include <glog/logging.h>
#include "Python/PythonEntry.h"

namespace Egametang {

PythonEntry::PythonEntry():
		python_init_()
{
	main_ns_ = boost::python::import("__main__").attr("__dict__");
}

void PythonEntry::ImportPath(std::string path)
{
	python_paths_.insert(path);
}

void PythonEntry::ImportModule(std::string module)
{
	python_modules_.insert(module);
}

bool PythonEntry::GetExecString(const std::string& main_fun, std::string& exec_string)
{
	exec_string = "import sys\n";
	if (python_paths_.size() == 0)
	{
		LOG(WARNING) << "no python path";
		return false;
	}
	foreach(std::string path, python_paths_)
	{
		exec_string += boost::str(boost::format("sys.path.append('%1%')\n") % path);
	}

	if (python_modules_.size() == 0)
	{
		LOG(WARNING) << "no python module";
		return false;
	}
	foreach(std::string module, python_modules_)
	{
		exec_string += boost::str(boost::format("import %1%\n") % module);
	}
	exec_string += main_fun;

	return true;
}

void PythonEntry::Execute(std::string main_fun)
{
	std::string exec_string;
	if (!GetExecString(main_fun, exec_string))
	{
		LOG(WARNING) << "no python exec string!";
		return;
	}

	try
	{
		boost::python::exec(exec_string.c_str(), main_ns_);
	}
	catch (...)
	{
		LOG(ERROR) << "python execute error";
		python_init_.PrintError();
	}
}

} // namespace Egametang
