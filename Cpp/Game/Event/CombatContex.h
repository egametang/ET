#ifndef EVENT_COMBATCONTEX_H
#define EVENT_COMBATCONTEX_H

#include "Event/ContexIf.h"

namespace Egametang {

class CombatContex: public ContexIf
{
private:
	Spell* spell;
	Buff* buff;

public:
	CombatContex(Spell* spell, Buff* buff);

	virtual Spell* GetSpell();

	virtual Buff* GetBuff();
};

} // namespace Egametang

#endif // EVENT_COMBATCONTEX_H

