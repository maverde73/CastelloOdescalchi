using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Scv_Entities;
using System.ComponentModel;

namespace Presentation
{
	public class BaseUserControl : UserControl, INotifyPropertyChanged
	{
		private LK_User user = null;
		public LK_User User
		{
			get
			{
				if (user == null)
					user = new LK_User();
				return user;
			}
			set { user = value; OnPropertyChanged("User"); }
		}


		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
