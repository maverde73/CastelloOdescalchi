using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Scv_Model;

namespace Presentation
{
	public abstract class BaseToolBarPage : Page
	{
		public Scv_Model.CommonEvents.ToolBarCommandEventHandler OnMenuCommand;

		public BaseToolBarPage()
		{
			
		}

		protected virtual void MenuCommand(ContentPageCommandEventArgs e)
		{
			if (OnMenuCommand != null)
				OnMenuCommand(this, e);
		}

		public abstract void SetButtons(ContentPageCommandEventArgs e);


	}
}
