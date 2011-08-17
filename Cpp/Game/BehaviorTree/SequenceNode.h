#ifndef BEHAVIORTREE_SEQUENCENODE_H
#define BEHAVIORTREE_SEQUENCENODE_H

#include <list>
#include "BehaviorTree/BehaviorNodeIf.h"

namespace Egametang {

class SequenceNode: public BehaviorNodeIf
{
private:
	std::list<BehaviorNodeIf*> nodes;

public:
	SequenceNode(int32 type);

	virtual ~SequenceNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(BehaviorNodeIf *node);

	virtual std::string ToString();
};

class SequenceNodeFactory: public BehaviorNodeFactoryIf
{
public:
	virtual BehaviorNodeIf* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang


#endif // BEHAVIORTREE_SEQUENCENODE_H
