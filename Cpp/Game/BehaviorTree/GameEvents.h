#ifndef BEHAVIORTREE_GAMEEVENTS_H
#define BEHAVIORTREE_GAMEEVENTS_H

#include <list>
#include <vector>
#include "BehaviorTree/BehaviorTree.h"

namespace Egametang {

class NodeFactories;

class GameEvents
{
private:
	NodeFactories& factories;
	std::vector<std::list<BehaviorTree*> > events;

public:
	GameEvents(NodeFactories& factories);

	~GameEvents();

	void AddEvent(const BehaviorTreeConf& conf);

	void Excute(int type, ContexIf* contex);
};

} // namespace Egametang

#endif // BEHAVIORTREE_GAMEEVENTS_H
