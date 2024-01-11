using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace YIUIFramework
{
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class RedDotKeyData
    {
        [LabelText("Id")]
        [LabelWidth(50)]
        [MinValue(1)]
        [TableColumnWidth(150, resizable: false)]
        [ShowInInspector]
        [OdinSerialize]
        public int Id { get; internal set; } = 1;

        [LabelText("描述")]
        [LabelWidth(50)]
        [ShowInInspector]
        [OdinSerialize]
        public string Des { get; internal set; }

        private RedDotKeyData()
        {
        }

        public RedDotKeyData(int id)
        {
            Id = id;
        }

        public RedDotKeyData(int id, string des)
        {
            Id  = id;
            Des = des;
        }

        internal void ChangeDes(string des)
        {
            Des = des;
        }
    }
}