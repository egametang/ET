#include <gtest/gtest.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include "Orm/Select.h"
#include "Orm/Person.pb.h"

namespace Egametang {

class RpcServerTest: public testing::Test
{
public:

};

TEST_F(RpcServerTest, ToString)
{
	std::string expectedSql;
	expectedSql = "select * from Egametang.Person";
	Select<Person> selectQuery1(Column("*"));
	EXPECT_EQ(expectedSql, selectQuery1.ToString());

	Select<Person> selectQuery2(Column("*"));
	expectedSql = "select * from Egametang.Person where age > 10";
	selectQuery2.Where(Column("age") > 10);
	EXPECT_EQ(expectedSql, selectQuery2.ToString());

	Select<Person> selectQuery3(Column("*"));
	expectedSql = "select * distinct from Egametang.Person where age > 10";
	selectQuery3.Distinct().Where(Column("age") > 10);
	EXPECT_EQ(expectedSql, selectQuery3.ToString());

	Select<Person> selectQuery4(Column("age, name"));
	expectedSql = "select age, name distinct from Egametang.Person where age > 10";
	selectQuery4.Distinct().Where(Column("age") > 10);
	EXPECT_EQ(expectedSql, selectQuery4.ToString());

	Select<Person> selectQuery5(Column("age, name"));
	expectedSql = "select age, name distinct from Egametang.Person where age > 10 limit 1 offset 10";
	selectQuery5.Distinct().Where(Column("age") > 10).Limit(1).Offset(10);
	EXPECT_EQ(expectedSql, selectQuery5.ToString());

	Select<Person> selectQuery6(Column("age, name"));
	expectedSql =
			"select age, name distinct from Egametang.Person"
			" group by age having age > 10 limit 1 offset 10";
	selectQuery6.Distinct()
			.GroupBy(Column("age"))
			.Having(Column("age") > 10)
			.Limit(1)
			.Offset(10);
	EXPECT_EQ(expectedSql, selectQuery6.ToString());
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}
