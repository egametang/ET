#ifndef EVENT_EVENT_H
#define EVENT_EVENT_H

#include "Event/ActionIf.h"
#include "Event/NodeIf.h"

namespace Egametang {

class Event
{
private:
	int type;
	ActionIf* action;
	NodeIf* condition;

	void BuildCondition(
			NodeFactories& factories, const LogicNode& conf,
			NodeIf*& condition);

	void BuildAction(
			NodeFactories& factories, const ConditionConf& conf);

public:
	Event(NodeFactories& factories, EventConf& conf);

	~Event();

	int Type() const;

	void Excute(ContexIf* contex);
};

} // namespace Egametang


#endif // EVENT_EVENT_H
