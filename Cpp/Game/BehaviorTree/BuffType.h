#ifndef BEHAVIORTREE_BUFFTYPE_H
#define BEHAVIORTREE_BUFFTYPE_H

#include "BehaviorTree/BehaviorNode.h"

namespace Egametang {

class ContexIf;

class BuffType: public BehaviorNode
{
private:
	int32 buffType;

public:
	BuffType(int32 type, int buff_type);

	virtual ~BuffType();

	virtual bool Run(ContexIf* contex);

	virtual std::string ToString();
};

class BuffTypeFactory: public BehaviorNodeFactoryIf
{
public:
	virtual ~BuffTypeFactory();

	virtual BehaviorNode* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang


#endif // BEHAVIORTREE_BUFFTYPE_H
