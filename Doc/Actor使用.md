ET的Actor消息提供两种使用方法，假设a是一个Entity,在A服务器上,挂载了MailBoxComponent成了一个Actor,B服务器需要向a发送actor消息。

#### 1. B知道a的InstanceId，这个时候，B调用ActorMessageSender.GetWithActorId(long actorId)方法获取ActorMessageSender发送消息即可。

#### 2. B只能知道a的Id，因为a的InstanceId并不固定，这时可以让a创建后调用MailBoxComponent把自己注册到location上去，这样B调用ActorMessageSender.Get(long id)获取ActorMessageSender发送消息即可。

知道actor的原理就知道为什么有这两种模式:  
  
#### 1. 第一种，是最普通的actor消息，erlang就是第一种actor，因为instanceId中已经带有了对象的地址信息，所以，知道instanceId就知道了对象的地址，ActorMessageSender就能把消息发送到a所在的正确的服务器上。消息到了A服务器又能根据InstanceId找到a对象，这样自然能把消息发给a    
  
#### 2. 第二种，如果B服务器无法知道a的InstanceId，比如a有可能从一个进程切换到另外一个进程，这种情况很常见，比如从一个场景传送到另外一个场景。a的instanceId是会变化的。怎么保证B进程发送给a的消息能正确呢？这个时候就在创建a的时候把instanceId注册到location上去（调用AddLocation），这样ActorMessageSender会去location获取a的instanceId。然后把消息发送给a。