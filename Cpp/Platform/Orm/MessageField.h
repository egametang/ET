#ifndef ORM_MESSAGEFIELD_H
#define ORM_MESSAGEFIELD_H

#include <string>
#include <google/protobuf/message.h>
#include "Orm/OrmTypedef.h"

namespace Egametang {

class MessageField
{
private:
	const google::protobuf::Message& message;
	const google::protobuf::FieldDescriptor* field;

public:
	MessageField(const google::protobuf::Message& message,
			const google::protobuf::FieldDescriptor* field);
	~MessageField();

	std::string GetField();
	std::string GetRepeatedField();
	std::string GetOptionalField();

	void SetField(ResultSetPtr resultSet, int index);
	void SetRepeatedField(ResultSetPtr resultSet, int index);
	void SetOptionalField(ResultSetPtr resultSet, int index);
};

} // namespace Egametang
#endif // ORM_MESSAGEFIELD_H
