using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Scv_Model
{
	public class CommonEvents
	{
		public delegate void ContextChangedEventHandler(ContextArgs e);
		public delegate void ContentCommandEventHandler(ContentCommandEventArgs e);
		public delegate void MenuCommandEventHandler(object sender, MenuEventArgs e);
		public delegate void ToolBarCommandEventHandler(object sender, ContentPageCommandEventArgs e);
		public delegate void ContentPageCommandEventHandler(ContentPageCommandEventArgs e);
		public delegate void ContentPageItemSelectionEventHandler(ContentPageCommandEventArgs e);
		public delegate void ContextButtoneventHandler(ContextArgs e);
      
	}
}
