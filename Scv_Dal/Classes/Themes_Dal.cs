using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Model;
using System.Collections.ObjectModel;
using System.Data.Objects.DataClasses;
using Scv_Model.Common;

namespace Scv_Dal
{
	public class Themes_Dal
	{
		public static List<ThemeItem> GetThemes()
		{
			List<ThemeItem> list = new List<ThemeItem>();

			list.Add(new ThemeItem("VistaTheme"));
			list.Add(new ThemeItem("Expression_DarkTheme"));
			list.Add(new ThemeItem("MetroTheme"));
			list.Add(new ThemeItem("ModernTheme"));
			list.Add(new ThemeItem("Office_BlackTheme"));
			list.Add(new ThemeItem("Office_BlueTheme"));
			list.Add(new ThemeItem("Office_SilverTheme"));
			list.Add(new ThemeItem("SummerTheme"));
			list.Add(new ThemeItem("TransparentTheme"));
			list.Add(new ThemeItem("Windows7Theme"));
			list.Add(new ThemeItem("Windows8Theme"));
			list.Add(new ThemeItem("Windows8TouchTheme"));

			return list;
		}
	}
}
