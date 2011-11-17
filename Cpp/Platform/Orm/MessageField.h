#ifndef ORM_MESSAGEFIELD_H
#define ORM_MESSAGEFIELD_H

#include <string>

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
	std::string MessageField::ValueToString();
};

} // namespace Egametang
#endif // ORM_MESSAGEFIELD_H
