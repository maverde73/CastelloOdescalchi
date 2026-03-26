using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Scv_Model
{
	public static class ApplicationState
	{
		public static string AppPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
		public static string XmlPath = AppPath + "\\" + "xml" + "\\";
		public static string PagesPath = "Pages" + "\\";
        public static string CustomControlsPath = "CustomControls" + "\\";
		public static string IconsPath = AppPath + "\\" + "Themes\\Images\\ButtonIcons" + "\\";

        private static Dictionary<string, object> _values = new Dictionary<string, object>();

        public static void SetValue(string key, object value)
        {
            if (_values.ContainsKey(key))
            {
                _values.Remove(key);
            }
            _values.Add(key, value);
        }

        public static T GetValue<T>(string key)
        {
            if (_values.ContainsKey(key))
            {
                return (T)_values[key];
            }
            else
            {
                return default(T);
            }
        }
	}
}
