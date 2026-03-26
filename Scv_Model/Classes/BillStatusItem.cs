using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scv_Model
{
	public class BillStatusItem :INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion Public Events



		#region Private Fields

		private int id = 0;
		private string description = string.Empty;

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


		#region Constructors

		public BillStatusItem() { }

		public BillStatusItem(int id, string description)
		{
			this.ID = id;
			this.Description = description;
		}

		#endregion// Constructors


		#region Event Handlers

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handlers
	}
}
