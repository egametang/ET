// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <gtest/gtest.h>
#include <gmock/gmock.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include "Log/Log.h"

namespace Egametang {

class LogTest: public testing::Test
{
};

TEST_F(LogTest, Log)
{
	ELOG(trace) << "Test ELOG Marco!";
}

} // namespace Egametang

int main(int argc, char* argv[])
{
	Egametang::BoostLogInit log(argv[0]);
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}




