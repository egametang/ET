using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace YIUIFramework
{
    public class RedDotBind : MonoBehaviour
    {
        [SerializeField]
        [LabelText("文本")]
        private TextMeshProUGUI m_Text;

        [SerializeField]
        [LabelText("红点枚举")]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        private ERedDotKeyType m_Key;

        public ERedDotKeyType Key => m_Key;

        [ShowInInspector]
        [ReadOnly]
        [LabelText("显影")]
        public bool Show { get; private set; }

        [ShowInInspector]
        [ReadOnly]
        [LabelText("数量")]
        public int Count { get; private set; }

        private void Awake()
        {
            RedDotMgr.Inst?.AddChanged(Key, OnRedDotChangeHandler);
        }

        private void OnDestroy()
        {
            if (SingletonMgr.Disposing)
                return;
            RedDotMgr.Inst?.RemoveChanged(Key, OnRedDotChangeHandler);
        }

        private void OnRedDotChangeHandler(int count)
        {
            Show  = count >= 1;
            Count = count;
            Refresh();
        }

        private void Refresh()
        {
            gameObject.SetActive(Show);
            if (m_Text != null)
                m_Text.text = Count.ToString();
        }
    }
}