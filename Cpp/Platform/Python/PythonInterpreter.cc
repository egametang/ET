#include <boost/foreach.hpp>
#include <boost/format.hpp>
#include <boost/python.hpp>
#include "Python/PythonInterpreter.h"

namespace Egametang {

PythonInterpreter::PythonInterpreter():
		python()
{
	mainNS = boost::python::import("__main__").attr("__dict__");
}

void PythonInterpreter::ImportPath(std::string path)
{
	paths.insert(path);
}

void PythonInterpreter::ImportModule(std::string module)
{
	modules.insert(module);
}

bool PythonInterpreter::GetExecString(const std::string& main_fun, std::string& exec_string)
{
	exec_string = "import sys\n";
	if (paths.size() == 0)
	{
		LOG(WARNING) << "no python path";
		return false;
	}
	foreach (std::string path, paths)
	{
		exec_string += boost::str(boost::format("sys.path.append('%1%')\n") % path);
	}

	if (modules.size() == 0)
	{
		LOG(WARNING) << "no python module";
		return false;
	}
	foreach (std::string module, modules)
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
		boost::python::exec(exec_string.c_str(), mainNS);
	}
	catch (...)
	{
		LOG(ERROR) << "python execute error";
		python.PrintError();
	}
}

} // namespace Egametang
