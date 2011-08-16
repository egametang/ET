#ifndef EVENT_GAMEEVENTS_H
#define EVENT_GAMEEVENTS_H

#include <list>
#include <vector>
#include "Event/Event.h"

namespace Egametang {

class NodeFactories;

class GameEvents
{
private:
	NodeFactories& factories;
	std::vector<std::list<Event*> > events;

public:
	GameEvents(NodeFactories& factories);

	~GameEvents();

	void AddEvent(EventConf& conf);

	void Excute(int type, ContexIf* contex);
};

} // namespace Egametang

#endif // EVENT_GAMEEVENTS_H
