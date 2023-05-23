using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// A component for making a child RectTransform scroll with reuseable content.
    /// </summary>
    /// <remarks>
    /// LoopScrollRect will not do any clipping on its own. Combined with a Mask component, it can be turned into a loop scroll view.
    /// </remarks>
    public abstract class LoopScrollRectBase : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        //==========LoopScrollRect==========
        public LoopScrollPrefabSourceInstance prefabSource = new  LoopScrollPrefabSourceInstance();

        /// <summary>
        /// The scroll's total count for items with id in [0, totalCount]. Negative value like -1 means infinite items.
        /// </summary>
        [Tooltip("Total count, negative means INFINITE mode")]
        public int totalCount;

        /// <summary>
        /// [Optional] Helper for accurate size so we can achieve better scrolling.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public LoopScrollSizeHelper sizeHelper = null;

        /// <summary>
        /// When threshold reached, we prepare new items outside view. This will be expanded to at least 1.5 * itemSize.
        /// </summary>
        protected float threshold = 0;

        /// <summary>
        /// Whether we use down-upsize or right-left direction?
        /// </summary>
        [Tooltip("Reverse direction for dragging")]
        public bool reverseDirection = false;

        /// <summary>
        /// The first item id in LoopScroll.
        /// </summary>
        protected int itemTypeStart = 0;

        /// <summary>
        /// The last item id in LoopScroll.
        /// </summary>
        protected int itemTypeEnd = 0;

        protected abstract float GetSize(RectTransform item, bool includeSpacing = true);
        protected abstract float GetDimension(Vector2 vector);
        protected abstract float GetAbsDimension(Vector2 vector);
        protected abstract Vector2 GetVector(float value);
        /// <summary>
        /// Direction for LoopScroll. This is a bit confusing with m_Horizontal/m_Vertical.
        /// </summary>
        protected enum LoopScrollRectDirection
        {
            Vertical,
            Horizontal,
        }
        protected LoopScrollRectDirection direction = LoopScrollRectDirection.Horizontal;

        private bool m_ContentSpaceInit = false;
        private float m_ContentSpacing = 0;
        protected float m_ContentLeftPadding = 0;
        protected float m_ContentRightPadding = 0;
        protected float m_ContentTopPadding = 0;
        protected float m_ContentBottomPadding = 0;
        protected GridLayoutGroup m_GridLayout = null;
        protected float contentSpacing
        {
            get
            {
                if (m_ContentSpaceInit)
                {
                    return m_ContentSpacing;
                }
                m_ContentSpaceInit = true;
                m_ContentSpacing = 0;
                if (m_Content != null)
                {
                    HorizontalOrVerticalLayoutGroup layout1 = m_Content.GetComponent<HorizontalOrVerticalLayoutGroup>();
                    if (layout1 != null)
                    {
                        m_ContentSpacing = layout1.spacing;
                        m_ContentLeftPadding = layout1.padding.left;
                        m_ContentRightPadding = layout1.padding.right;
                        m_ContentTopPadding = layout1.padding.top;
                        m_ContentBottomPadding = layout1.padding.bottom;
                    }
                    m_GridLayout = m_Content.GetComponent<GridLayoutGroup>();
                    if (m_GridLayout != null)
                    {
                        m_ContentSpacing = GetAbsDimension(m_GridLayout.spacing);
                        m_ContentLeftPadding = m_GridLayout.padding.left;
                        m_ContentRightPadding = m_GridLayout.padding.right;
                        m_ContentTopPadding = m_GridLayout.padding.top;
                        m_ContentBottomPadding = m_GridLayout.padding.bottom;
                    }
                }
                return m_ContentSpacing;
            }
        }

        private bool m_ContentConstraintCountInit = false;
        private int m_ContentConstraintCount = 0;
        protected int contentConstraintCount
        {
            get
            {
                if (m_ContentConstraintCountInit)
                {
                    return m_ContentConstraintCount;
                }
                m_ContentConstraintCountInit = true;
                m_ContentConstraintCount = 1;
                if (m_Content != null)
                {
                    GridLayoutGroup layout2 = m_Content.GetComponent<GridLayoutGroup>();
                    if (layout2 != null)
                    {
                        if (layout2.constraint == GridLayoutGroup.Constraint.Flexible)
                        {
                            Debug.LogWarning("[LoopScrollRect] Flexible not supported yet");
                        }
                        m_ContentConstraintCount = layout2.constraintCount;
                    }
                }
                return m_ContentConstraintCount;
            }
        }

        /// <summary>
        /// The first line in scroll. Grid may have multiply items in one line.
        /// </summary>
        protected int StartLine
        {
            get
            {
                return Mathf.CeilToInt((float)(itemTypeStart) / contentConstraintCount);
            }
        }
        
        /// <summary>
        /// Current line count in scroll. Grid may have multiply items in one line.
        /// </summary>
        protected int CurrentLines
        {
            get
            {
                return Mathf.CeilToInt((float)(itemTypeEnd - itemTypeStart) / contentConstraintCount);
            }
        }
        
        /// <summary>
        /// Total line count in scroll. Grid may have multiply items in one line.
        /// </summary>
        protected int TotalLines
        {
            get
            {
                return Mathf.CeilToInt((float)(totalCount) / contentConstraintCount);
            }
        }

        protected virtual bool UpdateItems(ref Bounds viewBounds, ref Bounds contentBounds) { return false; }
        //==========LoopScrollRect==========

        /// <summary>
        /// A setting for which behavior to use when content moves beyond the confines of its container.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     //Called when a button is pressed
        ///     public void Example(int option)
        ///     {
        ///         if (option == 0)
        ///         {
        ///             myScrollRect.movementType = ScrollRect.MovementType.Clamped;
        ///         }
        ///         else if (option == 1)
        ///         {
        ///             myScrollRect.movementType = ScrollRect.MovementType.Elastic;
        ///         }
        ///         else if (option == 2)
        ///         {
        ///             myScrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public enum MovementType
        {
            /// <summary>
            /// Unrestricted movement. The content can move forever.
            /// </summary>
            Unrestricted,

            /// <summary>
            /// Elastic movement. The content is allowed to temporarily move beyond the container, but is pulled back elastically.
            /// </summary>
            Elastic,

            /// <summary>
            /// Clamped movement. The content can not be moved beyond its container.
            /// </summary>
            Clamped,
        }

        /// <summary>
        /// Enum for which behavior to use for scrollbar visibility.
        /// </summary>
        public enum ScrollbarVisibility
        {
            /// <summary>
            /// Always show the scrollbar.
            /// </summary>
            Permanent,

            /// <summary>
            /// Automatically hide the scrollbar when no scrolling is needed on this axis. The viewport rect will not be changed.
            /// </summary>
            AutoHide,

            /// <summary>
            /// Automatically hide the scrollbar when no scrolling is needed on this axis, and expand the viewport rect accordingly.
            /// </summary>
            /// <remarks>
            /// When this setting is used, the scrollbar and the viewport rect become driven, meaning that values in the RectTransform are calculated automatically and can't be manually edited.
            /// </remarks>
            AutoHideAndExpandViewport,
        }

        [Serializable]
        /// <summary>
        /// Event type used by the ScrollRect.
        /// </summary>
        public class ScrollRectEvent : UnityEvent<Vector2> {}

        [SerializeField]
        protected RectTransform m_Content;	//==========LoopScrollRect==========

        /// <summary>
        /// The content that can be scrolled. It should be a child of the GameObject with ScrollRect on it.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public RectTransform scrollableContent;
        ///
        ///     //Do this when the Save button is selected.
        ///     public void Start()
        ///     {
        ///         // assigns the contect that can be scrolled using the ScrollRect.
        ///         myScrollRect.content = scrollableContent;
        ///     }
        /// }
        /// </code>
        /// </example>
        public RectTransform content { get { return m_Content; } set { m_Content = value; } }

        [SerializeField]
        private bool m_Horizontal = true;

        /// <summary>
        /// Should horizontal scrolling be enabled?
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         // Is horizontal scrolling enabled?
        ///         if (myScrollRect.horizontal == true)
        ///         {
        ///             Debug.Log("Horizontal Scrolling is Enabled!");
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool horizontal { get { return m_Horizontal; } set { m_Horizontal = value; } }

        [SerializeField]
        private bool m_Vertical = true;

        /// <summary>
        /// Should vertical scrolling be enabled?
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         // Is Vertical scrolling enabled?
        ///         if (myScrollRect.vertical == true)
        ///         {
        ///             Debug.Log("Vertical Scrolling is Enabled!");
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool vertical { get { return m_Vertical; } set { m_Vertical = value; } }

        [SerializeField]
        private MovementType m_MovementType = MovementType.Elastic;

        /// <summary>
        /// The behavior to use when the content moves beyond the scroll rect.
        /// </summary>
        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }

        [SerializeField]
        private float m_Elasticity = 0.1f;

        /// <summary>
        /// The amount of elasticity to use when the content moves beyond the scroll rect.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         // assigns a new value to the elasticity of the scroll rect.
        ///         // The higher the number the longer it takes to snap back.
        ///         myScrollRect.elasticity = 3.0f;
        ///     }
        /// }
        /// </code>
        /// </example>
        public float elasticity { get { return m_Elasticity; } set { m_Elasticity = value; } }

        [SerializeField]
        private bool m_Inertia = true;

        /// <summary>
        /// Should movement inertia be enabled?
        /// </summary>
        /// <remarks>
        /// Inertia means that the scrollrect content will keep scrolling for a while after being dragged. It gradually slows down according to the decelerationRate.
        /// </remarks>
        public bool inertia { get { return m_Inertia; } set { m_Inertia = value; } }

        [SerializeField]
        private float m_DecelerationRate = 0.135f; // Only used when inertia is enabled

        /// <summary>
        /// The rate at which movement slows down.
        /// </summary>
        /// <remarks>
        /// The deceleration rate is the speed reduction per second. A value of 0.5 halves the speed each second. The default is 0.135. The deceleration rate is only used when inertia is enabled.
        /// </remarks>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         // assigns a new value to the decelerationRate of the scroll rect.
        ///         // The higher the number the longer it takes to decelerate.
        ///         myScrollRect.decelerationRate = 5.0f;
        ///     }
        /// }
        /// </code>
        /// </example>
        public float decelerationRate { get { return m_DecelerationRate; } set { m_DecelerationRate = value; } }

        [SerializeField]
        private float m_ScrollSensitivity = 1.0f;

        /// <summary>
        /// The sensitivity to scroll wheel and track pad scroll events.
        /// </summary>
        /// <remarks>
        /// Higher values indicate higher sensitivity.
        /// </remarks>
        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }

        [SerializeField]
        private RectTransform m_Viewport;

        /// <summary>
        /// Reference to the viewport RectTransform that is the parent of the content RectTransform.
        /// </summary>
        public RectTransform viewport { get { return m_Viewport; } set { m_Viewport = value; SetDirtyCaching(); } }

        [SerializeField]
        private Scrollbar m_HorizontalScrollbar;

        /// <summary>
        /// Optional Scrollbar object linked to the horizontal scrolling of the ScrollRect.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     public void Start()
        ///     {
        ///         // Assigns a scroll bar element to the ScrollRect, allowing you to scroll in the horizontal axis.
        ///         myScrollRect.horizontalScrollbar = newScrollBar;
        ///     }
        /// }
        /// </code>
        /// </example>
        public Scrollbar horizontalScrollbar
        {
            get
            {
                return m_HorizontalScrollbar;
            }
            set
            {
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                m_HorizontalScrollbar = value;
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private Scrollbar m_VerticalScrollbar;

        /// <summary>
        /// Optional Scrollbar object linked to the vertical scrolling of the ScrollRect.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     public void Start()
        ///     {
        ///         // Assigns a scroll bar element to the ScrollRect, allowing you to scroll in the vertical axis.
        ///         myScrollRect.verticalScrollbar = newScrollBar;
        ///     }
        /// }
        /// </code>
        /// </example>
        public Scrollbar verticalScrollbar
        {
            get
            {
                return m_VerticalScrollbar;
            }
            set
            {
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                m_VerticalScrollbar = value;
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private ScrollbarVisibility m_HorizontalScrollbarVisibility;

        /// <summary>
        /// The mode of visibility for the horizontal scrollbar.
        /// </summary>
        public ScrollbarVisibility horizontalScrollbarVisibility { get { return m_HorizontalScrollbarVisibility; } set { m_HorizontalScrollbarVisibility = value; SetDirtyCaching(); } }

        [SerializeField]
        private ScrollbarVisibility m_VerticalScrollbarVisibility;

        /// <summary>
        /// The mode of visibility for the vertical scrollbar.
        /// </summary>
        public ScrollbarVisibility verticalScrollbarVisibility { get { return m_VerticalScrollbarVisibility; } set { m_VerticalScrollbarVisibility = value; SetDirtyCaching(); } }

        [SerializeField]
        private float m_HorizontalScrollbarSpacing;

        /// <summary>
        /// The space between the scrollbar and the viewport.
        /// </summary>
        public float horizontalScrollbarSpacing { get { return m_HorizontalScrollbarSpacing; } set { m_HorizontalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField]
        private float m_VerticalScrollbarSpacing;

        /// <summary>
        /// The space between the scrollbar and the viewport.
        /// </summary>
        public float verticalScrollbarSpacing { get { return m_VerticalScrollbarSpacing; } set { m_VerticalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField]
        private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();

        /// <summary>
        /// Callback executed when the position of the child changes.
        /// </summary>
        /// <remarks>
        /// onValueChanged is used to watch for changes in the ScrollRect object.
        /// The onValueChanged call will use the UnityEvent.AddListener API to watch for
        /// changes.  When changes happen script code provided by the user will be called.
        /// The UnityEvent.AddListener API for UI.ScrollRect._onValueChanged takes a Vector2.
        ///
        /// Note: The editor allows the onValueChanged value to be set up manually.For example the
        /// value can be set to run only a runtime.  The object and script function to call are also
        /// provided here.
        ///
        /// The onValueChanged variable can be alternatively set-up at runtime.The script example below
        /// shows how this can be done.The script is attached to the ScrollRect object.
        /// </remarks>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using UnityEngine.UI;
        ///
        /// public class ExampleScript : MonoBehaviour
        /// {
        ///     static ScrollRect scrollRect;
        ///
        ///     void Start()
        ///     {
        ///         scrollRect = GetComponent<ScrollRect>();
        ///         scrollRect.onValueChanged.AddListener(ListenerMethod);
        ///     }
        ///
        ///     public void ListenerMethod(Vector2 value)
        ///     {
        ///         Debug.Log("ListenerMethod: " + value);
        ///     }
        /// }
        /// </code>
        /// </example>
        public ScrollRectEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        // The offset from handle position to mouse down position
        private Vector2 m_PointerStartLocalCursor = Vector2.zero;
        protected Vector2 m_ContentStartPosition = Vector2.zero;

        private RectTransform m_ViewRect;

        protected RectTransform viewRect
        {
            get
            {
                if (m_ViewRect == null)
                    m_ViewRect = m_Viewport;
                if (m_ViewRect == null)
                    m_ViewRect = (RectTransform)transform;
                return m_ViewRect;
            }
        }

        protected Bounds m_ContentBounds;
        private Bounds m_ViewBounds;

        private Vector2 m_Velocity;

        /// <summary>
        /// The current velocity of the content.
        /// </summary>
        /// <remarks>
        /// The velocity is defined in units per second.
        /// </remarks>
        public Vector2 velocity { get { return m_Velocity; } set { m_Velocity = value; } }

        private bool m_Dragging;
        private bool m_Scrolling;

        private Vector2 m_PrevPosition = Vector2.zero;
        private Bounds m_PrevContentBounds;
        private Bounds m_PrevViewBounds;
        [NonSerialized]
        private bool m_HasRebuiltLayout = false;

        private bool m_HSliderExpand;
        private bool m_VSliderExpand;
        private float m_HSliderHeight;
        private float m_VSliderWidth;

        [System.NonSerialized] private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private RectTransform m_HorizontalScrollbarRect;
        private RectTransform m_VerticalScrollbarRect;

        private DrivenRectTransformTracker m_Tracker;

        protected LoopScrollRectBase()
        {}

        //==========LoopScrollRect==========
        #if UNITY_EDITOR
        protected override void Awake()
        {
            base.Awake();
            if (Application.isPlaying)
            {
                float value = (reverseDirection ^ (direction == LoopScrollRectDirection.Horizontal)) ? 0 : 1;
                if (m_Content != null)
                {
                    Debug.Assert(GetAbsDimension(m_Content.pivot) == value, this);
                    Debug.Assert(GetAbsDimension(m_Content.anchorMin) == value, this);
                    Debug.Assert(GetAbsDimension(m_Content.anchorMax) == value, this);
                }
                if (direction == LoopScrollRectDirection.Vertical)
                    Debug.Assert(m_Vertical && !m_Horizontal, this);
                else
                    Debug.Assert(!m_Vertical && m_Horizontal, this);
            }
        }
        #endif

        public void ClearCells()
        {
            if (Application.isPlaying)
            {
                itemTypeStart = 0;
                itemTypeEnd = 0;
                totalCount = 0;
                for (int i = m_Content.childCount - 1; i >= 0; i--)
                {
                    prefabSource.ReturnObject(m_Content.GetChild(i));
                }
            }
        }

        public int GetFirstItem(out float offset)
        {
            if (direction == LoopScrollRectDirection.Vertical)
                offset = m_ViewBounds.max.y - m_ContentBounds.max.y;
            else
                offset = m_ContentBounds.min.x - m_ViewBounds.min.x;
            int idx = 0;
            if (itemTypeEnd > itemTypeStart)
            {
                float size = GetSize(m_Content.GetChild(0) as RectTransform, false);
                while (size + offset <= 0 && itemTypeStart + idx + contentConstraintCount < itemTypeEnd)
                {
                    offset += size;
                    idx += contentConstraintCount;
                    size = GetSize(m_Content.GetChild(idx) as RectTransform);
                }
            }
            return idx + itemTypeStart;
        }
        
        public int GetLastItem(out float offset)
        {
            if (direction == LoopScrollRectDirection.Vertical)
                offset = m_ContentBounds.min.y - m_ViewBounds.min.y;
            else
                offset = m_ViewBounds.max.x - m_ContentBounds.max.x;
            int idx = 0;
            if (itemTypeEnd > itemTypeStart)
            {
                int totalChildCount = m_Content.childCount;
                float size = GetSize(m_Content.GetChild(totalChildCount - idx - 1) as RectTransform, false);
                while (size + offset <= 0 && itemTypeStart < itemTypeEnd - idx - contentConstraintCount)
                {
                    offset += size;
                    idx += contentConstraintCount;
                    size = GetSize(m_Content.GetChild(totalChildCount - idx - 1) as RectTransform);
                }
            }
            offset = -offset;
            return itemTypeEnd - idx - 1;
        }
        
        public void SrollToCell(int index, float speed)
        {
            if (totalCount >= 0 && (index < 0 || index >= totalCount))
            {
                Debug.LogErrorFormat("invalid index {0}", index);
                return;
            }
            StopAllCoroutines();
            if (speed <= 0)
            {
                RefillCells(index);
                return;
            }
            StartCoroutine(ScrollToCellCoroutine(index, speed));
        }
        
        public void SrollToCellWithinTime(int index, float time)
        {
            if (totalCount >= 0 && (index < 0 || index >= totalCount))
            {
                Debug.LogErrorFormat("invalid index {0}", index);
                return;
            }
            StopAllCoroutines();
            if (time <= 0)
            {
                RefillCells(index);
                return;
            }
            float dist = 0;
            float offset = 0;
            int currentFirst = reverseDirection ? GetLastItem(out offset) : GetFirstItem(out offset);

            int TargetLine = (index / contentConstraintCount);
            int CurrentLine = (currentFirst / contentConstraintCount);

            if (TargetLine == CurrentLine)
            {
                dist = offset;
            }
            else
            {
                if (sizeHelper != null)
                {
                    dist = GetDimension(sizeHelper.GetItemsSize(currentFirst) - sizeHelper.GetItemsSize(index)) + contentSpacing * (CurrentLine - TargetLine - 1);
                    dist += offset;
                }
                else
                {
                    float elementSize = (GetAbsDimension(m_ContentBounds.size) - contentSpacing * (CurrentLines - 1)) / CurrentLines;
                    dist = elementSize * (CurrentLine - TargetLine) + contentSpacing * (CurrentLine - TargetLine - 1);
                    dist -= offset;
                }
            }
            StartCoroutine(ScrollToCellCoroutine(index, Mathf.Abs(dist) / time));
        }

        IEnumerator ScrollToCellCoroutine(int index, float speed)
        {
            bool needMoving = true;
            while (needMoving)
            {
                yield return null;
                if (!m_Dragging)
                {
                    float move = 0;
                    if (index < itemTypeStart)
                    {
                        move = -Time.deltaTime * speed;
                    }
                    else if (index >= itemTypeEnd)
                    {
                        move = Time.deltaTime * speed;
                    }
                    else
                    {
                        m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                        var m_ItemBounds = GetBounds4Item(index);
                        var offset = 0.0f;
                        if (direction == LoopScrollRectDirection.Vertical)
                            offset = reverseDirection ? (m_ViewBounds.min.y - m_ItemBounds.min.y) : (m_ViewBounds.max.y - m_ItemBounds.max.y);
                        else
                            offset = reverseDirection ? (m_ItemBounds.max.x - m_ViewBounds.max.x) : (m_ItemBounds.min.x - m_ViewBounds.min.x);
                        // check if we cannot move on
                        if (totalCount >= 0)
                        {
                            if (offset > 0 && itemTypeEnd == totalCount && !reverseDirection)
                            {
                                m_ItemBounds = GetBounds4Item(totalCount - 1);
                                // reach bottom
                                if ((direction == LoopScrollRectDirection.Vertical && m_ItemBounds.min.y > m_ViewBounds.min.y) ||
                                    (direction == LoopScrollRectDirection.Horizontal && m_ItemBounds.max.x < m_ViewBounds.max.x))
                                {
                                    needMoving = false;
                                    break;
                                }
                            }
                            else if (offset < 0 && itemTypeStart == 0 && reverseDirection)
                            {
                                m_ItemBounds = GetBounds4Item(0);
                                if ((direction == LoopScrollRectDirection.Vertical && m_ItemBounds.max.y < m_ViewBounds.max.y) ||
                                    (direction == LoopScrollRectDirection.Horizontal && m_ItemBounds.min.x > m_ViewBounds.min.x))
                                {
                                    needMoving = false;
                                    break;
                                }
                            }
                        }

                        float maxMove = Time.deltaTime * speed;
                        if (Mathf.Abs(offset) < maxMove)
                        {
                            needMoving = false;
                            move = offset;
                        }
                        else
                            move = Mathf.Sign(offset) * maxMove;
                    }
                    if (move != 0)
                    {
                        Vector2 offset = GetVector(move);
                        m_Content.anchoredPosition += offset;
                        m_PrevPosition += offset;
                        m_ContentStartPosition += offset;
                        UpdateBounds(true);
                    }
                }
            }
            StopMovement();
            UpdatePrevData();
        }

        protected abstract void ProvideData(Transform transform, int index);

        /// <summary>
        /// Refresh item data
        /// </summary>
        public void RefreshCells()
        {
            if (Application.isPlaying && this.isActiveAndEnabled)
            {
                itemTypeEnd = itemTypeStart;
                // recycle items if we can
                for (int i = 0; i < m_Content.childCount; i++)
                {
                    if (itemTypeEnd < totalCount)
                    {
                        ProvideData(m_Content.GetChild(i), itemTypeEnd);
                        itemTypeEnd++;
                    }
                    else
                    {
                        prefabSource.ReturnObject(m_Content.GetChild(i));
                        i--;
                    }
                }
                UpdateBounds(true);
                UpdateScrollbars(Vector2.zero);
            }
        }

        /// <summary>
        /// Refill cells from endItem at the end while clear existing ones
        /// </summary>
        public void RefillCellsFromEnd(int endItem = 0, bool alignStart = false)
        {
            if (!Application.isPlaying)
                return;

            itemTypeEnd = reverseDirection ? endItem : totalCount - endItem;
            itemTypeStart = itemTypeEnd;

            if (totalCount >= 0 && itemTypeStart % contentConstraintCount != 0)
            {
                itemTypeStart = (itemTypeStart / contentConstraintCount) * contentConstraintCount;
            }

            ReturnToTempPool(!reverseDirection, m_Content.childCount);

            float sizeToFill = GetAbsDimension(viewRect.rect.size), sizeFilled = 0;

            bool first = true;
            while (sizeToFill > sizeFilled)
            {
                float size = reverseDirection ? NewItemAtEnd(!first) : NewItemAtStart(!first);
                if (size < 0)
                    break;
                first = false;
                sizeFilled += size;
            }

            // refill from start in case not full yet
            while (sizeToFill > sizeFilled)
            {
                float size = reverseDirection ? NewItemAtStart(!first) : NewItemAtEnd(!first);
                if (size < 0)
                    break;
                first = false;
                sizeFilled += size;
            }

            Vector2 pos = m_Content.anchoredPosition;
            float dist = alignStart ? 0 : Mathf.Max(0, sizeFilled - sizeToFill);
            if (reverseDirection)
                dist = -dist;
            if (direction == LoopScrollRectDirection.Vertical)
                pos.y = dist;
            else
                pos.x = -dist;
            m_Content.anchoredPosition = pos;
            m_ContentStartPosition = pos;

            ClearTempPool();
            // force build bounds here so scrollbar can access newest bounds
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_Content);
            Canvas.ForceUpdateCanvases();
            UpdateBounds(false);
            UpdateScrollbars(Vector2.zero);
            StopMovement();
            UpdatePrevData();
        }

        /// <summary>
        /// Refill cells with startItem at the beginning while clear existing ones
        /// </summary>
        /// <param name="startItem">The first item to fill</param>
        /// <param name="fillViewRect">When [startItem, totalCount] is not enough for the whole viewBound, should we fill backwords with [0, startItem)? </param>
        /// <param name="contentOffset">The first item's offset compared to viewBound</param>
        public void RefillCells(int startItem = 0, bool fillViewRect = false, float contentOffset = 0)
        {
            if (!Application.isPlaying)
                return;

            itemTypeStart = reverseDirection ? totalCount - startItem : startItem;
            if (totalCount >= 0 && itemTypeStart % contentConstraintCount != 0)
            {
                itemTypeStart = (itemTypeStart / contentConstraintCount) * contentConstraintCount;
            }
            itemTypeEnd = itemTypeStart;

            // Don't `Canvas.ForceUpdateCanvases();` here, or it will new/delete cells to change itemTypeStart/End
            ReturnToTempPool(reverseDirection, m_Content.childCount);

            float sizeToFill = GetAbsDimension(viewRect.rect.size) + Mathf.Abs(contentOffset);
            float sizeFilled = 0;
            // m_ViewBounds may be not ready when RefillCells on Start

            float itemSize = 0;

            bool first = true;
            while (sizeToFill > sizeFilled)
            {
                float size = reverseDirection ? NewItemAtStart(!first) : NewItemAtEnd(!first);
                if (size < 0)
                    break;
                first = false;
                itemSize = size;
                sizeFilled += size;
            }

            // refill from start in case not full yet
            while (sizeToFill > sizeFilled)
            {
                float size = reverseDirection ? NewItemAtEnd(!first) : NewItemAtStart(!first);
                if (size < 0)
                    break;
                first = false;
                sizeFilled += size;
            }

            if (fillViewRect && itemSize > 0 && sizeFilled < sizeToFill)
            {
                //calculate how many items can be added above the offset, so it still is visible in the view
                int itemsToAddCount = (int)((sizeToFill - sizeFilled) / itemSize);
                int newOffset = startItem - itemsToAddCount;
                if (newOffset < 0) newOffset = 0;
                if (newOffset != startItem)
                {
                    //refill again, with the new offset value, and now with fillViewRect disabled.
                    RefillCells(newOffset);
                }
            }

            Vector2 pos = m_Content.anchoredPosition;
            if (direction == LoopScrollRectDirection.Vertical)
                pos.y = -contentOffset;
            else
                pos.x = contentOffset;
            m_Content.anchoredPosition = pos;
            m_ContentStartPosition = pos;

            ClearTempPool();
            // force build bounds here so scrollbar can access newest bounds
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_Content);
            Canvas.ForceUpdateCanvases();
            UpdateBounds(false);
            UpdateScrollbars(Vector2.zero);
            StopMovement();
            UpdatePrevData();
        }

        protected float NewItemAtStart(bool includeSpacing = true)
        {
            if (totalCount >= 0 && itemTypeStart - contentConstraintCount < 0)
            {
                return -1;
            }
            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                itemTypeStart--;
                RectTransform newItem = GetFromTempPool(itemTypeStart);
                newItem.SetSiblingIndex(deletedItemTypeStart);
                size = Mathf.Max(GetSize(newItem, includeSpacing), size);
            }
            threshold = Mathf.Max(threshold, size * 1.5f);

            if (size > 0)
            {
                m_HasRebuiltLayout = false;
                if (!reverseDirection)
                {
                    Vector2 offset = GetVector(size);
                    m_Content.anchoredPosition += offset;
                    m_PrevPosition += offset;
                    m_ContentStartPosition += offset;
                }
            }

            return size;
        }

        protected float DeleteItemAtStart()
        {
            // special case: when moving or dragging, we cannot simply delete start when we've reached the end
            if ((m_Dragging || m_Velocity != Vector2.zero) && totalCount >= 0 && itemTypeEnd >= totalCount - contentConstraintCount)
            {
                return 0;
            }
            int availableChilds = m_Content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
            Debug.Assert(availableChilds >= 0);
            if (availableChilds == 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = m_Content.GetChild(deletedItemTypeStart) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                ReturnToTempPool(true);
                availableChilds--;
                itemTypeStart++;

                if (availableChilds == 0)
                {
                    break;
                }
            }

            if (size > 0)
            {
                m_HasRebuiltLayout = false;
                if (!reverseDirection)
                {
                    Vector2 offset = GetVector(size);
                    m_Content.anchoredPosition -= offset;
                    m_PrevPosition -= offset;
                    m_ContentStartPosition -= offset;
                }
            }

            return size;
        }


        protected float NewItemAtEnd(bool includeSpacing = true)
        {
            if (totalCount >= 0 && itemTypeEnd >= totalCount)
            {
                return -1;
            }
            float size = 0;
            // issue 4: fill lines to end first
            int availableChilds = m_Content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
            int count = contentConstraintCount - (availableChilds % contentConstraintCount);
            for (int i = 0; i < count; i++)
            {
                RectTransform newItem = GetFromTempPool(itemTypeEnd);
                newItem.SetSiblingIndex(m_Content.childCount - deletedItemTypeEnd - 1);
                size = Mathf.Max(GetSize(newItem, includeSpacing), size);
                itemTypeEnd++;
                if (totalCount >= 0 && itemTypeEnd >= totalCount)
                {
                    break;
                }
            }
            threshold = Mathf.Max(threshold, size * 1.5f);

            if (size > 0)
            {
                m_HasRebuiltLayout = false;
                if (reverseDirection)
                {
                    Vector2 offset = GetVector(size);
                    m_Content.anchoredPosition -= offset;
                    m_PrevPosition -= offset;
                    m_ContentStartPosition -= offset;
                }
            }

            return size;
        }

        protected float DeleteItemAtEnd()
        {
            if ((m_Dragging || m_Velocity != Vector2.zero) && totalCount >= 0 && itemTypeStart < contentConstraintCount)
            {
                return 0;
            }
            int availableChilds = m_Content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
            Debug.Assert(availableChilds >= 0);
            if (availableChilds == 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = m_Content.GetChild(m_Content.childCount - deletedItemTypeEnd - 1) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                ReturnToTempPool(false);
                availableChilds--;
                itemTypeEnd--;
                if (itemTypeEnd % contentConstraintCount == 0 || availableChilds == 0)
                {
                    break;  //just delete the whole row
                }
            }

            if (size > 0)
            {
                m_HasRebuiltLayout = false;
                if (reverseDirection)
                {
                    Vector2 offset = GetVector(size);
                    m_Content.anchoredPosition += offset;
                    m_PrevPosition += offset;
                    m_ContentStartPosition += offset;
                }
            }

            return size;
        }

        protected int deletedItemTypeStart = 0;
        protected int deletedItemTypeEnd = 0;
        protected abstract RectTransform GetFromTempPool(int itemIdx);
        protected abstract void ReturnToTempPool(bool fromStart, int count = 1);
        protected abstract void ClearTempPool();
        //==========LoopScrollRect==========

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                m_HasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        {}

        public virtual void GraphicUpdateComplete()
        {}

        void UpdateCachedData()
        {
            Transform transform = this.transform;
            m_HorizontalScrollbarRect = m_HorizontalScrollbar == null ? null : m_HorizontalScrollbar.transform as RectTransform;
            m_VerticalScrollbarRect = m_VerticalScrollbar == null ? null : m_VerticalScrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            bool viewIsChild = (viewRect.parent == transform);
            bool hScrollbarIsChild = (!m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform);
            bool vScrollbarIsChild = (!m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform);
            bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

            m_HSliderExpand = allAreChildren && m_HorizontalScrollbarRect && horizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_VSliderExpand = allAreChildren && m_VerticalScrollbarRect && verticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_HSliderHeight = (m_HorizontalScrollbarRect == null ? 0 : m_HorizontalScrollbarRect.rect.height);
            m_VSliderWidth = (m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            SetDirty();
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            m_Dragging = false;
            m_Scrolling = false;
            m_HasRebuiltLayout = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        /// <summary>
        /// See member in base class.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         //Checks if the ScrollRect called "myScrollRect" is active.
        ///         if (myScrollRect.IsActive())
        ///         {
        ///             Debug.Log("The Scroll Rect is active!");
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public override bool IsActive()
        {
            return base.IsActive() && m_Content != null;
        }

        private void EnsureLayoutHasRebuilt()
        {
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Sets the velocity to zero on both axes so the content stops moving.
        /// </summary>
        public virtual void StopMovement()
        {
            m_Velocity = Vector2.zero;
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (vertical && !horizontal)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    delta.y = delta.x;
                delta.x = 0;
            }
            if (horizontal && !vertical)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    delta.x = delta.y;
                delta.y = 0;
            }

            if (data.IsScrolling())
                m_Scrolling = true;

            Vector2 position = m_Content.anchoredPosition;
            position += delta * m_ScrollSensitivity;
            if (m_MovementType == MovementType.Clamped)
                position += CalculateOffset(position - m_Content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Velocity = Vector2.zero;
        }

        /// <summary>
        /// Handling for when the content is beging being dragged.
        /// </summary>
        ///<example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.EventSystems; // Required when using event data
        ///
        /// public class ExampleClass : MonoBehaviour, IBeginDragHandler // required interface when using the OnBeginDrag method.
        /// {
        ///     //Do this when the user starts dragging the element this script is attached to..
        ///     public void OnBeginDrag(PointerEventData data)
        ///     {
        ///         Debug.Log("They started dragging " + this.name);
        ///     }
        /// }
        /// </code>
        /// </example>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            m_PointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
            m_ContentStartPosition = m_Content.anchoredPosition;
            m_Dragging = true;
        }

        /// <summary>
        /// Handling for when the content has finished being dragged.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.EventSystems; // Required when using event data
        ///
        /// public class ExampleClass : MonoBehaviour, IEndDragHandler // required interface when using the OnEndDrag method.
        /// {
        ///     //Do this when the user stops dragging this UI Element.
        ///     public void OnEndDrag(PointerEventData data)
        ///     {
        ///         Debug.Log("Stopped dragging " + this.name + "!");
        ///     }
        /// }
        /// </code>
        /// </example>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Dragging = false;
        }

        /// <summary>
        /// Handling for when the content is dragged.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.EventSystems; // Required when using event data
        ///
        /// public class ExampleClass : MonoBehaviour, IDragHandler // required interface when using the OnDrag method.
        /// {
        ///     //Do this while the user is dragging this UI Element.
        ///     public void OnDrag(PointerEventData data)
        ///     {
        ///         Debug.Log("Currently dragging " + this.name);
        ///     }
        /// }
        /// </code>
        /// </example>
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!m_Dragging)
                return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            UpdateBounds();

            var pointerDelta = localCursor - m_PointerStartLocalCursor;
            Vector2 position = m_ContentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
            position += offset;
            if (m_MovementType == MovementType.Elastic)
            {
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
            }

            SetContentAnchoredPosition(position);
        }

        /// <summary>
        /// Sets the anchored position of the content.
        /// </summary>
        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (!m_Horizontal)
                position.x = m_Content.anchoredPosition.x;
            if (!m_Vertical)
                position.y = m_Content.anchoredPosition.y;

            //==========LoopScrollRect==========
            if ((position - m_Content.anchoredPosition).sqrMagnitude > 0.001f)
            {
                m_Content.anchoredPosition = position;
                UpdateBounds(true);
            }
            //==========LoopScrollRect==========
        }

        protected virtual void LateUpdate()
        {
            if (!m_Content)
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();
            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);
            if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
            {
                Vector2 position = m_Content.anchoredPosition;
                for (int axis = 0; axis < 2; axis++)
                {
                    // Apply spring physics if movement is elastic and content has an offset from the view.
                    if (m_MovementType == MovementType.Elastic && offset[axis] != 0)
                    {
                        float speed = m_Velocity[axis];
                        float smoothTime = m_Elasticity;
                        if (m_Scrolling)
                            smoothTime *= 3.0f;
                        position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis], ref speed, smoothTime, Mathf.Infinity, deltaTime);
                        if (Mathf.Abs(speed) < 1)
                            speed = 0;
                        m_Velocity[axis] = speed;
                    }
                    // Else move content according to velocity with deceleration applied.
                    else if (m_Inertia)
                    {
                        m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
                        if (Mathf.Abs(m_Velocity[axis]) < 1)
                            m_Velocity[axis] = 0;
                        position[axis] += m_Velocity[axis] * deltaTime;
                    }
                    // If we have neither elaticity or friction, there shouldn't be any velocity.
                    else
                    {
                        m_Velocity[axis] = 0;
                    }
                }

                if (m_MovementType == MovementType.Clamped)
                {
                    offset = CalculateOffset(position - m_Content.anchoredPosition);
                    position += offset;
                }

                SetContentAnchoredPosition(position);
            }

            if (m_Dragging && m_Inertia)
            {
                Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
            }

            if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
            {
                UpdateScrollbars(offset);
                #if UNITY_2017_1_OR_NEWER
                UISystemProfilerApi.AddMarker("ScrollRect.value", this);
                #endif
                m_OnValueChanged.Invoke(normalizedPosition);
                UpdatePrevData();
            }
            UpdateScrollbarVisibility();
            m_Scrolling = false;
        }

        /// <summary>
        /// Helper function to update the previous data fields on a ScrollRect. Call this before you change data in the ScrollRect.
        /// </summary>
        protected void UpdatePrevData()
        {
            if (m_Content == null)
                m_PrevPosition = Vector2.zero;
            else
                m_PrevPosition = m_Content.anchoredPosition;
            m_PrevViewBounds = m_ViewBounds;
            m_PrevContentBounds = m_ContentBounds;
        }

        //==========LoopScrollRect==========
        public void GetHorizonalOffsetAndSize(out float totalSize, out float offset)
        {
            if (sizeHelper != null)
            {
                totalSize = sizeHelper.GetItemsSize(TotalLines).x + contentSpacing * (TotalLines - 1);
                offset = m_ContentBounds.min.x - sizeHelper.GetItemsSize(StartLine).x - contentSpacing * StartLine;
            }
            else
            {
                float elementSize = (m_ContentBounds.size.x - contentSpacing * (CurrentLines - 1)) / CurrentLines;
                totalSize = elementSize * TotalLines + contentSpacing * (TotalLines - 1);
                offset = m_ContentBounds.min.x - elementSize * StartLine - contentSpacing * StartLine;
            }
        }
        
        public void GetVerticalOffsetAndSize(out float totalSize, out float offset)
        {
            if (sizeHelper != null)
            {
                totalSize = sizeHelper.GetItemsSize(TotalLines).y + contentSpacing * (TotalLines - 1);
                offset = m_ContentBounds.max.y + sizeHelper.GetItemsSize(StartLine).y + contentSpacing * StartLine;
            }
            else
            {
                float elementSize = (m_ContentBounds.size.y - contentSpacing * (CurrentLines - 1)) / CurrentLines;
                totalSize = elementSize * TotalLines + contentSpacing * (TotalLines - 1);
                offset = m_ContentBounds.max.y + elementSize * StartLine + contentSpacing * StartLine;
            }
        }
        //==========LoopScrollRect==========

        private void UpdateScrollbars(Vector2 offset)
        {
            if (m_HorizontalScrollbar)
            {
                //==========LoopScrollRect==========
                if (m_ContentBounds.size.x > 0 && totalCount > 0)
                {
                    float totalSize, _;
                    GetHorizonalOffsetAndSize(out totalSize, out _);
                    m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / totalSize);
                }
                //==========LoopScrollRect==========
                else
                    m_HorizontalScrollbar.size = 1;

                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }

            if (m_VerticalScrollbar)
            {
                //==========LoopScrollRect==========
                if (m_ContentBounds.size.y > 0 && totalCount > 0)
                {
                    float totalSize, _;
                    GetVerticalOffsetAndSize(out totalSize, out _);
                    m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / totalSize);
                }
                //==========LoopScrollRect==========
                else
                    m_VerticalScrollbar.size = 1;

                m_VerticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        /// <summary>
        /// The scroll position as a Vector2 between (0,0) and (1,1) with (0,0) being the lower left corner.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Vector2 myPosition = new Vector2(0.5f, 0.5f);
        ///
        ///     public void Start()
        ///     {
        ///         //Change the current scroll position.
        ///         myScrollRect.normalizedPosition = myPosition;
        ///     }
        /// }
        /// </code>
        /// </example>
        public Vector2 normalizedPosition
        {
            get
            {
                return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
            }
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        /// <summary>
        /// The horizontal scroll position as a value between 0 and 1, with 0 being at the left.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     public void Start()
        ///     {
        ///         //Change the current horizontal scroll position.
        ///         myScrollRect.horizontalNormalizedPosition = 0.5f;
        ///     }
        /// }
        /// </code>
        /// </example>
        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                //==========LoopScrollRect==========
                if (totalCount > 0 && itemTypeEnd > itemTypeStart)
                {
                    float totalSize, offset;
                    GetHorizonalOffsetAndSize(out totalSize, out offset);

                    if (totalSize <= m_ViewBounds.size.x)
                        return (m_ViewBounds.min.x > offset) ? 1 : 0;
                    return (m_ViewBounds.min.x - offset) / (totalSize - m_ViewBounds.size.x);
                }
                else
                    return 0.5f;
                //==========LoopScrollRect==========
            }
            set
            {
                SetNormalizedPosition(value, 0);
            }
        }

        /// <summary>
        /// The vertical scroll position as a value between 0 and 1, with 0 being at the bottom.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     public void Start()
        ///     {
        ///         //Change the current vertical scroll position.
        ///         myScrollRect.verticalNormalizedPosition = 0.5f;
        ///     }
        /// }
        /// </code>
        /// </example>

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                //==========LoopScrollRect==========
                if (totalCount > 0 && itemTypeEnd > itemTypeStart)
                {
                    float totalSize, offset;
                    GetVerticalOffsetAndSize(out totalSize, out offset);

                    if (totalSize <= m_ViewBounds.size.y)
                        return (offset > m_ViewBounds.max.y) ? 1 : 0;
                    return (offset - m_ViewBounds.max.y) / (totalSize - m_ViewBounds.size.y);
                }
                else
                    return 0.5f;
                //==========LoopScrollRect==========
            }
            set
            {
                SetNormalizedPosition(value, 1);
            }
        }

        private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
        private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

        /// <summary>
        /// >Set the horizontal or vertical scroll position as a value between 0 and 1, with 0 being at the left or at the bottom.
        /// </summary>
        /// <param name="value">The position to set, between 0 and 1.</param>
        /// <param name="axis">The axis to set: 0 for horizontal, 1 for vertical.</param>
        protected virtual void SetNormalizedPosition(float value, int axis)
        {
            //==========LoopScrollRect==========
            if (totalCount <= 0 || itemTypeEnd <= itemTypeStart)
                return;
            //==========LoopScrollRect==========

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            //==========LoopScrollRect==========
            float totalSize, offset;
            float newAnchoredPosition = m_Content.anchoredPosition[axis];
            if (axis == 0)
            {
                GetHorizonalOffsetAndSize(out totalSize, out offset);

                if (totalSize >= m_ViewBounds.size.x)
                {
                    newAnchoredPosition += m_ViewBounds.min.x - value * (totalSize - m_ViewBounds.size.x) - offset;
                }
            }
            else
            {
                GetVerticalOffsetAndSize(out totalSize, out offset);
                
                if (totalSize >= m_ViewBounds.size.y)
                {
                    newAnchoredPosition -= offset - value * (totalSize - m_ViewBounds.size.y) - m_ViewBounds.max.y;
                }
            }
            //==========LoopScrollRect==========
            
            Vector3 anchoredPosition = m_Content.anchoredPosition;
            if (Mathf.Abs(anchoredPosition[axis] - newAnchoredPosition) > 0.01f)
            {
                anchoredPosition[axis] = newAnchoredPosition;
                m_Content.anchoredPosition = anchoredPosition;
                m_Velocity[axis] = 0;
                UpdateBounds(true);	//==========LoopScrollRect==========
            }
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private bool hScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
                return true;
            }
        }
        private bool vScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
                return true;
            }
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal() {}

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputVertical() {}

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float minWidth { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float preferredWidth { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float flexibleWidth { get { return -1; } }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float minHeight { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float preferredHeight { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float flexibleHeight { get { return -1; } }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual int layoutPriority { get { return -1; } }

        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();

            if (m_HSliderExpand || m_VSliderExpand)
            {
                m_Tracker.Add(this, viewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                viewRect.anchorMin = Vector2.zero;
                viewRect.anchorMax = Vector2.one;
                viewRect.sizeDelta = Vector2.zero;
                viewRect.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_Content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);

                // Recalculate content layout with this size to see if it fits vertically
                // when there is a vertical scrollbar (which may reflowed the content to make it taller).
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_Content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
            if (m_HSliderExpand && hScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(viewRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If the vertical slider didn't kick in the first time, and the horizontal one did,
            // we need to check again if the vertical slider now needs to kick in.
            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded && viewRect.sizeDelta.x == 0 && viewRect.sizeDelta.y < 0)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);
            }
        }

        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();
        }

        void UpdateScrollbarVisibility()
        {
            UpdateOneScrollbarVisibility(vScrollingNeeded, m_Vertical, m_VerticalScrollbarVisibility, m_VerticalScrollbar);
            UpdateOneScrollbarVisibility(hScrollingNeeded, m_Horizontal, m_HorizontalScrollbarVisibility, m_HorizontalScrollbar);
        }

        private static void UpdateOneScrollbarVisibility(bool xScrollingNeeded, bool xAxisEnabled, ScrollbarVisibility scrollbarVisibility, Scrollbar scrollbar)
        {
            if (scrollbar)
            {
                if (scrollbarVisibility == ScrollbarVisibility.Permanent)
                {
                    if (scrollbar.gameObject.activeSelf != xAxisEnabled)
                        scrollbar.gameObject.SetActive(xAxisEnabled);
                }
                else
                {
                    if (scrollbar.gameObject.activeSelf != xScrollingNeeded)
                        scrollbar.gameObject.SetActive(xScrollingNeeded);
                }
            }
        }

        void UpdateScrollbarLayout()
        {
            if (m_VSliderExpand && m_HorizontalScrollbar)
            {
                m_Tracker.Add(this, m_HorizontalScrollbarRect,
                    DrivenTransformProperties.AnchorMinX |
                    DrivenTransformProperties.AnchorMaxX |
                    DrivenTransformProperties.SizeDeltaX |
                    DrivenTransformProperties.AnchoredPositionX);
                m_HorizontalScrollbarRect.anchorMin = new Vector2(0, m_HorizontalScrollbarRect.anchorMin.y);
                m_HorizontalScrollbarRect.anchorMax = new Vector2(1, m_HorizontalScrollbarRect.anchorMax.y);
                m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0, m_HorizontalScrollbarRect.anchoredPosition.y);
                if (vScrollingNeeded)
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), m_HorizontalScrollbarRect.sizeDelta.y);
                else
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(0, m_HorizontalScrollbarRect.sizeDelta.y);
            }

            if (m_HSliderExpand && m_VerticalScrollbar)
            {
                m_Tracker.Add(this, m_VerticalScrollbarRect,
                    DrivenTransformProperties.AnchorMinY |
                    DrivenTransformProperties.AnchorMaxY |
                    DrivenTransformProperties.SizeDeltaY |
                    DrivenTransformProperties.AnchoredPositionY);
                m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0);
                m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1);
                m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0);
                if (hScrollingNeeded)
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                else
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0);
            }
        }

        /// <summary>
        /// Calculate the bounds the ScrollRect should be using.
        /// </summary>
        protected void UpdateBounds(bool updateItems = false) //==========LoopScrollRect==========
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();

            if (m_Content == null)
                return;

            // ============LoopScrollRect============
            // Don't do this in Rebuild. Make use of ContentBounds before Adjust here.
            if (Application.isPlaying && updateItems && UpdateItems(ref m_ViewBounds, ref m_ContentBounds))
            {
                EnsureLayoutHasRebuilt();
                m_ContentBounds = GetBounds();
            }
            // ============LoopScrollRect============
            
            Vector3 contentSize = m_ContentBounds.size;
            Vector3 contentPos = m_ContentBounds.center;
            var contentPivot = m_Content.pivot;
            AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
            m_ContentBounds.size = contentSize;
            m_ContentBounds.center = contentPos;

            if (movementType == MovementType.Clamped)
            {
                // Adjust content so that content bounds bottom (right side) is never higher (to the left) than the view bounds bottom (right side).
                // top (left side) is never lower (to the right) than the view bounds top (left side).
                // All this can happen if content has shrunk.
                // This works because content size is at least as big as view size (because of the call to InternalUpdateBounds above).
                Vector2 delta = Vector2.zero;
                if (m_ViewBounds.max.x > m_ContentBounds.max.x)
                {
                    delta.x = Math.Min(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
                }
                else if (m_ViewBounds.min.x < m_ContentBounds.min.x)
                {
                    delta.x = Math.Max(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
                }

                if (m_ViewBounds.min.y < m_ContentBounds.min.y)
                {
                    delta.y = Math.Max(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
                }
                else if (m_ViewBounds.max.y > m_ContentBounds.max.y)
                {
                    delta.y = Math.Min(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
                }
                if (delta.sqrMagnitude > float.Epsilon)
                {
                    contentPos = m_Content.anchoredPosition + delta;
                    if (!m_Horizontal)
                        contentPos.x = m_Content.anchoredPosition.x;
                    if (!m_Vertical)
                        contentPos.y = m_Content.anchoredPosition.y;
                    AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
                }
            }
        }

        internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize, ref Vector3 contentPos)
        {
            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            Vector3 excess = viewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (contentPivot.x - 0.5f);
                contentSize.x = viewBounds.size.x;
            }
            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (contentPivot.y - 0.5f);
                contentSize.y = viewBounds.size.y;
            }
        }

        private readonly Vector3[] m_Corners = new Vector3[4];
        private Bounds GetBounds()
        {
            if (m_Content == null)
                return new Bounds();
            m_Content.GetWorldCorners(m_Corners);
            var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
            return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
        }

        internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
        {
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }
        
        //==========LoopScrollRect==========
        private Bounds GetBounds4Item(int index)
        {
            if (m_Content == null)
                return new Bounds();

            int offset = index - itemTypeStart;
            if (offset < 0 || offset >= m_Content.childCount)
                return new Bounds();

            var rt = m_Content.GetChild(offset) as RectTransform;
            if (rt == null)
                return new Bounds();
            rt.GetWorldCorners(m_Corners);

            var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
            return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
        }
        //==========LoopScrollRect==========

        private Vector2 CalculateOffset(Vector2 delta)
        {
        	//==========LoopScrollRect==========
            if (totalCount < 0 || movementType == MovementType.Unrestricted)
                return delta;

            Bounds contentBound = m_ContentBounds;
            if (m_Horizontal)
            {
                float totalSize, offset;
                GetHorizonalOffsetAndSize(out totalSize, out offset);
                
                Vector3 center = contentBound.center;
                center.x = offset;
                contentBound.Encapsulate(center);
                center.x = offset + totalSize;
                contentBound.Encapsulate(center);
            }
            if (m_Vertical)
            {
                float totalSize, offset;
                GetVerticalOffsetAndSize(out totalSize, out offset);

                Vector3 center = contentBound.center;
                center.y = offset;
                contentBound.Encapsulate(center);
                center.y = offset - totalSize;
                contentBound.Encapsulate(center);
            }
        	//==========LoopScrollRect==========
            return InternalCalculateOffset(ref m_ViewBounds, ref contentBound, m_Horizontal, m_Vertical, m_MovementType, ref delta);
        }

        internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds, bool horizontal, bool vertical, MovementType movementType, ref Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (movementType == MovementType.Unrestricted)
                return offset;
            
            Vector2 min = contentBounds.min;
            Vector2 max = contentBounds.max;

            // min/max offset extracted to check if approximately 0 and avoid recalculating layout every frame (case 1010178)

            if (horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;

                float maxOffset = viewBounds.max.x - max.x;
                float minOffset = viewBounds.min.x - min.x;

                if (minOffset < -0.001f)
                    offset.x = minOffset;
                else if (maxOffset > 0.001f)
                    offset.x = maxOffset;
            }

            if (vertical)
            {
                min.y += delta.y;
                max.y += delta.y;

                float maxOffset = viewBounds.max.y - max.y;
                float minOffset = viewBounds.min.y - min.y;

                if (maxOffset > 0.001f)
                    offset.y = maxOffset;
                else if (minOffset < -0.001f)
                    offset.y = minOffset;
            }

            return offset;
        }

        /// <summary>
        /// Override to alter or add to the code that keeps the appearance of the scroll rect synced with its data.
        /// </summary>
        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        /// <summary>
        /// Override to alter or add to the code that caches data to avoid repeated heavy operations.
        /// </summary>
        protected void SetDirtyCaching()
        {
            if (!IsActive())
                return;

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirtyCaching();
        }

        #endif
    }
}