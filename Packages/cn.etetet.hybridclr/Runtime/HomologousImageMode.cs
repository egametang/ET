
namespace HybridCLR
{
    public enum HomologousImageMode
	{
		Consistent,	// AOT dll需要跟主工程精确一致，即为裁剪后的AO dll
		SuperSet,  // AOT dll不需要跟主工程精确一致，但必须包含裁剪后的AOT dll的所有元数据，即为裁剪后dll的超集。推荐使用原始aot dll
	}
}

