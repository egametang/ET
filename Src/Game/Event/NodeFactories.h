#ifndef EVENT_NODEFACTORIES_H
#define EVENT_NODEFACTORIES_H

#include <vector>
#include "Event/NodeIf.h"

namespace Egametang {

class AndNodeFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const LogicNode& conf);
};


class OrNodeFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const LogicNode& conf);
};


class NotNodeFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const LogicNode& conf);
};


class NodeFactories
{
private:
	std::vector<NodeFactoryIf*> node_factories;

public:
	NodeFactories(ConditionFactory* condition_factory);

	virtual ~NodeFactories();

	virtual NodeIf* GetInstance(const LogicNode& conf);
};

} // namespace Egametang

#endif // EVENT_NODEFACTORIES_H
