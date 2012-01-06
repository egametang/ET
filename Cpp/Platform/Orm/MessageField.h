#ifndef ORM_MESSAGEFIELD_H
#define ORM_MESSAGEFIELD_H

#include <string>
#include <google/protobuf/message.h>
#include "Orm/OrmTypedef.h"

namespace Egametang {

class MessageField
{
private:
	google::protobuf::Message& message;
	const google::protobuf::FieldDescriptor* field;

	std::string GetRepeatedField();
	std::string GetOptionalField();

	void SetRepeatedField(ResultSetPtr resultSet);
	void SetOptionalField(ResultSetPtr resultSet);

public:
	MessageField(google::protobuf::Message& message,
			const google::protobuf::FieldDescriptor* field);
	~MessageField();

	std::string GetField();

	void SetField(ResultSetPtr resultSet);
};

} // namespace Egametang
#endif // ORM_MESSAGEFIELD_H
