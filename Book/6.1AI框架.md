# AI框架
## 1. 几种AI的设计
AI在游戏中很多，但是为什么大家总是感觉ai编写起来十分困难，我后来思考了一番，主要原因是使用的方法不当。之前大家编写ai主要有几种方案：
### a. 状态机  
我是不知道谁想出来这个做法的，真是无力吐槽。本来对象身上任何数据都是状态，这种方法又要把一些状态定义成一种新的节点，对象身上状态变化会引起节点之间的转换，执行对应的方法，比如OnEnter OnExit等等。这里以怪物来举例，怪物可以分为多种状态，巡逻，攻击，追逐，返回。怪物的状态变化有:

巡逻->追逐  巡逻状态发现远处有敌人变追逐状态  
巡逻->攻击  巡逻发现可以攻击敌人变攻击状态  
攻击->追逐  攻击状态发现敌人有段距离于是去追逐  
攻击->返回  攻击状态发现距离敌人过远变返回状态  
追逐->返回  追逐状态发现距离敌人过远变返回状态  

太多状态转换了，这里有没有漏掉我已经难以发现了。一旦节点更多，任何两个节点都可能需要连接，将成为超级复杂的网状结构，复杂度是N的平方级，维护起来十分困难。为了解决网状结构变复杂的问题于是又升级为分层状态机等等。当然各种打补丁的方法还是没能解决本质的问题。用不好状态机不是你们的问题，是状态机的问题。

### b. 行为树
可能大家都觉得状态机解决复杂ai实在太困难了，于是有人想出了行为树来做ai。行为树的ai是响应式ai，这棵树从上往下（或者从左往右执行，这里以从上往下举例）实际上是把action节点排了个优先级，上面的action最先判断是否满足条件，满足则执行。这里就不详细讲了。行为树的复杂度是N，比状态机大大简化了，但是仍然存在不少缺陷，ai太复杂的时候，树会变得非常大，而且难以重构。比如我们自己项目，要做一个跟人差不多的机器人ai，自动做任务，打怪，玩游戏中的系统，跟人聊天，甚至攻击别人。想象一下，这颗树将变得多复杂！行为树的另外一个缺陷是某些action节点是个持久的过程，也就是说是个协程，行为树管理起协程起来不太好处理，比如上面的例子，需要移动到目标身边，这个移动究竟是做成协程呢，还是每帧move呢？这是个难题，怎么做都不舒服。

## 2. 我的做法
ai是什么呢？很简单啊，ai就是不停的根据当前的状态，执行相应的行为。记住这两句话，很重要，这就是ai的本质！这两句话分成两部分，一是状态判断，二是执行行为。状态判断好理解，行为是啥？以上面状态机的怪物举例子，怪物的行为就是 巡逻，攻击敌人，返回巡逻点。比如：

巡逻  （当怪物在巡逻范围内，周围没有敌人，选择下一个巡逻点，移动）  
攻击敌人  （当怪物发现警戒范围内有敌人，如果攻击距离够就攻击，不够就移动过去攻击）  
返回  （当怪物发现离出生点超过一定距离，加上无敌buff，往出生点移动,到了出生点，删除无敌buff）  

跟状态机不一样的是，这3个状态的变化完全不关心上一个状态是啥，只关心当前的条件是否满足，满足就执行行为。行为可能能瞬间执行，也可能是一段持续的过程，比如巡逻，选下一个巡逻点移动过去，走到了再选一个点，不停的循环。比如攻击敌人，可能需要移动到目标去攻击。

怎么设计这个ai框架呢？到这里就十分简单了，抽象出ai节点，每个节点包含条件判断，跟执行行为。行为方法应该是一个协程
```csharp
public class AINode
{
	public virtual bool Check(Unit unit) // 检测条件是否满足
	{		
	}

	public virtual ETTask Run(Unit unit)
	{		
	}
}
```
进一步思考，假如怪物在巡逻过程中，发现敌人，那么怪物应该要打断当前的巡逻，转而去执行攻击敌人的行为。因此我们行为应该需要支持被打断，也就是说行为协程应该支持取消，这点特别需要注意，行为Run方法中任何协程都要支持取消操作！
```csharp
public class AINode
{
	public virtual bool Check(Unit unit)
	{		
	}

	public virtual ETVoid Run(Unit unit, ETCancelToken cancelToken)
	{
	}
}
```

实现三个ai节点 XunLuoNode(巡逻)  GongjiNode(攻击)  FanHuiNode(返回)

```csharp
public class XunLuoNode: AINode
{
	public virtual bool Check(Unit unit)
	{
		if (不在巡逻范围)
		{
			return false;
		}
		if (周围有敌人)
		{
			return false;
		}
		return true;
	}

	public virtual ETVoid Run(Unit unit, ETCancelToken cancelToken)
	{
		while (true)
		{
			Vector3 nextPoint = FindNextPoint();
			bool ret = await MoveToAsync(nextPoint, cancelToken); // 移动到目标点, 返回false表示协程取消
			if (!ret)
			{
				return;
			}
			// 停留两秒, 注意这里要能取消，任何协程都要能取消
			bool ret = await TimeComponent.Instance.Wait(2000, cancelToken);
			if (!ret)
			{
				return;
			}
		}
	}
}
```
同理可以实现另外两个节点。光设计出节点还不行，还需要把各个节点串起来，这样ai才能转动
```csharp
AINode[] aiNodes = {xunLuoNode, gongjiNode, fanHuiNode};
AINode current;
ETCancelToken cancelToken;
while(true)
{
	// 每秒中需要重新判断是否满足新的行为了，这个时间可以自己定
	await TimeComponent.Instance.Wait(1000);

	AINode next;
	foreach(var node in aiNodes)
	{
		if (node.Check())
		{
			next = node;
			break;
		}
	}

	if (next == null)
	{
		continue;
	}

	// 如果下一个节点跟当前执行的节点一样，那么就不执行
	if (next == current)
	{
		continue;
	}

	// 停止当前协程
	cancelToken.Cancel();

	// 执行下一个协程
	cancelToken = new ETCancelToken();
	next.Run(unit, cancelToken).Coroutine();
}
```
这段代码十分简单，意思就是每秒钟遍历节点，直到找到一个满足条件的节点就执行，等下一秒再判断，执行下一个节点之前，先打断当前执行的协程。
几个使用误区:
1. 行为中如果有协程必须能够取消，并且传入cancelToken，否则会出大事，因为怪物一旦满足执行下个节点，需要取消当前协程。
2. 跟行为树与状态机不同，节点的作用只是一块逻辑，节点并不需要共享。共享的是协程方法，比如MoveToAsync，怪物巡逻节点可以使用，怪物攻击敌人节点中追击敌人也可以使用。
3. 节点可以做的非常庞大，比如自动做任务节点，移动到npc，接任务，根据任务的子任务做子任务，比如移动到怪点打怪，移动到采集物去采集等等，做完所有子任务，移动到交任务npc交任务。所有的一切都是写在一个while循环中，利用协程串起来。

思考一个大问题，怎么设计一个压测机器人呢？压测机器人需要做到什么？自动做任务，自动玩各种系统，自动攻击敌人，会反击，会找人聊天等等。把上面说的每一条做成一个ai节点即可。兄弟们，AI简不简单?



