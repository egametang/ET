// Copyright 2011 Netease Inc. All Rights Reserved.
// Author: tanghai@corp.netease.com (tanghai)

#include <gtest/gtest.h>
#include <gmock/gmock.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include "Orm/Column.h"

namespace Egametang {

class ColumnTest: public testing::Test
{
};

TEST_F(ColumnTest, Gt)
{
	Column left("age");
	Expr expr = left > 10;
	ASSERT_EQ("age > 10", expr.ToString());
}

TEST_F(ColumnTest, Ge)
{
	Column left("age");
	Expr expr = left >= 10;
	ASSERT_EQ("age >= 10", expr.ToString());
}

TEST_F(ColumnTest, Lt)
{
	Column left("age");
	Expr expr = left < 10;
	ASSERT_EQ("age < 10", expr.ToString());
}

TEST_F(ColumnTest, Le)
{
	Column left("age");
	Expr expr = left <= 10;
	ASSERT_EQ("age <= 10", expr.ToString());
}

TEST_F(ColumnTest, Equal)
{
	Column left("age");
	Expr expr = left == 10;
	ASSERT_EQ("age = 10", expr.ToString());
}

TEST_F(ColumnTest, NotEqual)
{
	Column left("age");
	Expr expr = left != 10;
	ASSERT_EQ("age != 10", expr.ToString());
}

TEST_F(ColumnTest, Like)
{
	Column left("name");
	Expr expr = left.like("tanghai");
	ASSERT_EQ("name like 'tanghai'", expr.ToString());
}

} // namespace Egametang

int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}

