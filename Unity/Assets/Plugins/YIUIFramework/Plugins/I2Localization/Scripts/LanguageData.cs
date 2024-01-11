using System;

namespace I2.Loc
{
	public enum eLanguageDataFlags
	{
		DISABLED = 1,
		KEEP_LOADED = 2,
		NOT_LOADED = 4
	}
	[Serializable]
	public class LanguageData
	{
		public string Name;
		public string Code;
		public byte Flags;      // eLanguageDataFlags

		[NonSerialized]
		public bool Compressed = false;  // This will be used in the next version for only loading used Languages

		public bool IsEnabled () { return (Flags & (int)eLanguageDataFlags.DISABLED) == 0; }

        public void SetEnabled( bool bEnabled )
        {
            if (bEnabled) Flags = (byte)(Flags & ~(int)eLanguageDataFlags.DISABLED);
                     else Flags = (byte)(Flags | (int)eLanguageDataFlags.DISABLED);
        }

        public bool IsLoaded () { return (Flags & (int)eLanguageDataFlags.NOT_LOADED) == 0; }
		public bool CanBeUnloaded () { return (Flags & (int)eLanguageDataFlags.KEEP_LOADED) == 0; }

		public void SetLoaded ( bool loaded ) 
		{
			if (loaded) Flags = (byte)(Flags & ~(int)eLanguageDataFlags.NOT_LOADED);
	  			   else Flags = (byte)(Flags | (int)eLanguageDataFlags.NOT_LOADED);
		}
        public void SetCanBeUnLoaded(bool allowUnloading)
        {
            if (allowUnloading) Flags = (byte)(Flags & ~(int)eLanguageDataFlags.KEEP_LOADED);
                           else Flags = (byte)(Flags | (int)eLanguageDataFlags.KEEP_LOADED);
        }
    }
}