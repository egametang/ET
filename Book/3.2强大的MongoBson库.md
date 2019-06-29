# 强大的MongoBson库
后端开发，统计了一下大概有这些场景需要用到序列化：
1. 对象通过序列化反序列化clone
2. 服务端数据库存储数据，二进制
3. 分布式服务端，多进程间的消息，二进制
4. 后端日志，文本格式
5. 服务端的各种配置文件，文本格式

C#序列化库有非常非常多了，protobuf，json等等。但是这些序列化库都无法应当所有场景，既要可读又要小。protobuf不支持复杂的对象结构（无法使用继承），做消息合适，做数据库存储和日志格式并不好用。json做日志格式合适，但是做网络消息和数据存储就太大。我们当然希望一个库能满足上面所有场景，理由如下：
1. 你想想某天你的配置文件需要放到数据库中保存，你不需要进行格式转换，后端直接把前端发过来的配置消息保存到数据库中，这是不是能减少非常多错误呢？
2. 某天有些服务端的配置文件不用文件格式了，需要放在数据库中，同样，只需要几行代码就可以完成迁移。
3. 某天后端服务器crash，你需要扫描日志进行数据恢复，把日志进行反序列化成C#对象，一条条进行处理，再转成对象保存到数据库就完成了。
4. 对象保存在数据库，直接就可以看到文本内容，可以做各种类sql的操作
5. 想像一个场景，一个配置文本对象，反序列化到内存，通过网络消息发送，存储到数据库中。整个过程一气呵成。

简单来说就是减少各种数据转换，减少代码，提高开发效率，提高可维护性。当然，Mongo Bson就能够满足。MongoDB库既可以序列化成文本也可以序列化成BSON的二进制格式，并且MongoDB本身就是一个游戏中使用非常多的数据库。Mongo Bson非常完善，是我见过功能最全使用最强大的序列化库，有些功能十分贴心。其支持功能如下：
1. 支持复杂的继承结构
2. 支持忽略某些字段序列化
3. 支持字段默认值
4. 结构多出多余的字段照样可以反序列化，这对多版本协议非常有用
5. 支持ISupportInitialize接口使用，这个在反序列化的时候简直就是神器
6. 支持文本json和二进制bson序列化
7. MongoDB数据库支持

简单的介绍下mongo bson库
### 1.支持序列化反序列化成json或者bson
```csharp
    public sealed class Player
    {
        public long Id;

        public string Account { get; private set; }

        public long UnitId { get; set; }
    }

    Player player1 = new Player() { Id = 1 };
    string json = player1.ToJson();
    Console.WriteLine($"player1 to json: {json}");
    Console.WriteLine($"player to bson: {player.ToBson().ToHex()}");
    // output:
    // player to json: { "_id" : NumberLong(1), "C" : [], "Account" : null, "UnitId" : NumberLong(0) }
    // player to bson: B000000125F69640001000000000000000A4163636F756E740012556E6974496400000000000000000000


```
注意mongo的json跟标准的json有点区别，如果想用标准的json，可以传入一个JsonWriterSettings对象，限制使用JsonOutputMode.Strict模式
```csharp
    // 使用标准json
    Player player2 = new Player() { Id = 1 };
    Console.WriteLine($"player to json: {player2.ToJson(new JsonWriterSettings() {OutputMode = JsonOutputMode.Strict})}");
    // player to json: { "_id" : 1, "C" : [], "Account" : null, "UnitId" : 0 }
```

反序列化json:
```csharp
            // 反序列化json
        Player player11 = BsonSerializer.Deserialize<Player>(json);
        Console.WriteLine($"player11 to json: {player11.ToJson()}");
```
反序列化bson:
```csharp
    // 反序列化bson
    using (MemoryStream memoryStream = new MemoryStream(bson))
    {
        Player player12 = (Player) BsonSerializer.Deserialize(memoryStream, typeof (Player));
        Console.WriteLine($"player12 to json: {player12.ToJson()}");
    }
```

