#ifndef EVENT_CHANGEHEALTH_H
#define EVENT_CHANGEHEALTH_H

#include "Base/Typedef.h"
#include "Event/NodeIf.h"

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
};

class ChangeHealthFactory: public NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const EventNode& conf);
};

} // namespace Egametang

#endif // EVENT_CHANGEHEALTH_H
