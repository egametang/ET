#ifndef BEHAVIORTREE_SPELLBUFF_H
#define BEHAVIORTREE_SPELLBUFF_H

namespace Egametang {

class Unit
{
public:
	int health;
};

class Spell
{
public:
	Unit* caster;
	Unit* victim;
};

class Buff
{
public:
	int buffType;

public:
	Buff(): buffType(0)
	{
	}
};

} // namespace Egametang


#endif // BEHAVIORTREE_SPELLBUFF_H
