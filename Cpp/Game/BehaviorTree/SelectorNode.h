#ifndef BEHAVIORTREE_SELECTORNODE_H
#define BEHAVIORTREE_SELECTORNODE_H

#include <list>
#include "BehaviorTree/BehaviorNode.h"

namespace Egametang {

class SelectorNode: public BehaviorNode
{
private:
	std::list<BehaviorNode*> nodes;

public:
	SelectorNode(int32 type);

	virtual ~SelectorNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(BehaviorNode *node);

	virtual std::string ToString();
};

class SelectorNodeFactory: public BehaviorNodeFactoryIf
{
public:
	virtual BehaviorNode* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang


#endif // BEHAVIORTREE_SELECTORNODE_H
