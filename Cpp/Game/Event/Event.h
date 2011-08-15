#ifndef EVENT_EVENT_H
#define EVENT_EVENT_H

#include "Event/NodeIf.h"

namespace Egametang {

class EventNode;
class NodeFactories;
class ConditionConf;
class EventConf;

class Event
{
private:
	int type;
	NodeIf* condition;
	NodeIf* action;

	void BuildCondition(
			NodeFactories& factories, const EventNode& conf,
			NodeIf*& condition);

	void BuildAction(
			NodeFactories& factories, const ConditionConf& conf,
			NodeIf*& action);

public:
	Event(NodeFactories& factories, EventConf& conf);

	~Event();

	int Type() const;

	void Run(ContexIf* contex);
};

} // namespace Egametang


#endif // EVENT_EVENT_H
