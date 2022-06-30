#pragma once
#include "../CommonDef.h"

namespace huatuo
{
namespace interpreter
{

	struct HtVector2f
	{
		float x;
		float y;
	};
	static_assert(sizeof(HtVector2f) == 8, "Vector2f");

	struct HtVector3f
	{
		float x;
		float y;
		float z;
	};
	static_assert(sizeof(HtVector3f) == 12, "Vector3f");

	struct HtVector4f
	{
		float x;
		float y;
		float z;
		float w;
	};
	static_assert(sizeof(HtVector4f) == 16, "Vector4f");

	struct HtVector2d
	{
		double x;
		double y;
	};
	static_assert(sizeof(HtVector2d) == 16, "Vector2d");

	struct HtVector3d
	{
		double x;
		double y;
		double z;
	};
	static_assert(sizeof(HtVector3d) == 24, "Vector3d");

	struct HtVector4d
	{
		double x;
		double y;
		double z;
		double w;
	};
	static_assert(sizeof(HtVector4d) == 32, "Vector4d");

	struct HtVector2i
	{
		int32_t x;
		int32_t y;
	};
	static_assert(sizeof(HtVector2i) == 8, "IntVector2i");

	struct HtVector3i
	{
		int32_t x;
		int32_t y;
		int32_t z;
	};
	static_assert(sizeof(HtVector3i) == 12, "IntVector3i");

	struct HtVector4i
	{
		int32_t x;
		int32_t y;
		int32_t z;
		int32_t w;
	};
	static_assert(sizeof(HtVector4i) == 16, "IntVector4i");

#pragma endregion

}
}
