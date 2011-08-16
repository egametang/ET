#include "Event/BuffType.h"
#include "Event/ContexIf.h"
#include "Event/SpellBuff.h"
#include "Event/EventConf.pb.h"

namespace Egametang {

BuffType::BuffType(int buff_type): buff_type(buff_type)
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

NodeIf* BuffTypeFactory::GetInstance(const EventNode& conf)
{
	return new BuffType(conf.args(0));
}

} // namespace Egametang

