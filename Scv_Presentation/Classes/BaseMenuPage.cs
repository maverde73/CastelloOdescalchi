using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Scv_Model;
using Scv_Entities;

namespace Presentation
{
	public abstract class BaseMenuPage : Page
	{
		public Scv_Model.CommonEvents.MenuCommandEventHandler OnMenuCommand;

		public BaseMenuPage()
		{
			
		}

		private LK_User user = null;
		public LK_User User
		{
			get
			{
				if (user == null)
					user = new LK_User();
				return user;
			}
			set { user = value; }
		}

		protected virtual void MenuCommand(MenuEventArgs e)
		{
			if (OnMenuCommand != null)
				OnMenuCommand(this, e);
		}

	}
}
