using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace MongoDB.Bson.Serialization
{
	/// <summary>
	/// mongodb在unity3d上的bug,这几个函数用来辅助修改bug
	/// </summary>
	public static class ReaderWriterLockExtension
	{
		public static void EnterReadLock(this ReaderWriterLock locker)
		{
			locker.AcquireReaderLock(100000);
		}

		public static void EnterWriteLock(this ReaderWriterLock locker)
		{
			locker.AcquireWriterLock(100000);
		}

		public static void ExitReadLock(this ReaderWriterLock locker)
		{
			locker.ReleaseReaderLock();
		}

		public static void ExitWriteLock(this ReaderWriterLock locker)
		{
			locker.ReleaseWriterLock();
		}
	}

	public static class TypeExtension
	{
		public static MemberInfo[] GetMember2(this Type type, string name, MemberTypes memberTypes, BindingFlags bindingFlags)
		{
			MemberInfo[] members = type.GetMember(name, memberTypes, bindingFlags);
			List<MemberInfo> membersList = new List<MemberInfo>();
			foreach (MemberInfo info in members)
			{
				if ((bindingFlags & BindingFlags.DeclaredOnly) != 0 && info.DeclaringType != type)
				{
					continue;
				}
				membersList.Add(info);
			}
			return membersList.ToArray();
		}

		public static MemberInfo[] GetMember2(this Type type, string name, BindingFlags bindingFlags)
		{
			MemberInfo[] members = type.GetMember(name, bindingFlags);
			List<MemberInfo> membersList = new List<MemberInfo>();
			foreach (MemberInfo info in members)
			{
				if ((bindingFlags & BindingFlags.DeclaredOnly) != 0 && info.DeclaringType != type)
				{
					continue;
				}
				membersList.Add(info);
			}
			return membersList.ToArray();
		}
	}
}
