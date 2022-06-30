#pragma once

#include <stack>
#include <cmath>
#include <vector>

#include "../CommonDef.h"

namespace huatuo
{
	namespace transform
	{
		const size_t kMinBlockSize = 1024 * 1024;

		class TemporaryMemoryArena
		{
		public:

			TemporaryMemoryArena() : _buf(nullptr), _size(0), _pos(0)
			{
				Begin();
			}
			~TemporaryMemoryArena()
			{
				End();
			}

			static size_t AligndSize(size_t size)
			{
				return (size + 7) & ~7;
			}

			template<typename T>
			T* AllocIR()
			{
				const size_t aligndSize = AligndSize(sizeof(T));
				if (_pos + aligndSize <= _size)
				{
					T* ir = (T*)(_buf + _pos);
					*ir = {};
					_pos += aligndSize;
					return ir;
				}

				RequireSize(aligndSize);

				T* ir = (T*)(_buf + _pos);
				*ir = {};
				_pos += aligndSize;
				return ir;
			}

			template<typename T>
			T* NewAny()
			{
				const size_t needSize = AligndSize(sizeof(T));
				if (_pos + needSize <= _size)
				{
					T* ir = new (_buf + _pos) T();
					*ir = {};
					_pos += needSize;
					return ir;
				}

				RequireSize(needSize);

				T* ir = new (_buf + _pos) T();
				*ir = {};
				_pos += needSize;
				return ir;
			}

			template<typename T>
			T* NewNAny(int n)
			{
				if (n > 0)
				{
					size_t bytes = AligndSize(sizeof(T) * n);
					if (_pos + bytes > _size)
					{
						RequireSize(bytes);
					}
					T* ret = new (_buf + _pos) T[n];
					_pos += bytes;
					return ret;
				}
				else
				{
					return nullptr;
				}
			}

			void Begin();

			void End();

		private:
			struct Block
			{
				void* data;
				size_t size;
			};

			void RequireSize(size_t size)
			{
				if (_buf)
				{
					_useOuts.push_back({ (void*)_buf, _size});
				}

				Block newBlock = AllocBlock(std::max(size, kMinBlockSize));
				_buf = (byte*)newBlock.data;
				_size = newBlock.size;
				_pos = 0;
			}

			static Block AllocBlock(size_t size);

			std::vector<Block> _useOuts;

			byte* _buf;
			size_t _size;
			size_t _pos;
		};
	}
}