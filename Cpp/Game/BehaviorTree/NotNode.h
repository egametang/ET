#ifndef BEHAVIORTREE_NOTNODE_H
#define BEHAVIORTREE_NOTNODE_H

#include "BehaviorTree/BehaviorNode.h"

namespace Egametang {

class NotNode: public BehaviorNode
{
private:
	BehaviorNode* node;

public:
	NotNode(int32 type);

	virtual ~NotNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(BehaviorNode *node);

	virtual std::string ToString();
};

class NotNodeFactory: public BehaviorNodeFactoryIf
{
public:
	virtual BehaviorNode* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang


#endif // BEHAVIORTREE_NOTNODE_H
