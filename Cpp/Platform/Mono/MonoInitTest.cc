// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <gtest/gtest.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include "Mono/MonoInit.h"

namespace Egametang {

class MonoInitTest: public testing::Test
{
protected:
	int port;

public:
	MonoInitTest(): port(10002)
	{
	}
	virtual ~MonoInitTest()
	{
	}
};

TEST_F(MonoInitTest, Echo)
{

}

} // namespace Egametang

int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}
