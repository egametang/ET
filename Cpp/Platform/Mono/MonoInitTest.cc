// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <gtest/gtest.h>
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
	return RUN_ALL_TESTS();
}
