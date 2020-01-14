using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
	public class ComponentQueue: IDisposable
	{
		public long Id;
#if !SERVER
		public static GameObject Global { get; } = GameObject.Find("/Global");
		
		public GameObject ViewGO { get; set; }
#endif
		
		public string TypeName { get; }
		
		private readonly Queue<Entity> queue = new Queue<Entity>();

		public ComponentQueue(string typeName)
		{
			this.Id = IdGenerater.GenerateId();
			this.TypeName = typeName;
#if !SERVER
			this.ViewGO = new GameObject();
			this.ViewGO.name = this.GetType().Name;
			this.ViewGO.layer = LayerNames.GetLayerInt(LayerNames.HIDDEN);
			this.ViewGO.transform.SetParent(Global.transform, false);
			this.ViewGO.AddComponent<ComponentView>().Component = this;
#endif
		}

		public void Enqueue(Entity entity)
		{
			this.queue.Enqueue(entity);
		}

		public Entity Dequeue()
		{
			return this.queue.Dequeue();
		}

		public Entity Peek()
		{
			return this.queue.Peek();
		}

		public Queue<Entity> Queue
		{
			get
			{
				return this.queue;
			}
		}

		public int Count
		{
			get
			{
				return this.queue.Count;
			}
		}

		public void Dispose()
		{
			while (this.queue.Count > 0)
			{
				Entity component = this.queue.Dequeue();
				component.Dispose();
			}
		}
	}
	
    public class ObjectPool: IDisposable
    {
#if !SERVER
		public static GameObject Global { get; } = GameObject.Find("/Global");
		
		public GameObject ViewGO { get; set; }
#endif
	    
	    public string Name { get; set; }
	    
        private readonly Dictionary<Type, ComponentQueue> dictionary = new Dictionary<Type, ComponentQueue>();

        public ObjectPool()
        {
#if !SERVER
			this.ViewGO = new GameObject();
			this.ViewGO.name = this.GetType().Name;
			this.ViewGO.layer = LayerNames.GetLayerInt(LayerNames.HIDDEN);
			this.ViewGO.transform.SetParent(Global.transform, false);
			this.ViewGO.AddComponent<ComponentView>().Component = this;
#endif
        }

        public Entity Fetch(Type type)
        {
	        Entity obj;
            if (!this.dictionary.TryGetValue(type, out ComponentQueue queue))
            {
	            obj = (Entity)Activator.CreateInstance(type);
            }
	        else if (queue.Count == 0)
            {
	            obj = (Entity)Activator.CreateInstance(type);
            }
            else
            {
	            obj = queue.Dequeue();
            }
	        
	        obj.IsFromPool = true;
            return obj;
        }

        public T Fetch<T>() where T: Entity
		{
            T t = (T) this.Fetch(typeof(T));
			return t;
		}
        
        public void Recycle(Entity obj)
        {
            Type type = obj.GetType();
	        ComponentQueue queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new ComponentQueue(type.Name);
	            
#if !SERVER
	            if (queue.ViewGO != null)
	            {
		            queue.ViewGO.transform.SetParent(this.ViewGO.transform);
		            queue.ViewGO.name = $"{type.Name}s";
	            }
#endif
				this.dictionary.Add(type, queue);
            }
            
#if !SERVER
	        if (obj.ViewGO != null)
	        {
		        obj.ViewGO.transform.SetParent(queue.ViewGO.transform);
	        }
#endif
	        obj.Id = 0;
            queue.Enqueue(obj);
        }

	    public void Dispose()
	    {
		    foreach (var kv in this.dictionary)
		    {
			    kv.Value.Dispose();
		    }
		    this.dictionary.Clear();
	    }
	    
	    public override string ToString()
	    {
		    StringBuilder sb = new StringBuilder();
		    Dictionary<Type, int> typeCount = new Dictionary<Type, int>();
		    foreach (var kv in this.dictionary)
		    { 
			    typeCount[kv.Key] = kv.Value.Count;
		    }

		    IOrderedEnumerable<KeyValuePair<Type, int>> orderByDescending = typeCount.OrderByDescending(s => s.Value);
			
		    sb.AppendLine("ObjectPool Count: ");
		    foreach (var kv in orderByDescending)
		    {
			    if (kv.Value == 1)
			    {
				    continue;
			    }
			    sb.AppendLine($"\t{kv.Key.Name}: {kv.Value}");
		    }

		    MultiMapSet<string, string> dict = Check();
		    
		    sb.Append("not reset field:\n");
		    foreach (KeyValuePair<string,HashSet<string>> pair in dict.GetDictionary())
		    {
			    sb.Append(pair.Key + ": ");
			    foreach (string value in pair.Value)
			    {
				    sb.Append(value + ", ");
			    }
			    sb.Append("\n");
		    }

		    return sb.ToString();
	    }

	    public void LogErrorCheckResult()
	    {
		    MultiMapSet<string, string> dict = Check();
		    if (dict.Count == 0)
		    {
			    return;
		    }
		    StringBuilder sb = new StringBuilder();
		    sb.Append("not reset field:\n");
		    foreach (KeyValuePair<string,HashSet<string>> pair in dict.GetDictionary())
		    {
			    sb.Append(pair.Key + ": ");
			    foreach (string value in pair.Value)
			    {
				    sb.Append(value + ", ");
			    }
			    sb.Append("\n");
		    }
		    Log.Error(sb.ToString());
	    }

	    public MultiMapSet<string, string> Check()
	    {
		    MultiMapSet<string, string> dict = new MultiMapSet<string, string>();
		    foreach (ComponentQueue queue in this.dictionary.Values)
		    {
			    foreach (Entity entity in queue.Queue)
			    {
				    Type type = entity.GetType();
				    
#if SERVER
				    if (type.IsSubclassOf(typeof (LogDefine)))
				    {
					    continue;
				    }
#endif
				    
				    FieldInfo[] fieldInfos = type.GetFields();
				    foreach (FieldInfo fieldInfo in fieldInfos)
				    {
					    if (fieldInfo.IsLiteral)
					    {
						    continue;
					    }

					    if (fieldInfo.GetCustomAttributes(typeof (NoMemoryCheck)).Count() > 0)
					    {
						    continue;
					    }
					    
					    Type fieldType = fieldInfo.FieldType;
					    if (fieldType == typeof (int))
					    {
						    if ((int) fieldInfo.GetValue(entity) != 0)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }
					    
					    if (fieldType == typeof (uint))
					    {
						    if ((uint) fieldInfo.GetValue(entity) != 0)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }
					    
					    if (fieldType == typeof (long))
					    {
						    if ((long) fieldInfo.GetValue(entity) != 0)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }
					    
					    if (fieldType == typeof (ulong))
					    {
						    if ((ulong) fieldInfo.GetValue(entity) != 0)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }
					    
					    if (fieldType == typeof (short))
					    {
						    if ((short) fieldInfo.GetValue(entity) != 0)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }
					    
					    if (fieldType == typeof (ushort))
					    {
						    if ((ushort) fieldInfo.GetValue(entity) != 0)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }
					    
					    if (fieldType == typeof (float))
					    {
						    if (Math.Abs((float)fieldInfo.GetValue(entity)) > 0.0001)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }
					    
					    if (fieldType == typeof (double))
					    {
						    if (Math.Abs((double)fieldInfo.GetValue(entity)) > 0.0001)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }
					    
					    if (fieldType == typeof (bool))
					    {
						    if ((bool) fieldInfo.GetValue(entity) != false)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }
					    
					    if (typeof(ICollection).IsAssignableFrom(fieldType))
					    {
						    object fieldValue = fieldInfo.GetValue(entity);
						    if (fieldValue == null)
						    {
							    continue;
						    }
						    if (((ICollection)fieldValue).Count != 0)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }

					    PropertyInfo propertyInfo = fieldType.GetProperty("Count");
					    if (propertyInfo != null)
					    {
						    if ((int) propertyInfo.GetValue(fieldInfo.GetValue(entity)) != 0)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }

					    if (fieldType.IsClass)
					    {
						    if (fieldInfo.GetValue(entity) != null)
						    {
							    dict.Add(type.Name, fieldInfo.Name);
						    }
						    continue;
					    }
				    }
			    }
		    }

		    return dict;
	    }
    }
}