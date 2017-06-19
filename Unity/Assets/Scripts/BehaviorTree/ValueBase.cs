using System;

namespace Model
{
	[Serializable]
	public class ValueBase
	{
		private object value;

		public ValueBase(object v)
		{
			this.value = v;
		}

		public ValueBase()
		{
		}

		public ValueBase Clone()
		{
			ValueBase v;
			Type vType = this.value.GetType();
			if (vType.IsSubclassOf(typeof(Array)))
			{
				Array sourceArray = (Array) this.value;
				Array dest = Array.CreateInstance(vType.GetElementType(), sourceArray.Length);
				Array.Copy(sourceArray, dest, dest.Length);
				v = new ValueBase(dest);
			}
			else
			{
				v = new ValueBase(value);
			}
			return v;
		}

		public object GetValue()
		{
			return this.value;
		}
		
		public void SetValue(object v)
		{
			this.value = v;
		}
	}
}