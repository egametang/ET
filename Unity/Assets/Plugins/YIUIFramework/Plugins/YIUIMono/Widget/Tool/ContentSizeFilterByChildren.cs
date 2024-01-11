using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YIUIFramework
{
    public class ContentSizeFilterByChildren: UIBehaviour, ILayoutElement, ILayoutSelfController, ILayoutGroup
    {
        public enum FitMode
        {
            Width,
            Height,
            Both
        }

        public enum SizeMode
        {
            Add,
            Max
        }

        [SerializeField]
        protected SizeMode m_Size = SizeMode.Add;

        [SerializeField]
        protected FitMode m_Fit = FitMode.Height;

        public FitMode fit
        {
            get
            {
                return m_Fit;
            }
            set
            {
                m_Fit = value;
            }
        }

        [SerializeField]
        protected float m_MaxSize = int.MaxValue;

        public float maxSize
        {
            get
            {
                return m_MaxSize;
            }
            set
            {
                m_MaxSize = value;
            }
        }

        [SerializeField]
        protected int m_layoutPriority = 1;

        public int layoutPriority
        {
            get
            {
                return m_layoutPriority;
            }
            set
            {
                m_layoutPriority = value;
            }
        }

        [SerializeField]
        protected float m_minWidth = -1;

        public float minWidth
        {
            get
            {
                return m_minWidth;
            }
            set
            {
                m_minWidth = value;
            }
        }

        [SerializeField]
        protected float m_minHeight = -1;

        public float minHeight
        {
            get
            {
                return m_minHeight;
            }
            set
            {
                m_minHeight = value;
            }
        }

        [SerializeField]
        protected float m_Space = 0;

        [System.NonSerialized]
        private RectTransform m_Rect;

        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private HorizontalOrVerticalLayoutGroup m_LayoutGroup = null;

        public void AutoSize()
        {
            if (m_Fit == FitMode.Width || m_Fit == FitMode.Both)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                    ((ILayoutElement)this).preferredWidth);
            if (m_Fit == FitMode.Height || m_Fit == FitMode.Both)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    ((ILayoutElement)this).preferredHeight);
        }

        void ILayoutController.SetLayoutHorizontal()
        {
            AutoSize();
        }

        void ILayoutController.SetLayoutVertical()
        {
            AutoSize();
        }

        float ILayoutElement.minWidth
        {
            get
            {
                return m_minWidth;
            }
        }

        float ILayoutElement.preferredWidth
        {
            get
            {
                float size = -1;
                float max  = -2;
                if (m_Fit == FitMode.Width || m_Fit == FitMode.Both)
                {
                    max  = 0;
                    size = 0;
                    float spacing = m_Space;
                    if (m_LayoutGroup != null)
                    {
                        size = size + m_LayoutGroup.padding.left + m_LayoutGroup.padding.right;
                        if (m_LayoutGroup is HorizontalLayoutGroup)
                            spacing = m_LayoutGroup.spacing;
                    }

                    int One = 1;
                    for (int i = 0; i < rectTransform.childCount; ++i)
                    {
                        var child = rectTransform.GetChild(i) as RectTransform;
                        if (child.gameObject.activeSelf)
                        {
                            var ignore = child.GetComponent<ILayoutIgnorer>();
                            if (ignore != null && ignore.ignoreLayout)
                                continue;

                            if (m_Size == SizeMode.Add)
                            {
                                size += Mathf.Max(0, LayoutUtility.GetPreferredWidth(child));

                                if (One == 0)
                                    size += spacing;
                                if (One == 1)
                                    One = 0;
                            }

                            //else if (m_Size == SizeMode.Max)
                            //{
                            //    size = size + Mathf.Max(size, LayoutUtility.GetPreferredWidth(child));
                            //    break;
                            //}
                            else if (m_Size == SizeMode.Max)
                            {
                                max = Mathf.Max(max, LayoutUtility.GetPreferredWidth(child) + size);
                            }
                        }
                    }

                    if (m_Size == SizeMode.Max)
                        size = max;

                    size = Mathf.Min(maxSize, size);
                    if (minWidth > 0)
                    {
                        size = Mathf.Max(minWidth, size);
                    }
                }

                return size;
            }
        }

        float ILayoutElement.flexibleWidth
        {
            get
            {
                return -1;
            }
        }

        float ILayoutElement.minHeight
        {
            get
            {
                return m_minHeight;
            }
        }

        float ILayoutElement.preferredHeight
        {
            get
            {
                float size = -1;
                float max  = 0;
                if (m_Fit == FitMode.Height || m_Fit == FitMode.Both)
                {
                    size = 0;
                    float spacing = m_Space;
                    if (m_LayoutGroup != null)
                    {
                        size = size + m_LayoutGroup.padding.top + m_LayoutGroup.padding.bottom;
                        if (m_LayoutGroup is VerticalLayoutGroup)
                            spacing = m_LayoutGroup.spacing;
                    }

                    int One = 1;

                    for (int i = 0; i < rectTransform.childCount; ++i)
                    {
                        var child = rectTransform.GetChild(i) as RectTransform;
                        if (child.gameObject.activeSelf)
                        {
                            var ignore = child.GetComponent<ILayoutIgnorer>();
                            if (ignore != null && ignore.ignoreLayout)
                                continue;

                            if (m_Size == SizeMode.Add)
                            {
                                size += Mathf.Max(0, LayoutUtility.GetPreferredHeight(child));

                                if (One == 0)
                                    size += spacing;
                                if (One == 1)
                                    One = 0;
                            }
                            else if (m_Size == SizeMode.Max)
                            {
                                max = Mathf.Max(max, LayoutUtility.GetPreferredHeight(child) + size);
                            }
                        }
                    }

                    if (m_Size == SizeMode.Max)
                        size = max;
                    size = Mathf.Min(maxSize, size);
                    if (minHeight > 0)
                    {
                        size = Mathf.Max(minHeight, size);
                    }
                }

                return size;
            }
        }

        float ILayoutElement.flexibleHeight
        {
            get
            {
                return -1;
            }
        }

        int ILayoutElement.layoutPriority
        {
            get
            {
                return m_layoutPriority;
            }
        }

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
            AutoSize();
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
            AutoSize();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected override void Awake()
        {
            m_LayoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
        }
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            OnEnable();
            m_LayoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        #endif
    }
}