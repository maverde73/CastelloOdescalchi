using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scv_Model
{
	public class ExportType : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events

		#region Public Properties

		public int ID { get; set; }
		public string Description { get; set; }

		#endregion// Public Properties



		#region Constructors

		public ExportType()
		{ }

		public ExportType(int id, string description)
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
