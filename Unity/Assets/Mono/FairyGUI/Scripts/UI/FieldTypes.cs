namespace FairyGUI
{
    public enum PackageItemType
    {
        Image,
        MovieClip,
        Sound,
        Component,
        Atlas,
        Font,
        Swf,
        Misc,
        Unknown,
        Spine,
        DragoneBones
    }

    public enum ObjectType
    {
        Image,
        MovieClip,
        Swf,
        Graph,
        Loader,
        Group,
        Text,
        RichText,
        InputText,
        Component,
        List,
        Label,
        Button,
        ComboBox,
        ProgressBar,
        Slider,
        ScrollBar,
        Tree,
        Loader3D
    }

    public enum AlignType
    {
        Left,
        Center,
        Right
    }

    public enum VertAlignType
    {
        Top,
        Middle,
        Bottom
    }

    public enum OverflowType
    {
        Visible,
        Hidden,
        Scroll
    }

    public enum FillType
    {
        None,
        Scale,
        ScaleMatchHeight,
        ScaleMatchWidth,
        ScaleFree,
        ScaleNoBorder
    }

    public enum AutoSizeType
    {
        None,
        Both,
        Height,
        Shrink,
        Ellipsis
    }

    public enum ScrollType
    {
        Horizontal,
        Vertical,
        Both
    }

    public enum ScrollBarDisplayType
    {
        Default,
        Visible,
        Auto,
        Hidden
    }

    public enum RelationType
    {
        Left_Left,
        Left_Center,
        Left_Right,
        Center_Center,
        Right_Left,
        Right_Center,
        Right_Right,

        Top_Top,
        Top_Middle,
        Top_Bottom,
        Middle_Middle,
        Bottom_Top,
        Bottom_Middle,
        Bottom_Bottom,

        Width,
        Height,

        LeftExt_Left,
        LeftExt_Right,
        RightExt_Left,
        RightExt_Right,
        TopExt_Top,
        TopExt_Bottom,
        BottomExt_Top,
        BottomExt_Bottom,

        Size
    }

    public enum ListLayoutType
    {
        SingleColumn,
        SingleRow,
        FlowHorizontal,
        FlowVertical,
        Pagination
    }

    public enum ListSelectionMode
    {
        Single,
        Multiple,
        Multiple_SingleClick,
        None
    }

    public enum ProgressTitleType
    {
        Percent,
        ValueAndMax,
        Value,
        Max
    }

    public enum ButtonMode
    {
        Common,
        Check,
        Radio
    }

    public enum TransitionActionType
    {
        XY,
        Size,
        Scale,
        Pivot,
        Alpha,
        Rotation,
        Color,
        Animation,
        Visible,
        Sound,
        Transition,
        Shake,
        ColorFilter,
        Skew,
        Text,
        Icon,
        Unknown
    }

    public enum GroupLayoutType
    {
        None,
        Horizontal,
        Vertical
    }

    public enum ChildrenRenderOrder
    {
        Ascent,
        Descent,
        Arch,
    }

    public enum PopupDirection
    {
        Auto,
        Up,
        Down
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FlipType
    {
        None,
        Horizontal,
        Vertical,
        Both
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FillMethod
    {
        None = 0,

        /// <summary>
        /// The Image will be filled Horizontally
        /// </summary>
        Horizontal = 1,

        /// <summary>
        /// The Image will be filled Vertically.
        /// </summary>
        Vertical = 2,

        /// <summary>
        /// The Image will be filled Radially with the radial center in one of the corners.
        /// </summary>
        Radial90 = 3,

        /// <summary>
        /// The Image will be filled Radially with the radial center in one of the edges.
        /// </summary>
        Radial180 = 4,

        /// <summary>
        /// The Image will be filled Radially with the radial center at the center.
        /// </summary>
        Radial360 = 5,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OriginHorizontal
    {
        Left,
        Right,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OriginVertical
    {
        Top,
        Bottom
    }

    /// <summary>
    /// 
    /// </summary>
    public enum Origin90
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// 
    /// </summary>
    public enum Origin180
    {
        Top,
        Bottom,
        Left,
        Right
    }

    /// <summary>
    /// 
    /// </summary>
    public enum Origin360
    {
        Top,
        Bottom,
        Left,
        Right
    }

    public enum FocusRule
    {
        NotFocusable,
        Focusable,
        NavigationBase
    }
}
