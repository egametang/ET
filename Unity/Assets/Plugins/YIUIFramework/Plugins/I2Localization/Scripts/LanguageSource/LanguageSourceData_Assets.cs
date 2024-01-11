using System.Linq;
using UnityEngine;

namespace I2.Loc
{
	public partial class LanguageSourceData
	{
        #region Assets

        public void UpdateAssetDictionary()
        {
            Assets.RemoveAll(x => x == null);
            mAssetDictionary = Assets.Distinct()
                                     .GroupBy(o => o.name, System.StringComparer.Ordinal)
                                     .ToDictionary(g => g.Key, g => g.First(), System.StringComparer.Ordinal);
        }

        public Object FindAsset( string Name )
		{
			if (Assets!=null)
			{
                if (mAssetDictionary==null || mAssetDictionary.Count!=Assets.Count)
                {
                    UpdateAssetDictionary();
                }
                Object obj;
                if (mAssetDictionary.TryGetValue(Name, out obj))
                {
                    return obj;
                }
				//for (int i=0, imax=Assets.Length; i<imax; ++i)
				//	if (Assets[i]!=null && Name.EndsWith( Assets[i].name, StringComparison.OrdinalIgnoreCase))
				//		return Assets[i];
			}
			return null;
		}
		
		public bool HasAsset( Object Obj )
		{
			return Assets.Contains(Obj);
		}

		public void AddAsset( Object Obj )
		{
            if (Assets.Contains(Obj))
                return;

            Assets.Add(Obj);
            UpdateAssetDictionary();
		}

		
		#endregion
	}
}