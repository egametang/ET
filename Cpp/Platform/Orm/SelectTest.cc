// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <gtest/gtest.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include "Orm/Select.h"
#include "Orm/Person.pb.h"

namespace Egametang {

class RpcServerTest: public testing::Test
{
};

TEST_F(RpcServerTest, SelectFrom)
{
	std::string expectedSql;
	expectedSql = "select * from Egametang.Person";
	Select<Person> selectQuery1(Column("*"));
	EXPECT_EQ(expectedSql, selectQuery1.ToString());
}

TEST_F(RpcServerTest, SelectWhere)
{
	std::string expectedSql;
	Select<Person> selectQuery(Column("*"));
	expectedSql = "select * from Egametang.Person where age > 10";
	selectQuery.Where(Column("age") > 10);
	EXPECT_EQ(expectedSql, selectQuery.ToString());
}

TEST_F(RpcServerTest, SelectDistinct)
{
	std::string expectedSql;
	Select<Person> selectQuery(Column("*"));
	expectedSql = "select * distinct from Egametang.Person where age > 10";
	selectQuery.Distinct().Where(Column("age") > 10);
	EXPECT_EQ(expectedSql, selectQuery.ToString());
}

TEST_F(RpcServerTest, SelectTwoColumn)
{
	std::string expectedSql;
	Select<Person> selectQuery(Column("age, name"));
	expectedSql = "select age, name distinct from Egametang.Person where age > 10";
	selectQuery.Distinct().Where(Column("age") > 10);
	EXPECT_EQ(expectedSql, selectQuery.ToString());
}

TEST_F(RpcServerTest, LimitOffset)
{
	std::string expectedSql;
	Select<Person> selectQuery(Column("age, name"));
	expectedSql = "select age, name distinct from Egametang.Person where age > 10 limit 1 offset 10";
	selectQuery.Distinct().Where(Column("age") > 10).Limit(1).Offset(10);
	EXPECT_EQ(expectedSql, selectQuery.ToString());
}

TEST_F(RpcServerTest, GroupByHaving)
{
	std::string expectedSql;
	Select<Person> selectQuery(Column("age, name"));
	expectedSql =
			"select age, name distinct from Egametang.Person"
			" group by age having age > 10 limit 1 offset 10";
	selectQuery.Distinct()
			.GroupBy(Column("age"))
			.Having(Column("age") > 10)
			.Limit(1)
			.Offset(10);
	EXPECT_EQ(expectedSql, selectQuery.ToString());
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}
