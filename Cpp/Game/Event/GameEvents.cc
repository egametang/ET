#include <boost/foreach.hpp>
#include <glog/logging.h>
#include "Base/Marcos.h"
#include "Base/Typedef.h"
#include "Event/GameEvents.h"
#include "Event/EventConf.pb.h"

namespace Egametang {

GameEvents::GameEvents(NodeFactories& factories):
		factories(factories), events(100)
{
}

GameEvents::~GameEvents()
{
	foreach(std::list<Event*> list, events)
	{
		foreach(Event* event, list)
		{
			delete event;
		}
	}
}

void GameEvents::AddEvent(EventConf& conf)
{
	int32 type = conf.type();
	Event* event = new Event(factories, conf);
	events[type].push_back(event);
}

void GameEvents::Excute(int type, ContexIf* contex)
{
	std::list<Event*>& es = events[type];

	for (std::list<Event*>::iterator iter = es.begin(); iter != es.end(); ++iter)
	{
		(*iter)->Run(contex);
	}
}

} // namespace Egametang
