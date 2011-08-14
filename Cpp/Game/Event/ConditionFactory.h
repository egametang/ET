#ifndef EVENT_CONDITIONFACTORY_H
#define EVENT_CONDITIONFACTORY_H

#include <vector>
#include "Event/NodeIf.h"

namespace Egametang {

class ConditionFactory
{
private:
	std::vector<NodeFactoryIf*> factories;

public:
	ConditionFactory();

	~ConditionFactory();

	void Register(int type, NodeFactoryIf* factory);

	NodeIf* GetInstance(const LogicNode& conf);
};

} // namespace Egametang

#endif // EVENT_CONDITIONFACTORY_H
