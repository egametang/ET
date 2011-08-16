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

	void BuildTree(
			NodeFactories& factories, const EventNode& conf,
			NodeIf*& condition);

public:
	Event(NodeFactories& factories, EventConf& conf);

	~Event();

	void Run(ContexIf* contex);

	std::string ToString();
};

} // namespace Egametang


#endif // EVENT_EVENT_H
