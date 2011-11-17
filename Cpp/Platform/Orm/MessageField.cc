#include <glog/logging.h>
#include "Orm/MessageField.h"

namespace Egametang {

MessageField::MessageField(
		const google::protobuf::Message& message,
		const google::protobuf::FieldDescriptor* field):
	message(message), field(field)
{
}

MessageField::~MessageField()
{
}

std::string MessageField::ValueToString()
{
	const google::protobuf::Reflection* reflection = message.GetReflection();
	google::protobuf::FieldDescriptor::Type type = field->type();
	std::string valueStr = "";
	switch (type)
	{
		case google::protobuf::FieldDescriptor::TYPE_BOOL:
		{
			bool value = reflection->GetBool(message, field);
			valueStr = value? "1" : "0";
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_DOUBLE:
		{
			double value = reflection->GetDouble(message, field);
			valueStr = boost::lexical_cast<std::string>(value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_INT32:
		{
			int32 value = reflection->GetInt32(message, field);
			valueStr = boost::lexical_cast<std::string>(value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_INT64:
		{
			int64 value = reflection->GetInt64(message, field);
			valueStr = boost::lexical_cast<std::string>(value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_UINT32:
		{
			uint32 value = reflection->GetUInt32(message, field);
			valueStr = boost::lexical_cast<std::string>(value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_UINT64:
		{
			uint64 value = reflection->GetUInt64(message, field);
			valueStr = boost::lexical_cast<std::string>(value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_STRING:
		{
			valueStr = "'" + reflection->GetString(message, field) + "'";
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_BYTES:
		{
			valueStr = "'" + reflection->GetString(message, field) + "'";
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_MESSAGE:
		{
			google::protobuf::Message& message = reflection->GetMessage(message, field);
			valueStr = "'" + message.SerializeAsString() + "'";
			break;
		}
		default:
		{
			LOG(FATAL) << "no such type";
			break;
		}
	}
	return valueStr;
}

} // namespace Egametang
