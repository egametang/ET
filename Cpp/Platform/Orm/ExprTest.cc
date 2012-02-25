// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <gtest/gtest.h>
#include <gmock/gmock.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include "Orm/Column.h"

namespace Egametang {

class ExprTest: public testing::Test
{
};

TEST_F(ExprTest, Or)
{
	Column leftColumn("age");
	Oper leftOper(leftColumn, ">", 10);
	Column rightColumn("name");
	Oper rightOper(rightColumn, "like", "%tanghai");
	Or orExpr(leftOper, rightOper);
	ASSERT_EQ("(age > 10) or (name like '%tanghai') ", orExpr.ToString());
}

TEST_F(ExprTest, And)
{
	Column leftColumn("age");
	Oper leftOper(leftColumn, ">", 10);
	Column rightColumn("name");
	Oper rightOper(rightColumn, "like", "%tanghai");
	And andExpr(leftOper, rightOper);
	ASSERT_EQ("(age > 10) and (name like '%tanghai') ", andExpr.ToString());
}

TEST_F(ExprTest, Not)
{
	Column column("age");
	Oper oper(column, ">", 10);
	Not notExpr(oper);
	ASSERT_EQ("not (age > 10) ", notExpr.ToString());
}

} // namespace Egametang

int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}
