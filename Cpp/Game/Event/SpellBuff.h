#ifndef EVENT_SPELLBUFF_H
#define EVENT_SPELLBUFF_H

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
	int buff_type;
};

} // namespace Egametang


#endif // EVENT_SPELLBUFF_H
