#ifndef BEHAVIORTREE_SEQUENCENODE_H
#define BEHAVIORTREE_SEQUENCENODE_H

#include <list>
#include "BehaviorTree/BehaviorNode.h"

namespace Egametang {

class SequenceNode: public BehaviorNode
{
private:
	std::list<BehaviorNode*> nodes;

public:
	SequenceNode(int32 type);

	virtual ~SequenceNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(BehaviorNode *node);

	virtual std::string ToString();
};

class SequenceNodeFactory: public BehaviorNodeFactoryIf
{
public:
	virtual BehaviorNode* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang


#endif // BEHAVIORTREE_SEQUENCENODE_H
