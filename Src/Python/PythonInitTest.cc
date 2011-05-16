#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>
#include "Python/PythonInit.h"

namespace Egametang {

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

} // namespace Egametang

int main(int argc, char* argv[])
{
	FLAGS_logtostderr = true;
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
