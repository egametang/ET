namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public enum TweenPropType
    {
        None,
        X,
        Y,
        Z,
        XY,
        Position,
        Width,
        Height,
        Size,
        ScaleX,
        ScaleY,
        Scale,
        Rotation,
        RotationX,
        RotationY,
        Alpha,
        Progress
    }

    internal class TweenPropTypeUtils
    {
        internal static void SetProps(object target, TweenPropType propType, TweenValue value)
        {
            GObject g = target as GObject;
            if (g == null)
                return;

            switch (propType)
            {
                case TweenPropType.X:
                    g.x = value.x;
                    break;

                case TweenPropType.Y:
                    g.y = value.x;
                    break;

                case TweenPropType.Z:
                    g.z = value.x;
                    break;

                case TweenPropType.XY:
                    g.xy = value.vec2;
                    break;

                case TweenPropType.Position:
                    g.position = value.vec3;
                    break;

                case TweenPropType.Width:
                    g.width = value.x;
                    break;

                case TweenPropType.Height:
                    g.height = value.x;
                    break;

                case TweenPropType.Size:
                    g.size = value.vec2;
                    break;

                case TweenPropType.ScaleX:
                    g.scaleX = value.x;
                    break;

                case TweenPropType.ScaleY:
                    g.scaleY = value.x;
                    break;

                case TweenPropType.Scale:
                    g.scale = value.vec2;
                    break;

                case TweenPropType.Rotation:
                    g.rotation = value.x;
                    break;

                case TweenPropType.RotationX:
                    g.rotationX = value.x;
                    break;

                case TweenPropType.RotationY:
                    g.rotationY = value.x;
                    break;

                case TweenPropType.Alpha:
                    g.alpha = value.x;
                    break;

                case TweenPropType.Progress:
                    g.asProgress.Update(value.d);
                    break;
            }
        }
    }
}
