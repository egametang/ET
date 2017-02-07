using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Model
{
    public class BehaviorNodeConfig : MonoBehaviour 
    {
        public int id;

        public string name;

        public string describe;

        public BehaviorNodeConfig(string name, string _desc,int _id = 0)
        {
            this.name = name;
            describe = _desc;
            id = _id;
        }

        public BehaviorTreeArgsDict GetArgsDict()
        {
            BehaviorTreeArgsDict dict = new BehaviorTreeArgsDict();
            foreach (var item in gameObject.GetComponents<BTTypeBaseComponent>())
            {
                FieldInfo info = item.GetType().GetField("fieldValue");
                ValueBase valueBase = new ValueBase();
               if (item.GetType() == typeof(BTEnumComponent))
               {
                   
                    valueBase.SetValueByType(typeof(Enum), info.GetValue(item));
                }
               else
                {
                    valueBase.SetValueByType(info.FieldType, info.GetValue(item));
                }
                
                dict.Add(item.fieldName,valueBase);
            }
            return dict;
        }
        public void SetValue(Type type,string fieldName, object value)
        {
            foreach (BTTypeBaseComponent item in gameObject.GetComponents<BTTypeBaseComponent>())
            {
                if (fieldName == item.fieldName)
                {
                    object fieldValue = value;
                    FieldInfo fieldValueinfo = item.GetType().GetField("fieldValue");
                    if (BehaviorTreeArgsDict.IsEnumType(type))
                    {
                        fieldValue = value.ToString();
                    }
                    fieldValueinfo.SetValue(item, fieldValue);
                }
            }
        }
        public object GetValue(string fieldName)
        {
            object fieldValue = null;
            foreach (BTTypeBaseComponent item in gameObject.GetComponents<BTTypeBaseComponent>())
            {
                if (fieldName == item.fieldName)
                {
                    FieldInfo fieldValueinfo = item.GetType().GetField("fieldValue");
                    fieldValue = fieldValueinfo.GetValue(item);
                }
            }
            return fieldValue;
        }
        public NodeProto ToNodeProto()
        {
            return  NodeConfigToNodeProto(this);
        }

        private static NodeProto NodeConfigToNodeProto(BehaviorNodeConfig nodeProto)
        {
            NodeProto nodeData = new NodeProto();
            nodeData.nodeId = nodeProto.id;
            nodeData.name = nodeProto.name;
            nodeData.describe = nodeProto.describe;
            nodeData.args_dict = nodeProto.GetArgsDict();
            nodeData.children = new List<NodeProto>();
            foreach (Transform child in nodeProto.gameObject.transform)
            {
                BehaviorNodeConfig nodeConfig = child.gameObject.GetComponent<BehaviorNodeConfig>();
                NodeProto childData = NodeConfigToNodeProto(nodeConfig);
                nodeData.children.Add(childData);
            }
            return nodeData;
        }


    }
}