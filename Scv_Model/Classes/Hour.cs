using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scv_Model
{
	public class Hour : INotifyPropertyChanged
	{
		private string time = "";
		public string Time
		{
			get
			{
				return time;
			}
			set
			{
				OnPropertyChanged(this, "Time");
				time = value;
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}
	}
}
