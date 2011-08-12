#include "Event/GameEvents.h"

namespace Egametang {

GameEvents::GameEvents(): events(100)
{
}

void GameEvents::AddEvent(Event& event)
{
	events[event.Type()].push_back(event);
}

void GameEvents::Excute(int type, ContexIf* contex)
{
	const std::list<Event>& es = events[type];

	for (std::list<Event>::iterator iter = es.begin(); iter != es.end();)
	{
		// 暂未考虑event删除情况,每个event都会对应到一个buff
		// 所以可以在这里遍历的时候查看相应buff是否删除,如果
		// 删除就删除相应event
		if (false)
		{
			std::list<Event>::iterator current = iter;
			++iter;
			es.erase(current);
			continue;
		}
		iter->Excute(contex);
		++iter;
	}
}

} // namespace Egametang
