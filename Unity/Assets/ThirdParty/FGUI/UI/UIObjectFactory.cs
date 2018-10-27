using System;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class UIObjectFactory
	{
		public delegate GComponent GComponentCreator();
		public delegate GLoader GLoaderCreator();

		static Dictionary<string, GComponentCreator> packageItemExtensions = new Dictionary<string, GComponentCreator>();
		static GLoaderCreator loaderCreator;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="type"></param>
        public static void SetPackageItemExtension(string url, System.Type type)
	    {
	        SetPackageItemExtension(url, () => { return (GComponent) Activator.CreateInstance(type); });
	    }

	    /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="creator"></param>
        public static void SetPackageItemExtension(string url, GComponentCreator creator)
		{
			if (url == null) throw new Exception("Invaild url: " + url);

			PackageItem pi = UIPackage.GetItemByURL(url);
			if (pi != null) pi.extensionCreator = creator;

			packageItemExtensions[url] = creator;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public static void SetLoaderExtension(System.Type type)
		{
			loaderCreator = () => { return (GLoader)Activator.CreateInstance(type); };
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="creator"></param>
		public static void SetLoaderExtension(GLoaderCreator creator)
		{
			loaderCreator = creator;
		}

		internal static void ResolvePackageItemExtension(PackageItem pi)
		{
			if (!packageItemExtensions.TryGetValue(UIPackage.URL_PREFIX + pi.owner.id + pi.id, out pi.extensionCreator)
				&& !packageItemExtensions.TryGetValue(UIPackage.URL_PREFIX + pi.owner.name + "/" + pi.name, out pi.extensionCreator))
				pi.extensionCreator = null;
		}

		public static void Clear()
		{
			packageItemExtensions.Clear();
			loaderCreator = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pi"></param>
		/// <returns></returns>
		public static GObject NewObject(PackageItem pi)
		{
			if (pi.extensionCreator != null)
			{
				Stats.LatestObjectCreation++;
				return pi.extensionCreator();
			}
			else
				return NewObject(pi.objectType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static GObject NewObject(ObjectType type)
		{
			Stats.LatestObjectCreation++;

			switch (type)
			{
				case ObjectType.Image:
					return new GImage();

				case ObjectType.MovieClip:
					return new GMovieClip();

				case ObjectType.Component:
					return new GComponent();

				case ObjectType.Text:
					return new GTextField();

				case ObjectType.RichText:
					return new GRichTextField();

				case ObjectType.InputText:
					return new GTextInput();

				case ObjectType.Group:
					return new GGroup();

				case ObjectType.List:
					return new GList();

				case ObjectType.Graph:
					return new GGraph();

				case ObjectType.Loader:
					if (loaderCreator != null)
						return loaderCreator();
					else
						return new GLoader();

				case ObjectType.Button:
					return new GButton();

				case ObjectType.Label:
					return new GLabel();

				case ObjectType.ProgressBar:
					return new GProgressBar();

				case ObjectType.Slider:
					return new GSlider();

				case ObjectType.ScrollBar:
					return new GScrollBar();

				case ObjectType.ComboBox:
					return new GComboBox();

				default:
					return null;
			}
		}
	}
}
