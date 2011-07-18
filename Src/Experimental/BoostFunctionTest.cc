//  Created on: 2010-6-28
//      Author: tanghai

#include <boost/function.hpp>
#include <boost/bind.hpp>
#include <gtest/gtest.h>
#include <gflags/gflags.h>
#include <glog/logging.h>

namespace Egametang {
class BoostTest: public testing::Test
{
protected:
	int a;
	boost::function<int(int)> func;

	void SetUp()
	{
		a = 6;
	}
public:
	BoostTest()
	{
	}

	virtual ~BoostTest()
	{
	}

	int Max(int a, int b)
	{
		LOG(INFO) << a << " " << b;
		return a > b? a : b;
	}
};

TEST_F(BoostTest, Test1)
{
	int x = 5;
	func = boost::bind(&BoostTest::Max, this, _1, x);
	int b = func(a);
	LOG(INFO) << b;
}
}

int main(int argc, char* argv[])
{
	FLAGS_logtostderr = true;
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	google::InitGoogleLogging(argv[0]);
	return RUN_ALL_TESTS();
}
