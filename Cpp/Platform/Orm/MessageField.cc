#include <string>
#include <iostream>
#include <boost/algorithm/string.hpp>
#include <boost/lexical_cast.hpp>
#include <glog/logging.h>
#include <google/protobuf/descriptor.h>
#include <google/protobuf/text_format.h>
#include "Base/Typedef.h"
#include "Orm/MessageField.h"

namespace Egametang {

MessageField::MessageField(google::protobuf::Message& message,
		const google::protobuf::FieldDescriptor* field):
	message(message), field(field)
{
}

MessageField::~MessageField()
{
}

std::string MessageField::GetField()
{
	std::string valueStr;
	if (field->is_repeated())
	{
		valueStr = GetRepeatedField();
	}
	else
	{
		valueStr = GetOptionalField();
	}
	return valueStr;
}

std::string MessageField::GetRepeatedField()
{
	const google::protobuf::Reflection* reflection = message.GetReflection();
	google::protobuf::FieldDescriptor::Type type = field->type();
	std::string valueStr;
	switch (type)
	{
		case google::protobuf::FieldDescriptor::TYPE_BOOL:
		{
			for (int i = 0; i < reflection->FieldSize(message, field); ++i)
			{
				bool value = reflection->GetBool(message, field);
				valueStr += value? "1\t" : "0\t";
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_DOUBLE:
		{
			for (int i = 0; i < reflection->FieldSize(message, field); ++i)
			{
				double value = reflection->GetDouble(message, field);
				valueStr += boost::lexical_cast<std::string>(value) + "\t";
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_INT32:
		{
			for (int i = 0; i < reflection->FieldSize(message, field); ++i)
			{
				int32 value = reflection->GetInt32(message, field);
				valueStr += boost::lexical_cast<std::string>(value) + "\t";
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_INT64:
		{
			for (int i = 0; i < reflection->FieldSize(message, field); ++i)
			{
				int64 value = reflection->GetInt64(message, field);
				valueStr += boost::lexical_cast<std::string>(value) + "\t";
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_UINT32:
		{
			for (int i = 0; i < reflection->FieldSize(message, field); ++i)
			{
				int64 value = reflection->GetInt64(message, field);
				valueStr += boost::lexical_cast<std::string>(value) + "\t";
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_UINT64:
		{
			for (int i = 0; i < reflection->FieldSize(message, field); ++i)
			{
				int64 value = reflection->GetInt64(message, field);
				valueStr += boost::lexical_cast<std::string>(value) + "\t";
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_STRING:
		{
			valueStr += "'";
			for (int i = 0; i < reflection->FieldSize(message, field); ++i)
			{
				int64 value = reflection->GetInt64(message, field);
				valueStr += reflection->GetString(message, field) + "\t";
			}
			valueStr += "'";
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_BYTES:
		{
			valueStr += "'";
			for (int i = 0; i < reflection->FieldSize(message, field); ++i)
			{
				int64 value = reflection->GetInt64(message, field);
				valueStr += reflection->GetString(message, field) + "\t";
			}
			valueStr += "'";
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_MESSAGE:
		{
			valueStr += "'";
			for (int i = 0; i < reflection->FieldSize(message, field); ++i)
			{
				const google::protobuf::Message& message = reflection->GetMessage(message, field);
				valueStr += message.ShortDebugString() + "\t";
			}
			valueStr += "'";
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

std::string MessageField::GetOptionalField()
{
	const google::protobuf::Reflection* reflection = message.GetReflection();
	google::protobuf::FieldDescriptor::Type type = field->type();
	std::string valueStr;
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
			const google::protobuf::Message& message = reflection->GetMessage(message, field);
			valueStr = message.ShortDebugString();
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

void MessageField::SetRepeatedField(ResultSetPtr resultSet)
{
	const google::protobuf::Reflection* reflection = message.GetReflection();
	google::protobuf::FieldDescriptor::Type type = field->type();

	// 获取blob string(repeated字段统一存成blob type)
	int index = field->index();
	std::istream* is = resultSet->getBlob(index);
	std::ostringstream os;
	os << is->rdbuf();
	std::string fieldStr = os.str();

	std::vector<std::string> strVector;
	boost::split(strVector, fieldStr, boost::is_any_of("\t"));

	switch (type)
	{
		case google::protobuf::FieldDescriptor::TYPE_BOOL:
		{
			for (std::size_t i = 0; i < strVector.size(); ++i)
			{
				int intValue = boost::lexical_cast<int>(strVector[i]);
				bool value = intValue? true : false;
				reflection->SetRepeatedBool(&message, field, i, value);
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_DOUBLE:
		{
			for (std::size_t i = 0; i < strVector.size(); ++i)
			{
				double value = boost::lexical_cast<double>(strVector[i]);
				reflection->SetRepeatedDouble(&message, field, i, value);
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_INT32:
		{
			for (std::size_t i = 0; i < strVector.size(); ++i)
			{
				int32 value = boost::lexical_cast<int32>(strVector[i]);
				reflection->SetRepeatedInt32(&message, field, i, value);
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_INT64:
		{
			for (std::size_t i = 0; i < strVector.size(); ++i)
			{
				int64 value = boost::lexical_cast<int64>(strVector[i]);
				reflection->SetRepeatedInt64(&message, field, i, value);
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_UINT32:
		{
			for (std::size_t i = 0; i < strVector.size(); ++i)
			{
				uint32 value = boost::lexical_cast<uint32>(strVector[i]);
				reflection->SetRepeatedUInt32(&message, field, i, value);
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_UINT64:
		{
			for (std::size_t i = 0; i < strVector.size(); ++i)
			{
				uint64 value = boost::lexical_cast<uint64>(strVector[i]);
				reflection->SetRepeatedUInt64(&message, field, i, value);
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_STRING:
		{
			for (std::size_t i = 0; i < strVector.size(); ++i)
			{
				reflection->SetRepeatedString(&message, field, i, strVector[i]);
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_BYTES:
		{
			for (std::size_t i = 0; i < strVector.size(); ++i)
			{
				reflection->SetRepeatedString(&message, field, i, strVector[i]);
			}
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_MESSAGE:
		{
			for (std::size_t i = 0; i < strVector.size(); ++i)
			{
				google::protobuf::Message* msg =
						reflection->MutableRepeatedMessage(&message, field, i);
				google::protobuf::TextFormat::ParseFromString(strVector[i], msg);
			}
			break;
		}
		default:
		{
			LOG(FATAL) << "no such type";
			break;
		}
	}
}

void MessageField::SetOptionalField(ResultSetPtr resultSet)
{
	const google::protobuf::Reflection* reflection = message.GetReflection();
	google::protobuf::FieldDescriptor::Type type = field->type();
	int index = field->index();
	switch (type)
	{
		case google::protobuf::FieldDescriptor::TYPE_BOOL:
		{
			bool value = resultSet->getBoolean(index);
			reflection->SetBool(&message, field, value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_DOUBLE:
		{
			double value = resultSet->getDouble(index);
			reflection->SetDouble(&message, field, value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_INT32:
		{
			int32 value = resultSet->getInt(index);
			reflection->SetInt32(&message, field, value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_INT64:
		{
			int64 value = resultSet->getInt64(index);
			reflection->SetInt64(&message, field, value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_UINT32:
		{
			uint32 value = resultSet->getUInt(index);
			reflection->SetUInt32(&message, field, value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_UINT64:
		{
			uint64 value = resultSet->getUInt64(index);
			reflection->SetUInt64(&message, field, value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_STRING:
		{
			std::string value = resultSet->getString(index);
			reflection->SetString(&message, field, value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_BYTES:
		{
			std::string value = resultSet->getString(index);
			reflection->SetString(&message, field, value);
			break;
		}
		case google::protobuf::FieldDescriptor::TYPE_MESSAGE:
		{
			std::istream* is = resultSet->getBlob(index);
			std::ostringstream os;
			os << is->rdbuf();
			std::string value = os.str();

			google::protobuf::Message* msg =
					reflection->MutableMessage(&message, field);
			google::protobuf::TextFormat::ParseFromString(value, msg);
			break;
		}
		default:
		{
			LOG(FATAL) << "no such type";
			break;
		}
	}
}

void MessageField::SetField(ResultSetPtr resultSet)
{
	if (field->is_repeated())
	{
		SetRepeatedField(resultSet);
	}
	else
	{
		SetOptionalField(resultSet);
	}
}

} // namespace Egametang
