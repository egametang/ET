using ILRuntime.Runtime.Intepreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntime.Runtime.Stack
{
    unsafe struct MemoryBlockInfo
    {
        public StackObject* RequestAddress;
        public StackObject* StartAddress;
        public int Size;
        public int ManagedIndex;
        public int ManagedCount;
    }

    public unsafe struct StackObjectAllocation
    {
        public StackObject* Address;
        public int ManagedIndex;
    }
    public unsafe delegate void StackObjectAllocateCallback(int size, out StackObject* ptr, out int managedIdx);
    public unsafe class StackObjectAllocator
    {
        MemoryBlockInfo[] freeBlocks;
        StackObjectAllocateCallback allocCallback;

        public StackObjectAllocator(StackObjectAllocateCallback cb)
        {
            allocCallback = cb;
            freeBlocks = new MemoryBlockInfo[8];
        }

        public void Clear()
        {
            for(int i = 0; i < freeBlocks.Length; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                    break;
                freeBlocks[i] = default(MemoryBlockInfo);
            }
        }

        void ExpandFreeList()
        {
            int expandSize = Math.Min(freeBlocks.Length, 32);
            MemoryBlockInfo[] newArr = new MemoryBlockInfo[freeBlocks.Length + expandSize];
            freeBlocks.CopyTo(newArr, 0);
            freeBlocks = newArr;
        }

        void FreeBlock(int idx)
        {
            freeBlocks[idx].RequestAddress = null;
            var cnt = freeBlocks.Length;
            int freeSize = 0;
            int freeManaged = 0;
            int freeBlock = 0;
            for (int i = idx-1; i >= 0; i--)
            {
                if (freeBlocks[i].RequestAddress == null)
                {
                    idx = i;
                }
                else
                    break;
            }
            for (int i = idx + 1; i < cnt; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                    break;
                if (freeBlocks[i].RequestAddress == null)
                {
                    freeSize += freeBlocks[i].Size;
                    freeManaged += freeBlocks[i].ManagedCount;
                    freeBlock++;
                }
                else
                    break;
            }
            if (freeBlock > 0)
            {
                freeBlocks[idx].Size += freeSize;
                freeBlocks[idx].ManagedCount += freeManaged;
                int tail = idx + freeBlock + 1;
                if (tail < freeBlocks.Length)
                {
                    Array.Copy(freeBlocks, tail, freeBlocks, idx + 1, cnt - tail);
                }
                for (int i = cnt - freeBlock; i < cnt; i++)
                {
                    freeBlocks[i] = default(MemoryBlockInfo);
                }
            }
        }

        public void FreeBefore(StackObject* ptr)
        {
            int firstHit = -1;
            var cnt = freeBlocks.Length;
            for (int i = 0; i < cnt; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                    break;
                if (freeBlocks[i].StartAddress <= ptr)
                {
                    freeBlocks[i] = default(MemoryBlockInfo);
                    if (firstHit < 0)
                        firstHit = i;
                }
            }
            if (firstHit >= 0)
            {
                int validIdx = 0;
                for (int i = firstHit; i < cnt; i++)
                {
                    if (freeBlocks[i].StartAddress != null)
                    {
                        if (validIdx != i)
                        {
                            freeBlocks[validIdx++] = freeBlocks[i];
                        }
                    }
                }
            }
        }

        public void Free(StackObject* ptr)
        {
            var cnt = freeBlocks.Length;
            for (int i = 0; i < cnt; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                    break;
                if (freeBlocks[i].RequestAddress == ptr)
                {
                    FreeBlock(i);
                    break;
                }
            }
        }

        public void RegisterAllocation(StackObject* ptr, StackObject* src, int size, int managedIndex, int managedCount)
        {
            int emptyIndex = -1;
            int cnt = freeBlocks.Length;
            for (int i = 0; i < cnt; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                {
                    emptyIndex = i;
                    break;
                }
            }
            if (emptyIndex == -1)
            {
                emptyIndex = freeBlocks.Length;
                ExpandFreeList();
            }
            StackObject* dst;
            int mIdx;
            allocCallback(size, out dst, out mIdx);
            if (dst != src)
                throw new NotSupportedException();
            freeBlocks[emptyIndex] = new MemoryBlockInfo()
            {
                StartAddress = dst,
                RequestAddress = ptr,
                Size = size,
                ManagedCount = managedCount,
                ManagedIndex = managedIndex != int.MaxValue ? managedIndex : mIdx
            };
        }

        public bool AllocExisting(StackObject* ptr, int size, int managedSize, out StackObjectAllocation alloc)
        {
            int cnt = freeBlocks.Length;
            for (int i = 0; i < cnt; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                {
                    break;
                }
                if (freeBlocks[i].RequestAddress == ptr || freeBlocks[i].RequestAddress == null)
                {
                    if (freeBlocks[i].Size >= size && freeBlocks[i].ManagedCount >= managedSize)
                    {
                        freeBlocks[i].RequestAddress = ptr;
                        alloc = new StackObjectAllocation()
                        {
                            Address = freeBlocks[i].StartAddress,
                            ManagedIndex = freeBlocks[i].ManagedIndex
                        };
                        return true;
                    }
                }
            }
            alloc = new StackObjectAllocation();
            return false;
        }
        public StackObjectAllocation Alloc(StackObject* ptr, int size, int managedSize)
        {
            int found = -1;
            int emptyIndex = -1;
            StackObjectAllocation alloc;
            int cnt = freeBlocks.Length;
            for (int i = 0; i < cnt; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                {
                    emptyIndex = i;
                    break;
                }
                if (freeBlocks[i].RequestAddress == ptr)
                {
                    if (freeBlocks[i].Size >= size && freeBlocks[i].ManagedCount >= managedSize)
                    {
                        return new StackObjectAllocation()
                        {
                            Address = freeBlocks[i].StartAddress,
                            ManagedIndex = freeBlocks[i].ManagedIndex
                        };
                    }
                    //freeBlocks[i].RequestAddress = null;
                    //freeIndex = i;
                    FreeBlock(i);
                }
            }
            for (int i = 0; i < cnt; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                    break;
                if (freeBlocks[i].RequestAddress == null)
                {
                    if (freeBlocks[i].Size >= size && freeBlocks[i].ManagedCount >= managedSize)
                    {
                        found = i;
                        break;
                    }
                }
            }
            if (found >= 0)
            {
                freeBlocks[found].RequestAddress = ptr;
                return new StackObjectAllocation()
                {
                    Address = freeBlocks[found].StartAddress,
                    ManagedIndex = freeBlocks[found].ManagedIndex
                };
            }
            else
            {
                if (emptyIndex == -1)
                {
                    emptyIndex = freeBlocks.Length;
                    ExpandFreeList();
                }
                allocCallback(size, out alloc.Address, out alloc.ManagedIndex);
                freeBlocks[emptyIndex] = new MemoryBlockInfo()
                {
                    StartAddress = alloc.Address,
                    RequestAddress = ptr,
                    Size = size,
                    ManagedCount = managedSize,
                    ManagedIndex = alloc.ManagedIndex
                };
            }
            return alloc;
        }
    }
}
