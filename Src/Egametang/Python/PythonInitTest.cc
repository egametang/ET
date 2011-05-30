#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Python/PythonInit.h"

namespace Egametang {

using namespace boost::python;

class PythonInitTest: public testing::Test
{
public:
	PythonInitTest(): python_init()
	{}

protected:
	PythonInit python_init;
};

TEST_F(PythonInitTest, Int)
{
	boost::python::object i(10);
	i = 10 * i;
	ASSERT_EQ(100, boost::python::extract<int>(i));
}

TEST_F(PythonInitTest, String)
{
	boost::python::object i("ab");
	std::string str = boost::python::extract<std::string>(i * 5);
	ASSERT_EQ("ababababab", str);
}

TEST_F(PythonInitTest, List)
{
	boost::python::list list;
	list.append("zelda");
	list.append(2.236);
	ASSERT_EQ(2, boost::python::len(list));
	ASSERT_EQ(1, list.count("zelda"));
	ASSERT_FLOAT_EQ(2.236, boost::python::extract<double>(list[1]));
}

TEST_F(PythonInitTest, Tuple)
{
	boost::python::tuple tuple = boost::python::make_tuple("metroid", "samus", "ridley");
	ASSERT_EQ(3, boost::python::len(tuple));
	ASSERT_EQ("samus", std::string(boost::python::extract<std::string>(tuple[-2])));
}

TEST_F(PythonInitTest, Dict)
{
	boost::python::dict dict;
	dict["mario"] = "peach";
	dict[0] = "killer7";
	ASSERT_TRUE(dict.has_key(0));
	ASSERT_EQ(2, boost::python::len(dict));
}

class Person
{
private:
	int guid_;
	std::string name_;

public:
	Person(): guid_(0)
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

typedef boost::shared_ptr<Person> PersonPtr;

BOOST_PYTHON_MODULE(Person)
{
	boost::python::class_<Person>("Person")
		.def("SetGuid", &Person::SetGuid)
		.def("Guid", &Person::Guid)
	;
	boost::python::register_ptr_to_python<PersonPtr>();
}

TEST_F(PythonInitTest, EnterPythonScript)
{
	try
	{
		initPerson();
		boost::python::object main_module = boost::python::import("__main__");
		boost::python::object main_namespace = main_module.attr("__dict__");
		PersonPtr person(new Person);
		main_namespace["person"] = person;
		std::string str = "import sys\n"
				"sys.path.append('../../../Src/Egametang/Python/')\n"
				"import Person\n"
				"import PythonInitTest\n"
				"PythonInitTest.fun(person)\n";
		ASSERT_EQ(0, person->Guid());

		// 进到python脚本层设置person的值为2
		boost::python::exec(str.c_str(), main_namespace);
		ASSERT_EQ(2, person->Guid());
	}
	catch (boost::python::error_already_set& err)
	{
		python_init.PrintError();
		throw err;
	}
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
