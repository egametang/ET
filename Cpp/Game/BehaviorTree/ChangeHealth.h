#ifndef BEHAVIORTREE_CHANGEHEALTH_H
#define BEHAVIORTREE_CHANGEHEALTH_H

#include "Base/Typedef.h"
#include "BehaviorTree/NodeIf.h"

namespace Egametang {

class ChangeHealth: public NodeIf
{
private:
	int32 unit;
	int32 value;

public:
	ChangeHealth(int32 unit, int32 value);

	virtual ~ChangeHealth();

	virtual bool Run(ContexIf* contex);

	virtual std::string ToString();
};

class ChangeHealthFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const BehaviorNodeConf& conf);
};

} // namespace Egametang

#endif // BEHAVIORTREE_CHANGEHEALTH_H
