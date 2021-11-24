using System;
using ProtoBuf.Meta;

namespace ProtoBuf
{
    internal sealed class NetObjectCache
    {
        internal const int Root = 0;
        private MutableList underlyingList;

        private MutableList List
        {
            get
            {
                if (underlyingList == null) underlyingList = new MutableList();
                return underlyingList;
            }
        }


        internal object GetKeyedObject(int key)
        {
            if (key-- == Root)
            {
                if (rootObject == null) throw new ProtoException("No root object assigned");
                return rootObject;
            }
            BasicList list = List;

            if (key < 0 || key >= list.Count)
            {
                Helpers.DebugWriteLine("Missing key: " + key);
                throw new ProtoException("Internal error; a missing key occurred");
            }

            object tmp = list[key];
            if (tmp == null)
            {
                throw new ProtoException("A deferred key does not have a value yet");
            }
            return tmp;
        }

        internal void SetKeyedObject(int key, object value)
        {
            if (key-- == Root)
            {
                if (value == null) throw new ArgumentNullException("value");
                if (rootObject != null && ((object)rootObject != (object)value)) throw new ProtoException("The root object cannot be reassigned");
                rootObject = value;
            }
            else
            {
                MutableList list = List;
                if (key < list.Count)
                {
                    object oldVal = list[key];
                    if (oldVal == null)
                    {
                        list[key] = value;
                    }
                    else if (!ReferenceEquals(oldVal, value) )
                    {
                        throw new ProtoException("Reference-tracked objects cannot change reference");
                    } // otherwise was the same; nothing to do
                }
                else if (key != list.Add(value))
                {
                    throw new ProtoException("Internal error; a key mismatch occurred");
                }
            }
        }

        private object rootObject;
        internal int AddObjectKey(object value, out bool existing)
        {
            if (value == null) throw new ArgumentNullException("value");

            if ((object)value == (object)rootObject) // (object) here is no-op, but should be
            {                                        // preserved even if this was typed - needs ref-check
                existing = true;
                return Root;
            }

            string s = value as string;
            BasicList list = List;
            int index;

#if NO_GENERICS
            
            if(s == null)
            {
                if (objectKeys == null)
                {
                    objectKeys = new ReferenceHashtable();
                    index = -1;
                }
                else
                {
                    object tmp = objectKeys[value];
                    index = tmp == null ? -1 : (int) tmp;
                }
            }
            else
            {
                if (stringKeys == null)
                {
                    stringKeys = new Hashtable();
                    index = -1;
                }
                else
                {
                    object tmp = stringKeys[s];
                    index = tmp == null ? -1 : (int) tmp;
                }
            }
#else

            if(s == null)
            {
#if CF || PORTABLE // CF has very limited proper object ref-tracking; so instead, we'll search it the hard way
                index = list.IndexOfReference(value);
#else
                if (objectKeys == null) 
                {
                    objectKeys = new System.Collections.Generic.Dictionary<object, int>(ReferenceComparer.Default);
                    index = -1;
                }
                else
                {
                    if (!objectKeys.TryGetValue(value, out index)) index = -1;
                }
#endif
            }
            else
            {
                if (stringKeys == null)
                {
                    stringKeys = new System.Collections.Generic.Dictionary<string, int>();
                    index = -1;
                } 
                else
                {
                    if (!stringKeys.TryGetValue(s, out index)) index = -1;
                }
            }
#endif

            if (!(existing = index >= 0))
            {
                index = list.Add(value);

                if (s == null)
                {
#if !CF && !PORTABLE // CF can't handle the object keys very well
                    objectKeys.Add(value, index);
#endif
                }
                else
                {
                    stringKeys.Add(s, index);
                }
            }
            return index + 1;
        }

        private int trapStartIndex; // defaults to 0 - optimization for RegisterTrappedObject
                                    // to make it faster at seeking to find deferred-objects

        internal void RegisterTrappedObject(object value)
        {
            if (rootObject == null)
            {
                rootObject = value;
            }
            else
            {
                if(underlyingList != null)
                {
                    for (int i = trapStartIndex; i < underlyingList.Count; i++)
                    {
                        trapStartIndex = i + 1; // things never *become* null; whether or
                                                // not the next item is null, it will never
                                                // need to be checked again

                        if(underlyingList[i] == null)
                        {
                            underlyingList[i] = value;    
                            break;
                        }
                    }
                }
            }
        }
#if NO_GENERICS
        private ReferenceHashtable objectKeys;
        private System.Collections.Hashtable stringKeys;
        private class ReferenceHashtable : System.Collections.Hashtable
        {
            protected override int GetHash(object key)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(key);
            }
            protected override bool KeyEquals(object item, object key)
            {
                return item == key;
            }
        }   
#else

        private System.Collections.Generic.Dictionary<string, int> stringKeys;

#if !CF && !PORTABLE // CF lacks the ability to get a robust reference-based hash-code, so we'll do it the harder way instead
        private System.Collections.Generic.Dictionary<object, int> objectKeys;
        private sealed class ReferenceComparer : System.Collections.Generic.IEqualityComparer<object>
        {
            public readonly static ReferenceComparer Default = new ReferenceComparer();
            private ReferenceComparer() {}

            bool System.Collections.Generic.IEqualityComparer<object>.Equals(object x, object y)
            {
                return x == y; // ref equality
            }

            int System.Collections.Generic.IEqualityComparer<object>.GetHashCode(object obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
#endif

#endif

        internal void Clear()
        {
            trapStartIndex = 0;
            rootObject = null;
            if (underlyingList != null) underlyingList.Clear();
            if (stringKeys != null) stringKeys.Clear();
#if !CF && !PORTABLE
            if (objectKeys != null) objectKeys.Clear();
#endif
        }
    }
}
