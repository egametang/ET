#ifndef BEHAVIORTREE_NOTNODE_H
#define BEHAVIORTREE_NOTNODE_H

#include "BehaviorTree/BehaviorNodeIf.h"

namespace Egametang {

class NotNode: public BehaviorNodeIf
{
private:
	BehaviorNodeIf* node;

public:
	NotNode(int32 type);

	virtual ~NotNode();

	virtual bool Run(ContexIf* contex);

	virtual void AddChildNode(BehaviorNodeIf *node);

	virtual std::string ToString();
};

class NotNodeFactory: public BehaviorNodeFactoryIf
{
public:
	virtual BehaviorNodeIf* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang


#endif // BEHAVIORTREE_NOTNODE_H
