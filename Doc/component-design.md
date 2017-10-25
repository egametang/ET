# component design
In terms of code reuse and organization data, object oriented is probably the first response. The three characteristics of object-oriented inheritance, encapsulation, polymorphism, to a certain extent, can solve a lot of code reuse, data reuse problems. However, object oriented is not omnipotent, it also has great defects: 
### 1. the data organization and strong coupling
Once a field is added or deleted in the parent class, all subclasses may be affected, affecting all subclasses related logic. This is not very flexible, in a complex inheritance system, to change the parent class field will become more and more trouble, for example, ABC is a subclass of D, one day found a need to increase the AB of some data, but not C, then this data is certainly not good in a parent class, can only be AB abstract a parent class E inherited from D, E, AB were added to E in some fields, once the inheritance structure changes, may also need to change the interface, for example, before a interface into the parameter type is E, when the AB is not in the field need to be shared, so need to adjust the inheritance relationship, let AB inherited from D, so this interface requires the parameter type logic code into D, which are likely to adjust. Even more frightening is that the game logic changes are very complex, very frequent, may add a field today, and tomorrow deleted, and if each time to adjust the inheritance structure, this is simply a nightmare. Inheritance structure is very weak in the face of frequent data structure adjustment. There is also a serious problem that the inheritance structure can not be run, add delete fields, such as the player Player usually walk, after riding the horse riding. The problem is that the mount's relevant information needs to hang on top of the Player object. This is very inflexible. Why do I have horse data in my memory when I'm not riding?
### 2. interface logic is difficult to reuse, hot swap.
The object oriented method of processing the same behavior is to inherit the same parent class or interface. The problem is that the interface does not implement the code, but it needs its subclass to write its own implementation. Obviously, the same function, each subclass can write a similar code. This makes the code for the interface not reusable. There is a problem, a class implements an interface, so the interface will stick to this kind of person, you want to get rid of her not to ride, or as an example, game player Player can ride, then may inherit a riding interface, the problem is, when I Player from the mounts down, Player game player who still has a riding interface, can't delete this dynamic interface! The possible examples are not quite right, but the arguments should be clear.
### 3. the use of object-oriented could lead to disastrous consequences
There are new people in the game development, old people, good technology, poor technology. People are lazy, when you find the adjustment inheritance relationship trouble, it is possible to add a field in AB, in order to save, directly into the parent class D to go. C leads to a much more useless field. The key can not be found, and finally led to the father class D more and more big, and finally may simply do not use ABC, directly make all objects become D, convenient! Yes, a lot of games do that, and at the end of the development, they don't care about inheritance, because they don't want to manage it.
Object oriented is weak in the face of complex game logic, so many game developers had retreated back, using the process oriented development game, process oriented, simple and rough, without considering the complex inheritance, is not considered abstract, without considering the polymorphism, is the development of the freestyle, roll up their sleeves and open up, but at the same time the logic, code reuse, reuse of data is greatly reduced. Process oriented is not a good game development model, either.
Component mode solves the defects of object oriented and process oriented, which is widely used in game client, Unity3d, unreal 4, and so on. Features of component patterns:
1. highly modular, a component is a data plus a section of logic
2. components can be hot plug, need to add, not need to delete
There is very little dependency between 3. types, and any type of addition or deletion of components does not affect other types.
At present, only a few have the server to use components design, the server should watch pioneer is the use of component design, and the pioneer developer called ECS architecture, in fact is a variant of component model, E is Entity, C is Component, S is System, which is the logic and data stripping assembly Component the logic part is called System, the topic Cheyuan, or to return to the ET framework.
The ET framework uses the design of components. Everything is Entity and Component, and any object that inherits from Entity can mount components, such as player objects:
```C#
public sealed class Player : Entity
{
	public string Account { get; private set; }
	public long UnitId { get; set; }
	
	public void Awake(string account)
	{
		this.Account = account;
	}
	
	public override void Dispose()
	{
		if (this.Id == 0)
		{
			return;
		}
		base.Dispose();
	}
}
```
.
To mount a MoveComponent mobile game player object component, so you can move the game player, game player to hang a backpack assembly, game player can manage the goods, to hang up the game player skill components, then you can spell the game player, plus the Buff component can manage buff.
```C#
player.AddComponent<MoveComponent>();
player.AddComponent<ItemsComponent>();
player.AddComponent<SpellComponent>();
player.AddComponent<BuffComponent>();
```
Component is highly reusable, such as a NPC, he can move to NPC MoveComponent hanging on the line, some NPC can also cast skills, then give it to hang SpellComponent, NPC does not require a backpack, then do not hang ItemsComponent
ET framework modules are all made up of components, and a process is made up of different components. For example, Loginserver needs external connections and also needs to be connected to the server, so login server hangs on

```C#
// Internal network component NetInnerComponent, processing the internal network connection
Game.Scene.AddComponent<NetInnerComponent, string, int>(innerConfig.Host, innerConfig.Port);
// External network component NetOuterComponent, processing and client connection
Game.Scene.AddComponent<NetOuterComponent, string, int>(outerConfig.Host, outerConfig.Port);
```
For example, battle server does not need external network connection (external network message is forwarded by gateserver), so it is natural to install only one intranet component.
```C#
// Internal network component NetInnerComponent processing for internal network connection
Game.Scene.AddComponent<NetInnerComponent, string, int>(innerConfig.Host, innerConfig.Port);
```
Like Unity3d components, the ET framework also provides component events, such as Awake, Start, Update, and so on. To add these events to a Component or Entity, you must write a helper class. For example, NetInnerComponent components require Awake and Update methods, and then add a class like this:
```C#
	[ObjectEvent]
	public class NetInnerComponentEvent : ObjectEvent<NetInnerComponent>, IAwake, IUpdate
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Update()
		{
			this.Get().Update();
		}
	}
```
In this way, NetInnerComponent calls its Awake method after AddComponent, and calls the Update method per frame.
ET did not like Unity use reflection to achieve this function, the reflection performance is poor, and thus realize the benefits of this class can be more on the hot DLL, this component of the Awake Start, Update method and other methods are more on the hot layer. Only the data is placed on the model layer to facilitate the heat to repair the logical bug.
The biggest advantage of component development is that, regardless of the novice or master, the development of a function can quickly know how to organize data, how to organize logic. Object can be completely abandoned. The most difficult thing to use object-oriented development is what class should I inherit? The most frightening thing ever was illusion three, and the inheritance structure of unreal three was very multi-layered, and did not know where to begin. In the end, it may lead to a very small function, inheriting one and its huge class, which is common in unreal three development. So unreal 4 uses component mode. The module isolation of component mode is very good. A component of the technical novice writes very badly, and it doesn't affect other modules, so it is impossible to rewrite this component.



