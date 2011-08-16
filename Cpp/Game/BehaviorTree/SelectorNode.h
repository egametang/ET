#ifndef BEHAVIORTREE_SELECTORNODE_H
#define BEHAVIORTREE_SELECTORNODE_H

#include <list>
#include "BehaviorTree/NodeIf.h"

namespace Egametang {

class SelectorNode: public NodeIf
{
private:
	std::list<NodeIf*> nodes;

public:
	virtual ~SelectorNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(NodeIf *node);

	virtual std::string ToString();
};

class SelectorNodeFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang


#endif // BEHAVIORTREE_SELECTORNODE_H
