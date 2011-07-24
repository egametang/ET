#include <boost/foreach.hpp>
#include <boost/format.hpp>
#include <boost/python.hpp>
#include <glog/logging.h>
#include "Python/PythonInterpreter.h"

namespace Egametang {

PythonInterpreter::PythonInterpreter():
		python_init()
{
	main_ns = boost::python::import("__main__").attr("__dict__");
}

void PythonInterpreter::ImportPath(std::string path)
{
	python_paths.insert(path);
}

void PythonInterpreter::ImportModule(std::string module)
{
	python_modules.insert(module);
}

bool PythonInterpreter::GetExecString(const std::string& main_fun, std::string& exec_string)
{
	exec_string = "import sys\n";
	if (python_paths.size() == 0)
	{
		LOG(WARNING) << "no python path";
		return false;
	}
	foreach(std::string path, python_paths)
	{
		exec_string += boost::str(boost::format("sys.path.append('%1%')\n") % path);
	}

	if (python_modules.size() == 0)
	{
		LOG(WARNING) << "no python module";
		return false;
	}
	foreach(std::string module, python_modules)
	{
		exec_string += boost::str(boost::format("import %1%\n") % module);
	}
	exec_string += main_fun;

	return true;
}

void PythonInterpreter::Execute(std::string main_fun)
{
	std::string exec_string;
	if (!GetExecString(main_fun, exec_string))
	{
		LOG(WARNING) << "no python exec string!";
		return;
	}

	try
	{
		boost::python::exec(exec_string.c_str(), main_ns);
	}
	catch (...)
	{
		LOG(ERROR) << "python execute error";
		python_init.PrintError();
	}
}

} // namespace Egametang
