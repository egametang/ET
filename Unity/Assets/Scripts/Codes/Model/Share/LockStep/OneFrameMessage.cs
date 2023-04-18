using System;

namespace ET
{
    public partial class FrameMessage
    {
        public bool Equals(FrameMessage other)
        {
            return this.PlayerId == other.PlayerId && this.Frame == other.Frame && this.V.Equals(other.V) && this.Button == other.Button;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((FrameMessage) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.PlayerId, this.Frame, this.V, this.Button);
        }

        public static bool operator ==(FrameMessage a, FrameMessage b)
        {
            if (a.PlayerId != b.PlayerId)
            {
                return false;
            }
            
            if (a.Frame != b.Frame)
            {
                return false;
            }

            if (a.V != b.V)
            {
                return false;
            }

            if (a.Button != b.Button)
            {
                return false;
            }

            return true;
        }

        public static bool operator !=(FrameMessage a, FrameMessage b)
        {
            return !(a == b);
        }
    }


    public partial class OneFrameMessages
    {
        public bool Equals(OneFrameMessages other)
        {
            return this.Frame == other.Frame && Equals(this.Messages, other.Messages);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((OneFrameMessages) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Frame, this.Messages);
        }

        public static bool operator==(OneFrameMessages a, OneFrameMessages b)
        {
            if (a.Frame != b.Frame)
            {
                return false;
            }

            if (a.Messages.Count != b.Messages.Count)
            {
                return false;
            }

            foreach (var kv in a.Messages)
            {
                if (!b.Messages.TryGetValue(kv.Key, out FrameMessage frameMessage))
                {
                    return false;
                }

                if (kv.Value != frameMessage)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator !=(OneFrameMessages a, OneFrameMessages b)
        {
            return !(a == b);
        }
    }
}