using System.Collections.Generic;

namespace ET.Client
{
    public partial class YIUIPanelComponent
    {
        private HashSet<string> m_ViewOpening = new HashSet<string>();

        private void AddOpening(string name)
        {
            m_ViewOpening.Add(name);
        }

        private void RemovOpening(string name)
        {
            m_ViewOpening.Remove(name);
        }

        public bool ViewIsOpening(string name)
        {
            return m_ViewOpening.Contains(name);
        }
    }
}