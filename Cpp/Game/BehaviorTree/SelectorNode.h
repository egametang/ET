#ifndef BEHAVIORTREE_SELECTORNODE_H
#define BEHAVIORTREE_SELECTORNODE_H

#include <list>
#include "BehaviorTree/BehaviorNodeIf.h"

namespace Egametang {

class SelectorNode: public BehaviorNodeIf
{
private:
	std::list<BehaviorNodeIf*> nodes;

public:
	SelectorNode(int32 type);

	virtual ~SelectorNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(BehaviorNodeIf *node);

	virtual std::string ToString();
};

class SelectorNodeFactory: public BehaviorNodeFactoryIf
{
public:
	virtual BehaviorNodeIf* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang


#endif // BEHAVIORTREE_SELECTORNODE_H