### 2.可以忽略某些字段
[BsonIgnore]该标签用来禁止字段序列化。
```csharp
	public sealed class Player
	{
        public long Id;

		[BsonIgnore]
		public string Account { get; private set; }
		
		public long UnitId { get; set; }
    ｝

    Player player = new Player() { Id = 2, UnitId = 3, Account = "panda"};
	Console.WriteLine($"player to json: {player.ToJson()}");
    // player to json: { "_id" : 2, "UnitId" : 3 }
```
### 3.支持默认值以及取别名
[BsonElement] 字段加上该标签，即使是private字段也会序列化(默认只序列化public字段)，该标签还可以带一个string参数，给字段序列化指定别名。
```csharp
	public sealed class Player
	{
        public long Id;

		public string Account { get; private set; }

		[BsonElement("UId")]
		public long UnitId { get; set; }
    ｝
    Player player = new Player() { Id = 2, UnitId = 3, Account = "panda"};
	Console.WriteLine($"player to json: {player.ToJson()}");
    // player to json: { "_id" : 2, "Account" : "panda", "UId" : 3 }
```
### 4.升级版本支持
[BsonIgnoreExtraElements] 该标签用在class上面，反序列化时用来忽略多余的字段，一般版本兼容需要考虑，低版本的协议需要能够反
序列化高版本的内容,否则新版本加了字段，旧版本结构反序列化会出错
```csharp
	[BsonIgnoreExtraElements]
	public sealed class Player
	{
        public long Id;

		public string Account { get; private set; }

		[BsonElement("UId")]
		public long UnitId { get; set; }
    ｝
```
### 5.支持复杂的继承结构
mongo bson库强大的地方在于完全支持序列化反序列化继承结构。需要注意的是，继承反序列化需要注册所有的父类，有两种方法：
a. 你可以在父类上面使用[BsonKnownTypes]标签声明继承的子类，这样mongo会自动注册，例如:
```csharp
    [BsonKnownTypes(typeof(Entity))]
    public class Component
    {
    }
    [BsonKnownTypes(typeof(Player))]
    public class Entity: Component
    {
    }
    public sealed class Player: Entity
    {
        public long Id;
        
        public string Account { get; set; }
		
        public long UnitId { get; set; }
    }
```
这样有缺陷，因为框架并不知道一个类会有哪些子类，这样做对框架代码有侵入性，我们希望能解除这个耦合
。可以扫描程序集中所有子类父类的类型，将他们注册到mongo驱动中
```csharp
			Type[] types = typeof(Game).Assembly.GetTypes();
			foreach (Type type in types)
			{
				if (!type.IsSubclassOf(typeof(Component)))
				{
					continue;
				}

				BsonClassMap.LookupClassMap(type);
			}

			BsonSerializer.RegisterSerializer(new EnumSerializer<NumericType>(BsonType.String));
```
这样完全的自动化注册，使用者也不需要关系类是否注册。

### 6.ISupportInitialize接口
mongo bson反序列化时支持一个ISupportInitialize接口，ISupportInitialize有两个方法
```csharp
    public interface ISupportInitialize
    {
        void BeginInit();
        void EndInit();
    }
```
BeginInit在反序列化前调用，EndInit在反序列化后调用。这个接口非常有用了，可以在反序列化后执行一些操作。例如
```csharp
	[BsonIgnoreExtraElements]
	public class InnerConfig: AConfigComponent
	{
		[BsonIgnore]
		public IPEndPoint IPEndPoint { get; private set; }
		
		public string Address { get; set; }

		public override void EndInit()
		{
			this.IPEndPoint = NetworkHelper.ToIPEndPoint(this.Address);
		}
	}
```
InnerConfig是ET中进程内网地址的配置，由于IPEndPoint不太好配置，我们可以配置成string形式，然后反序列化的时候在EndInit中把string转换成IPEndPoint。
同样我给protobuf反序列化方法也加上了这个调用，参考ProtobufHelper.cs，ET的protobuf因为要支持ilruntime，所以去掉了map的支持，假如我们想要一个map怎么办呢？这里我给生成的代码都做了手脚，把proto消息都改成了partial class，这样我们可以自己扩展这个class，比如：
```csharp
message UnitInfo
{
	int64 UnitId  = 1;

	float X = 2;
	float Y = 3;
	float Z = 4;
}

// protobuf
message G2C_EnterMap // IResponse
{
	int32 RpcId = 90;
	int32 Error = 91;
	string Message = 92;
	// 自己的unit id
	int64 UnitId = 1;
	// 所有的unit
	repeated UnitInfo Units = 2;
}
```
这个网络消息有个repeated UnitInfo字段，在protobuf中其实是个数组，使用起来不是很方便，我希望转成一个Dictionary<Int64, UnitInfo>的字段，我们可以做这样的操作：
```csharp
    public partial class G2C_EnterMap: ISupportInitialize
    {
        public Dictionary<Int64, UnitInfo> unitsDict = new Dictionary<long, UnitInfo>();
        
        public void BeginInit()
        {
        }

        public void EndInit()
        {
            foreach (var unit in this.Units)
            {
                this.unitsDict.Add(unit.UnitId, unit);
            }
        }
    }
```
通过这样一段代码把消息进行扩展一下，反序列化出来之后，自动转成了一个Dictionary。