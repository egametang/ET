#include "Event/CombatContex.h"

namespace Egametang {

CombatContex::CombatContex(Spell* spell, Buff* buff):
	spell(spell), buff(buff)
{
}

CombatContex::~CombatContex()
{
}

Spell* CombatContex::GetSpell()
{
	return spell;
}

Buff* CombatContex::GetBuff()
{
	return buff;
}

} // namespace Egametang
