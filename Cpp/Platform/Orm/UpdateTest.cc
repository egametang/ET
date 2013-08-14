// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <gtest/gtest.h>
#include "Orm/Column.h"
#include "Orm/Exception.h"
#include "Orm/Update.h"
#include "Orm/Person.pb.h"

namespace Egametang {

class UpdateTest: public testing::Test
{
};

TEST_F(UpdateTest, Update_OneField)
{
	std::string expectedSql;
	expectedSql = "update Egametang.Person set guid = 1";
	Person person;
	person.set_guid(1);
	Update update(person);
	EXPECT_EQ(expectedSql, update.ToString());
}

TEST_F(UpdateTest, Update_MutiField)
{
	std::string expectedSql;
	expectedSql =
			"update Egametang.Person "
			"set guid = 1, age = 18, comment = 'a good student!'";
	Person person;
	person.set_guid(1);
	person.set_age(18);
	person.set_comment("a good student!");
	Update update(person);
	EXPECT_EQ(expectedSql, update.ToString());
}

TEST_F(UpdateTest, Update_Where)
{
	std::string expectedSql;
	expectedSql =
			"update Egametang.Person "
			"set guid = 1, age = 18, comment = 'a good student!' "
			"where age > 10";
	Person person;
	person.set_guid(1);
	person.set_age(18);
	person.set_comment("a good student!");
	Update update(person);
	update.Where(Column("age") > 10);
	EXPECT_EQ(expectedSql, update.ToString());
}

TEST_F(UpdateTest, Update_ThrowException)
{
	std::string expectedSql;
	Person person;
	Update update1(person);
	EXPECT_THROW(update1.Where(Column("age") == 1).ToString(), MessageNoFeildIsSetException);

	person.set_guid(1);
	Update update2(person);
	EXPECT_THROW(update2.Where(Column("color") == 1), MessageHasNoSuchFeildException);
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	return RUN_ALL_TESTS();
}
