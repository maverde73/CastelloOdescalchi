using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scv_Model
{
	public partial class PayRollPrintType: INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events



		#region Private Fields

		private int id = 0;

		string description = string.Empty;

		#endregion// Private Fields



		#region Public Properties

		public int ID
		{
			get { return id; }
			set { id = value; OnPropertyChanged(this, "ID"); }
		}

		public string Description
		{
			get { return description; }
			set { description = value; OnPropertyChanged(this, "Description"); }
		}

		#endregion// Public Properties



		#region Private Methods

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Private Methods
	}
}
