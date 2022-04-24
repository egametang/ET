#if FEAT_COMPILER
using System;
using System.Reflection.Emit;

namespace ProtoBuf.Compiler
{
    internal sealed class Local : IDisposable
    {
        // public static readonly Local InputValue = new Local(null, null);
        private LocalBuilder value;
        private readonly Type type;
        private CompilerContext ctx;

        private Local(LocalBuilder value, Type type)
        {
            this.value = value;
            this.type = type;
        }

        internal Local(CompilerContext ctx, Type type)
        {
            this.ctx = ctx;
            if (ctx != null) { value = ctx.GetFromPool(type); }
            this.type = type;
        }

        internal LocalBuilder Value => value ?? throw new ObjectDisposedException(GetType().Name);

        public Type Type => type;

        public Local AsCopy()
        {
            if (ctx == null) return this; // can re-use if context-free
            return new Local(value, this.type);
        }
        
        public void Dispose()
        {
            if (ctx != null)
            {
                // only *actually* dispose if this is context-bound; note that non-bound
                // objects are cheekily re-used, and *must* be left intact agter a "using" etc
                ctx.ReleaseToPool(value);
                value = null; 
                ctx = null;
            }            
        }

        internal bool IsSame(Local other)
        {
            if((object)this == (object)other) return true;

            object ourVal = value; // use prop to ensure obj-disposed etc
            return other != null && ourVal == (object)(other.value); 
        }
    }
}
#endif