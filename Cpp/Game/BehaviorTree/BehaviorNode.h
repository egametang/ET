#ifndef BEHAVIORTREE_BEHAVIORNODE_H
#define BEHAVIORTREE_BEHAVIORNODE_H

#include <string>
#include "Base/Typedef.h"

namespace Egametang {

class ContexIf;
class BehaviorNodeConf;

class BehaviorNode
{
private:
	int32 type;

public:
	BehaviorNode(int32 type): type(type)
	{
	}

	virtual ~BehaviorNode()
	{
	}

	int32 Type() const
	{
		return type;
	}

	virtual void AddChildNode(BehaviorNode *node)
	{
	}

	virtual bool Run(ContexIf* contex) = 0;

	virtual std::string ToString() = 0;
};

class BehaviorNodeFactoryIf
{
public:
	virtual BehaviorNode* GetInstance(const BehaviorNodeConf& conf) = 0;
};

} // namespace Egametang

#endif // BEHAVIORTREE_BEHAVIORNODE_H
