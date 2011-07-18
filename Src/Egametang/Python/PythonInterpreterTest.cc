#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include <boost/python.hpp>
#include "Python/PythonInterpreter.h"

namespace Egametang {

class PythonInterpreterTest: public testing::Test
{
protected:
	PythonInterpreter python_interpreter_;

public:
	PythonInterpreterTest(): python_interpreter_()
	{
	}

	virtual ~PythonInterpreterTest()
	{
	}
};

class PersonTest
{
private:
	int guid_;
	std::string name_;

public:
	PersonTest(): guid_(0)
	{
	}
	void SetGuid(int guid)
	{
		guid_ = guid;
	}

	int Guid() const
	{
		return guid_;
	}

	void SetName(const std::string& name)
	{
		name_ = name;
	}

	std::string Name() const
	{
		return name_;
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
	python_interpreter_.ImportPath("../../../Src/Egametang/Python/");
	python_interpreter_.ImportModule("PythonInterpreterTest");

	PersonTestPtr person(new PersonTest);
	python_interpreter_.RegisterObjectPtr("person", person);

	ASSERT_EQ(0, person->Guid());

	// 进到python脚本层设置person的值为2
	python_interpreter_.Execute("PythonInterpreterTest.fun(person)");

	ASSERT_EQ(2, person->Guid());
	ASSERT_EQ(std::string("tanghai"), person->Name());
}

} // namespace Egametang

int main(int argc, char* argv[])
{
	FLAGS_logtostderr = true;
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
