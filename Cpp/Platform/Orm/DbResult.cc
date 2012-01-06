// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#include <google/protobuf/descriptor.h>
#include "Orm/DbResult.h"
#include "Orm/MessageField.h"

namespace Egametang {

DbResult::DbResult(ResultSetPtr resultSet): resultSet(resultSet)
{
}

void DbResult::FillMessage(ProtobufMessagePtr message)
{
	const google::protobuf::Descriptor* descriptor = message->GetDescriptor();
	for (int i = 0; i < descriptor->field_count(); ++i)
	{
		if (resultSet->isNull(i))
		{
			continue;
		}
		const google::protobuf::FieldDescriptor* field = descriptor->field(i);
		MessageField messageField(*message, field);
		messageField.SetField(resultSet);
	}
}

void DbResult::All(std::vector<ProtobufMessagePtr>& messages)
{
	for (int i = 0; i < messages.size(); ++i)
	{
		if (!resultSet->next())
		{
			return;
		}
		ProtobufMessagePtr message = messages[i];
		FillMessage(message);
	}
}

void DbResult::One(ProtobufMessagePtr message)
{
	if (!resultSet->next())
	{
		return;
	}
	FillMessage(message);
}

std::size_t DbResult::Count()
{
	return resultSet->rowsCount();
}

} // namespace Egametang
