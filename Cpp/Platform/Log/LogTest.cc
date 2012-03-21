// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <gtest/gtest.h>
#include <gmock/gmock.h>
#include <gflags/gflags.h>
#include "Log/Log.h"

namespace Egametang {

class LogTest: public testing::Test
{
};

TEST_F(LogTest, Log)
{
	ELOG(INFO) << "Test ELOG Marco!";
}

} // namespace Egametang

int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::ParseCommandLineFlags(&argc, &argv, true);
	Egametang::ELog::Init(argv[0]);
	return RUN_ALL_TESTS();
}




