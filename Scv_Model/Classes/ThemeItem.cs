using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class ThemeItem
	{
		public ThemeItem()
		{
		}

		public ThemeItem(string name, string value)
		{
			this.Name = name;
			this.Value = value;
		}

		public ThemeItem(string name)
		{
			this.Name = name;
			this.Value = name;
		}

		public string Name { get; set; }
		public string Value { get; set; }
	}
}
