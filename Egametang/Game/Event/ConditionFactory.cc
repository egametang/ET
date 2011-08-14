#include "Event/ConditionFactory.h"

namespace Egametang {

ConditionFactory::ConditionFactory(): factories(100)
{
}

ConditionFactory::~ConditionFactory()
{
	for (std::size_t i = 0; i < factories.size(); ++i)
	{
		delete factories[i];
	}
}

void ConditionFactory::Register(int type, NodeFactoryIf* factory)
{
	factories[type] = factory;
}

NodeIf* ConditionFactory::GetInstance(const LogicNode& conf)
{
	int32 type = conf.type();
	return factories[type]->GetInstance(conf);
}

} // namespace Egametang
