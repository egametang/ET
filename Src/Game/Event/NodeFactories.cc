#include "Event/NodeFactories.h"

namespace Egametang {

NodeIf* AndNodeFactory::GetInstance(const LogicNode& conf)
{
	return new AndNode();
}

NodeIf* AndNodeFactory::GetInstance(const LogicNode& conf)
{
	return new OrNode();
}

NodeIf* AndNodeFactory::GetInstance(const LogicNode& conf)
{
	return new NotNode();
}

NodeFactories::NodeFactories(ConditionFactory* condition_factory):
		node_factories(4)
{
	node_factories[0] = new AndNodeFactory();
	node_factories[1] = new OrNodeFactory();
	node_factories[2] = new NotNodeFactory();
	node_factories[3] = condition_factory;
}

NodeFactories::~NodeFactories()
{
}

NodeIf* NodeFactories::GetInstance(const LogicNode& conf)
{
	int32 type = conf.type();
	return node_factories[type]->GetInstance(conf);
}

}


