// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <gtest/gtest.h>
#include <gmock/gmock.h>
#include <boost/log/detail/thread_id.hpp>
#include "Log/Log.h"

namespace Egametang {

class LogTest: public testing::Test
{
};

TEST_F(LogTest, Log)
{
	LOG(INFO) << "Test LOG Marco!";
}

} // namespace Egametang

int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	Egametang::Log::Init(argv[0]);
	return RUN_ALL_TESTS();
}