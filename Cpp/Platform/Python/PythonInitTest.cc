#include <boost/python.hpp>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Python/PythonInit.h"

namespace Egametang {

using namespace boost::python;

class PythonInitTest: public testing::Test
{
protected:
	PythonInit python;

public:
	PythonInitTest(): python()
	{
	}

	virtual ~PythonInitTest()
	{
	}
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

} // namespace Egametang

int main(int argc, char* argv[])
{
	FLAGS_logtostderr = true;
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
