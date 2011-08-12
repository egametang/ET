#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "Event/BuffTypeCondition.h"

namespace Egametang {
BuffType::BuffType(int buff_type): buff_type(buff_type)
{
}

bool BuffType::Check(ContexIf* contex)
{
	return true;
}

Egametang::BuffTypeFactory::~BuffTypeFactory()
{
}

NodeIf* BuffTypeFactory::GetInstance(const LogicNode& conf)
{
	return new BuffType(conf.args(0));
}

} // namespace Egametang

