using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class Property
    {
        private object value;

        public Type ValueType { get; private set; }

        public T Get<T>()
        {
            return (T)this.value;
        }

        public void Set(object value)
        {
            this.value = value;
        }

        public Property(object value)
        {
            this.value = value;
            this.ValueType = this.value.GetType();
        }
    }

    public class Properties
    {
        private Dictionary<string, Property> properties = new Dictionary<string, Property>();

        public Property this[string peopertyName]
        {
            get
            {
                return properties[peopertyName];
            }
            set
            {
                properties[peopertyName] = value;
            }
        }

        public void Remove(string peopertyName)
        {
            properties.Remove(peopertyName);
        }
    }
}
