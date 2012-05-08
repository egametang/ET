// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <sstream>
#include <memory>
#include <gtest/gtest.h>
#include <gmock/gmock.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include <google/protobuf/descriptor.h>
#include "Orm/MessageField.h"
#include "Orm/Person.pb.h"
#include "Orm/ResultSetMock.h"

namespace Egametang {

using testing::_;
using testing::Invoke;
using testing::Return;
using testing::An;

class MessageFieldTest: public testing::Test
{
};

TEST_F(MessageFieldTest, GetField_FieldIsInt32)
{
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(1);

	std::string str;
	MessageField messageField(person, field);

	person.set_age(10);
	str = messageField.GetField();
	ASSERT_EQ("10", str);

	person.set_age(-10);
	str = messageField.GetField();
	ASSERT_EQ("-10", str);
}

TEST_F(MessageFieldTest, GetField_FieldIsUInt32)
{
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(2);

	std::string str;
	MessageField messageField(person, field);

	person.set_number(10);
	str = messageField.GetField();
	ASSERT_EQ("10", str);

	person.set_number(-10);
	str = messageField.GetField();
	ASSERT_EQ("4294967286", str);
}

TEST_F(MessageFieldTest, GetField_FieldIsUInt64)
{
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(3);

	std::string str;
	MessageField messageField(person, field);

	person.set_time(33333333333333LL);
	str = messageField.GetField();
	ASSERT_EQ("33333333333333", str);

	person.set_time(-33333333333333LL);
	str = messageField.GetField();
	ASSERT_EQ("18446710740376218283", str);
}

TEST_F(MessageFieldTest, GetField_FieldIsInt64)
{
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(0);

	std::string str;
	MessageField messageField(person, field);

	person.set_guid(33333333333333LL);
	str = messageField.GetField();
	ASSERT_EQ("33333333333333", str);

	person.set_guid(-33333333333333LL);
	str = messageField.GetField();
	ASSERT_EQ("-33333333333333", str);
}

TEST_F(MessageFieldTest, GetField_FieldIsString)
{
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(4);

	std::string str;
	MessageField messageField(person, field);

	person.set_name("tanghai");
	str = messageField.GetField();
	ASSERT_EQ("'tanghai'", str);
}

TEST_F(MessageFieldTest, GetField_FieldIsDouble)
{
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(5);

	std::string str;
	MessageField messageField(person, field);

	person.set_height(1.78);
	str = messageField.GetField();
	ASSERT_EQ("1.78", str);
}

TEST_F(MessageFieldTest, GetField_FieldIsBytes)
{
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(6);

	std::string str;
	MessageField messageField(person, field);

	person.set_comment("tanghai is a good student!");
	str = messageField.GetField();
	ASSERT_EQ("'tanghai is a good student!'", str);
}

TEST_F(MessageFieldTest, GetField_FieldIsBool)
{
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(7);

	std::string str;
	MessageField messageField(person, field);

	person.set_marry(false);
	str = messageField.GetField();
	ASSERT_EQ("0", str);

	person.set_marry(true);
	str = messageField.GetField();
	ASSERT_EQ("1", str);
}

TEST_F(MessageFieldTest, GetField_FieldIsMessage)
{
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(8);

	std::string str;
	MessageField messageField(person, field);

	person.mutable_item()->set_id(123);
	person.mutable_item()->set_name("pen");
	str = messageField.GetField();
	ASSERT_EQ("id: 123 name: \"pen\"", str);
}

TEST_F(MessageFieldTest, SetField_FieldIsBool)
{
	auto resultSetMock = std::make_shared<ResultSetMock>();
	EXPECT_CALL(*resultSetMock, getBoolean(8))
		.WillOnce(Return(true))
		.WillOnce(Return(false));
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(7);

	MessageField messageField(person, field);

	messageField.SetField(resultSetMock);
	ASSERT_TRUE(person.marry());

	messageField.SetField(resultSetMock);
	ASSERT_FALSE(person.marry());
}

TEST_F(MessageFieldTest, SetField_FieldIsDouble)
{
	auto resultSetMock = std::make_shared<ResultSetMock>();
	EXPECT_CALL(*resultSetMock, getDouble(6))
		.WillOnce(Return(1.00))
		.WillOnce(Return(12345.6789));
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(5);

	MessageField messageField(person, field);

	messageField.SetField(resultSetMock);
	ASSERT_FLOAT_EQ(1.00, person.height());

	messageField.SetField(resultSetMock);
	ASSERT_FLOAT_EQ(12345.6789, person.height());
}

TEST_F(MessageFieldTest, SetField_FieldIsInt32)
{
	auto resultSetMock = std::make_shared<ResultSetMock>();
	EXPECT_CALL(*resultSetMock, getInt(2))
		.WillOnce(Return(1))
		.WillOnce(Return(0xFFFFFFFF));
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(1);

	MessageField messageField(person, field);

	messageField.SetField(resultSetMock);
	ASSERT_EQ(1, person.age());

	messageField.SetField(resultSetMock);
	ASSERT_EQ(-1, person.age());
}

TEST_F(MessageFieldTest, SetField_FieldIsInt64)
{
	auto resultSetMock = std::make_shared<ResultSetMock>();
	EXPECT_CALL(*resultSetMock, getInt64(1))
		.WillOnce(Return(1))
		.WillOnce(Return(0xFFFFFFFFFFFFFFFF));
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(0);

	MessageField messageField(person, field);

	messageField.SetField(resultSetMock);
	ASSERT_EQ(1, person.guid());

	messageField.SetField(resultSetMock);
	ASSERT_EQ(-1, person.guid());
}

TEST_F(MessageFieldTest, SetField_FieldIsUInt32)
{
	auto resultSetMock = std::make_shared<ResultSetMock>();
	EXPECT_CALL(*resultSetMock, getUInt(3))
		.WillOnce(Return(1))
		.WillOnce(Return(0xFFFFFFFF));
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(2);

	MessageField messageField(person, field);

	messageField.SetField(resultSetMock);
	ASSERT_EQ(1, person.number());

	messageField.SetField(resultSetMock);
	ASSERT_EQ(4294967295, person.number());
}

TEST_F(MessageFieldTest, SetField_FieldIsUInt64)
{
	auto resultSetMock = std::make_shared<ResultSetMock>();
	EXPECT_CALL(*resultSetMock, getUInt64(4))
		.WillOnce(Return(1))
		.WillOnce(Return(0xFFFFFFFFFFFFFFFF));
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(3);

	MessageField messageField(person, field);

	messageField.SetField(resultSetMock);
	ASSERT_EQ(1, person.time());

	messageField.SetField(resultSetMock);
	ASSERT_EQ(18446744073709551615ULL, person.time());
}

TEST_F(MessageFieldTest, SetField_FieldIsString)
{
	auto resultSetMock = std::make_shared<ResultSetMock>();
	EXPECT_CALL(*resultSetMock, getString(5))
		.WillOnce(Return("1"))
		.WillOnce(Return("tanghai"));
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(4);

	MessageField messageField(person, field);

	messageField.SetField(resultSetMock);
	ASSERT_EQ("1", person.name());

	messageField.SetField(resultSetMock);
	ASSERT_EQ("tanghai", person.name());
}

TEST_F(MessageFieldTest, SetField_FieldIsBytes)
{
	auto resultSetMock = std::make_shared<ResultSetMock>();
	EXPECT_CALL(*resultSetMock, getString(7))
		.WillOnce(Return("1"))
		.WillOnce(Return("tanghai is a good student"));
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(6);

	MessageField messageField(person, field);

	messageField.SetField(resultSetMock);
	ASSERT_EQ("1", person.comment());

	messageField.SetField(resultSetMock);
	ASSERT_EQ("tanghai is a good student", person.comment());
}

TEST_F(MessageFieldTest, SetField_FieldIsMessage)
{
	std::istringstream is;
	is.str("id: 123 name: \"pen\"");
	auto resultSetMock = std::make_shared<ResultSetMock>();
	EXPECT_CALL(*resultSetMock, getBlob(9))
		.WillOnce(Return(&is));
	Person person;
	const google::protobuf::Descriptor* descriptor = person.GetDescriptor();
	const google::protobuf::FieldDescriptor* field = descriptor->field(8);

	MessageField messageField(person, field);

	messageField.SetField(resultSetMock);
	ASSERT_EQ(123, person.item().id());
	ASSERT_EQ("pen", person.item().name());
}

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}

