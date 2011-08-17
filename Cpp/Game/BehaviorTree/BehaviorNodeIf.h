#ifndef BEHAVIORTREE_BEHAVIORNODEIF_H
#define BEHAVIORTREE_BEHAVIORNODEIF_H

#include <string>
#include "Base/Typedef.h"

namespace Egametang {

class ContexIf;
class BehaviorNodeConf;

class BehaviorNodeIf
{
private:
	int32 type;

public:
	BehaviorNodeIf(int32 type): type(type)
	{
	}

	virtual ~BehaviorNodeIf()
	{
	}

	int32 Type() const
	{
		return type;
	}

	virtual void AddChildNode(BehaviorNodeIf *node)
	{
	}

	virtual bool Run(ContexIf* contex) = 0;

	virtual std::string ToString() = 0;
};

class BehaviorNodeFactoryIf
{
public:
	virtual BehaviorNodeIf* GetInstance(const BehaviorNodeConf& conf) = 0;
};

} // namespace Egametang

#endif // BEHAVIORTREE_BEHAVIORNODEIF_H
