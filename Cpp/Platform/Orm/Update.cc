// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <google/protobuf/descriptor.h>
#include "Orm/Update.h"
#include "Orm/Column.h"
#include "Orm/Exception.h"
#include "Orm/MessageField.h"

namespace Egametang {

Update::Update(google::protobuf::Message& message): message(message)
{
}

Update& Update::Where(Expr expr)
{
	expr.CheckAllColumns(message);
	where = expr;
	return *this;
}

std::string Update::ToString() const
{
	int fieldIsSetCount = 0;
	std::string sql = "update " + message.GetDescriptor()->full_name();
	sql += " set ";
	const google::protobuf::Descriptor* descriptor = message.GetDescriptor();
	const google::protobuf::Reflection* reflection = message.GetReflection();
	for (int i = 0; i < descriptor->field_count(); ++i)
	{
		const google::protobuf::FieldDescriptor* field = descriptor->field(i);
		if (!reflection->HasField(message, field))
		{
			continue;
		}
		++fieldIsSetCount;
		MessageField messageField(message, field);
		sql += field->name() + " = " + messageField.GetField() + ", ";
	}

	if (fieldIsSetCount == 0)
	{
		throw MessageNoFeildIsSetException() << MessageNoFeildIsSetErrStr("no field is set");
	}
	// 去除最后的逗号和空格
	sql.resize(sql.size() - 2);
	if (!where.Empty())
	{
		sql += " where " + where.ToString();
	}
	return sql;
}

} // namespace Egametang
