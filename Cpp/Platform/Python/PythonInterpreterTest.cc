#include <boost/python.hpp>
#include <boost/make_shared.hpp>
#include <gtest/gtest.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include "Python/PythonInterpreter.h"

namespace Egametang {

class PythonInterpreterTest: public testing::Test
{
protected:
	PythonInterpreter interpreter;

public:
	PythonInterpreterTest(): interpreter()
	{
	}

	virtual ~PythonInterpreterTest()
	{
	}
};

class PersonTest
{
private:
	int guid;
	std::string name;

public:
	PersonTest(): guid(0)
	{
	}
	void SetGuid(int guid)
	{
		this->guid = guid;
	}

	int Guid() const
	{
		return guid;
	}

	void SetName(const std::string& name)
	{
		this->name = name;
	}

	std::string Name() const
	{
		return name;
	}
};

typedef boost::shared_ptr<PersonTest> PersonTestPtr;

BOOST_PYTHON_MODULE(PersonTest)
{
	boost::python::class_<PersonTest>("Person")
		.def("SetGuid", &PersonTest::SetGuid)
		.def("Guid", &PersonTest::Guid)
		.def("SetName", &PersonTest::SetName)
		.def("Name", &PersonTest::Name)
	;
	boost::python::register_ptr_to_python<PersonTestPtr>();
}

TEST_F(PythonInterpreterTest, EnterPythonScript)
{
	initPersonTest();
	interpreter.ImportPath("../../../Cpp/Platform/Python/");
	interpreter.ImportModule("PythonInterpreterTest");

	auto person = boost::make_shared<PersonTest>();
	interpreter.RegisterObjectPtr("person", person);

	ASSERT_EQ(0, person->Guid());

	// 进到python脚本层设置person的值为2
	interpreter.Execute("PythonInterpreterTest.fun(person)");

	ASSERT_EQ(2, person->Guid());
	ASSERT_EQ(std::string("tanghai"), person->Name());
}

} // namespace Egametang

int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}
