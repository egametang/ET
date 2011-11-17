#include <google/protobuf/descriptor.h>
#include <glog/logging.h>
#include "Orm/OrmConn.h"

namespace Egametang {

OrmConn::OrmConn()
{
}

OrmConn::~OrmConn()
{
}

// 根据message拼出类似如下的sql语句
// insert into person(id, name, sex, age, marry) values(1, 'xiaoming', 'M', 18, 1);
std::string OrmConn::GetInsertSQL(const google::protobuf::Message& message)
{
	std::string sql = "";
	std::string tableName = message.GetTypeName();
	const google::protobuf::Reflection* reflection = message.GetReflection();

	sql += "insert into " + tableName + "(";
	std::string columnNames = "";
	std::string valueString = "";
	for (int i = 0; i < message.GetDescriptor()->field_count(); ++i)
	{
		const google::protobuf::FieldDescriptor* field = message.GetDescriptor()->field(i);
		if (!reflection->HasField(field))
		{
			continue;
		}

		std::string name = field->name();
		columnNames += name + ", ";

		MessageField msgField(message, field);
		valueString += msgField.ValueToString() + ", ";
	}
	// 去除最后的逗号
	columnNames.resize(columnNames.size() - 2);
	valueString.resize(valueString.size() - 2);
	sql += columnNames + ") values(";
	sql += valueString + ");";
	return sql;
}

void OrmConn::Select(std::vector<boost::shared_ptr<google::protobuf::Message> >& messages, Condition condition)
{
}

void OrmConn::Insert(const google::protobuf::Message& message)
{
	std::string sql = GetInsertSQL(message);
}

void OrmConn::Update(const google::protobuf::Message& message, Condition condition)
{
}

} // namespace Egametang
