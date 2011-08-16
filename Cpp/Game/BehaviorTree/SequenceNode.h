#ifndef BEHAVIORTREE_SEQUENCENODE_H
#define BEHAVIORTREE_SEQUENCENODE_H

#include <list>
#include "BehaviorTree/NodeIf.h"

namespace Egametang {

class SequenceNode: public NodeIf
{
private:
	std::list<NodeIf*> nodes;

public:
	virtual ~SequenceNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(NodeIf *node);

	virtual std::string ToString();
};

class SequenceNodeFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang


#endif // BEHAVIORTREE_SEQUENCENODE_H
