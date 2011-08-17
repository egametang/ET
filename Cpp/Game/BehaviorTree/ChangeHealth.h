#ifndef BEHAVIORTREE_CHANGEHEALTH_H
#define BEHAVIORTREE_CHANGEHEALTH_H

#include "Base/Typedef.h"
#include "BehaviorTree/BehaviorNodeIf.h"

namespace Egametang {

class ChangeHealth: public BehaviorNodeIf
{
private:
	int32 unit;
	int32 value;

public:
	ChangeHealth(int32 type, int32 unit, int32 value);

	virtual ~ChangeHealth();

	virtual bool Run(ContexIf* contex);

	virtual std::string ToString();
};

class ChangeHealthFactory: public BehaviorNodeFactoryIf
{
public:
	virtual BehaviorNodeIf* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang

#endif // BEHAVIORTREE_CHANGEHEALTH_H
