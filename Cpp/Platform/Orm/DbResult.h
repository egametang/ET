// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_DBRESULT_H
#define ORM_DBRESULT_H

#include <cppconn/resultset.h>

namespace Egametang {

class DbResult
{
private:
	ResultSetPtr resultSet;

public:
	DbResult(ResultSetPtr resultSet): resultSet(resultSet)
	{
	}
	void All(std::vector<ProtobufMessagePtr>& messages)
	{
		while (resultSet->next())
		{

		}
	}

	void One(ProtobufMessagePtr message)
	{
		if (!resultSet->first())
		{
			return;
		}
		const google::protobuf::Descriptor* descriptor = message->GetDescriptor();
		for (int i = 0; i < descriptor->field_count(); ++i)
		{
			const google::protobuf::FieldDescriptor* field = descriptor->field(i);
			MessageField messageField(*message, field);
			messageField.SetField(resultSet, i);
		}
	}

	virtual ~DbResult();
};

} // namespace Egametang
#endif // ORM_DBRESULT_H
