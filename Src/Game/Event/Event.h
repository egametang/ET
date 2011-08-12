#ifndef EVENT_EVENT_H
#define EVENT_EVENT_H

#include "Event/ActionIf.h"
#include "Event/LogicNodeIf.h"

namespace Egametang {

class EventConf;

class Event
{
private:
	int type;
	LogicNodeIf* condition;
	ActionIf* action;

	void BuildCondition(EventConf& conf);
	void BuildAction(EventConf& conf);

public:
	Event(EventConf& conf);
	~Event();
	int Type() const;
	void Excute(ContexIf* contex);
};

} // namespace Egametang


#endif // EVENT_EVENT_H
