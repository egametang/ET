#include "BehaviorTree/BuffType.h"
#include "BehaviorTree/ContexIf.h"
#include "BehaviorTree/SpellBuff.h"
#include "BehaviorTree/BehaviorTreeConf.pb.h"

namespace Egametang {

BuffType::BuffType(int32 type, int32 buff_type):
		BehaviorNodeIf(type), buff_type(buff_type)
{
}

BuffType::~BuffType()
{
}

bool BuffType::Run(ContexIf* contex)
{
	Buff* buff = contex->GetBuff();
	return buff->buff_type == buff_type;
}

std::string BuffType::ToString()
{
	std::string s;
	s += "BuffType: \n";
	return s;
}

BuffTypeFactory::~BuffTypeFactory()
{
}

BehaviorNodeIf* BuffTypeFactory::GetInstance(const BehaviorNodeConf& conf)
{
	return new BuffType(conf.type(), conf.args(0));
}

} // namespace Egametang

