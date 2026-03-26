using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scv_Entities
{
	public class CsvFieldSeparationTypeItem : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events




		#region Private Fields

		private int id = 0;
		private string description = string.Empty;
		private string code = string.Empty;

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

		public string Code 
		{
			get { return code; }
			set { code = value; OnPropertyChanged(this, "Code"); }
		}

		#endregion// Public Properties



		#region Constructors

		public CsvFieldSeparationTypeItem()
		{ }

		public CsvFieldSeparationTypeItem(int id, string description, string code)
		{
			this.ID = id;
			this.Description = description;
			this.Code = code;
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
