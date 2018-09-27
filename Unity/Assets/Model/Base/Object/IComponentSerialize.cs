namespace ETModel
{
	// 在序列化前或者反序列化之后需要做一些操作，可以实现该接口，该接口的方法需要手动调用
	// 相比ISupportInitialize接口，BeginSerialize在BeginInit之前调用，EndDeSerialize在EndInit之后调用
	// 并且需要手动调用，可以在反序列化之后，在次方法中将注册组件到EventSystem之中等等
	public interface IComponentSerialize
	{
		// 序列化之前调用
		void BeginSerialize();
		// 反序列化之后调用
		void EndDeSerialize();
	}
}