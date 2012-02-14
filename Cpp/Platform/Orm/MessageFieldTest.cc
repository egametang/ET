// Copyright 2011 Netease Inc. All Rights Reserved.
// Author: tanghai@corp.netease.com (tanghai)

#include <gtest/gtest.h>
#include <glog/logging.h>
#include <gflags/gflags.h>
#include <google/protobuf/descriptor.h>
#include "Orm/MessageField.h"
#include "Orm/Person.pb.h"

namespace Egametang {

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

} // namespace Egametang


int main(int argc, char* argv[])
{
	testing::InitGoogleTest(&argc, argv);
	google::InitGoogleLogging(argv[0]);
	google::ParseCommandLineFlags(&argc, &argv, true);
	return RUN_ALL_TESTS();
}

