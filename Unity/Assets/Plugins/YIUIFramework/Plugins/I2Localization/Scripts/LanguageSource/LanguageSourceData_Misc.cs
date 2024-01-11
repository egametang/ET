using System.Collections.Generic;

namespace I2.Loc
{
	public partial class LanguageSourceData
	{
		public static string EmptyCategory = "Default";
		public static char[] CategorySeparators = "/\\".ToCharArray();

		#region Keys
		
		public List<string> GetCategories( bool OnlyMainCategory = false, List<string> Categories = null )
		{
			if (Categories==null)
				Categories = new List<string>();
			
			foreach (TermData data in mTerms)
			{
				string sCategory = GetCategoryFromFullTerm( data.Term, OnlyMainCategory );
				if (!Categories.Contains(sCategory))
					Categories.Add(sCategory);
			}
			Categories.Sort();
			return Categories;
		}
		
		public static string GetKeyFromFullTerm( string FullTerm, bool OnlyMainCategory = false )
		{
			int Index = OnlyMainCategory ? FullTerm.IndexOfAny(CategorySeparators) : 
				FullTerm.LastIndexOfAny(CategorySeparators);

			return Index<0 ? FullTerm :FullTerm.Substring(Index+1);
		}
		
		public static string GetCategoryFromFullTerm( string FullTerm, bool OnlyMainCategory = false )
		{
			int Index = OnlyMainCategory ? FullTerm.IndexOfAny(CategorySeparators) : 
				FullTerm.LastIndexOfAny(CategorySeparators);

			return Index<0 ? EmptyCategory : FullTerm.Substring(0, Index);
		}
		
		public static void DeserializeFullTerm( string FullTerm, out string Key, out string Category, bool OnlyMainCategory = false )
		{
			int Index = OnlyMainCategory ? FullTerm.IndexOfAny(CategorySeparators) : 
				FullTerm.LastIndexOfAny(CategorySeparators);

			if (Index<0) 
			{
				Category = EmptyCategory;
				Key = FullTerm;
			}
			else 
			{
				Category = FullTerm.Substring(0, Index);
				Key = FullTerm.Substring(Index+1);
			}
		}

		#endregion
		
		#region Misc
		
		#endregion

	}
}