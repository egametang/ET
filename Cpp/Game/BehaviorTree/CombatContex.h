#ifndef BEHAVIORTREE_COMBATCONTEX_H
#define BEHAVIORTREE_COMBATCONTEX_H

#include "BehaviorTree/ContexIf.h"

namespace Egametang {

class CombatContex: public ContexIf
{
private:
	Spell* spell;
	Buff* buff;

public:
	CombatContex(Spell* spell, Buff* buff);
	~CombatContex();

	virtual Spell* GetSpell();

	virtual Buff* GetBuff();
};

} // namespace Egametang

#endif // BEHAVIORTREE_COMBATCONTEX_H

