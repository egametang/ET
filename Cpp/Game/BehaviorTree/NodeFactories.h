#ifndef BEHAVIORTREE_NODEFACTORIES_H
#define BEHAVIORTREE_NODEFACTORIES_H

#include <vector>
#include "BehaviorTree/NodeIf.h"

namespace Egametang {

class NodeFactories
{
private:
	std::vector<NodeFactoryIf*> factories;

public:
	NodeFactories();

	virtual ~NodeFactories();

	virtual NodeIf* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang

#endif // BEHAVIORTREE_NODEFACTORIES_H
