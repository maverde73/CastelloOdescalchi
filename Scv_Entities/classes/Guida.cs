using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class Guida : EntityObject, IDataErrorInfo, INotifyPropertyChanged
	{
		#region Events

		event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events



		#region Private Fields

		private decimal compenso = 0;

		private bool validateCollege = false;

		#endregion// Private Fields



		#region Public Properties

		public string Nominativo
		{
			get { return this.Cognome + " " + this.Nome; }
		}

		public decimal Compenso
		{
			get { return compenso; }
			set { compenso = value; OnNotifyPropertyChanged("Compenso"); }
		}

		public bool ValidateCollege
		{
			get { return validateCollege; }
			set { validateCollege = value; OnNotifyPropertyChanged("ValidateCollege"); }
		}

		#endregion// Public Properties



		#region Constructors

		public Guida()
		{

		}

		#endregion// Constructors



		#region Event Handlers

		partial void OnFl_CapofilaChanged()
		{
			//OnPropertyChanged("Fl_Capofila");
			//OnPropertyChanged("Id_Collegio");
		}

		private void OnNotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handlers



		#region Error Handlers

		public string Error
		{
			//get { throw new NotImplementedException(); }
			get { return string.Empty; }
		}

		public string this[string columnName]
		{
			get
			{
				string result = null;

				switch (columnName)
				{
					case "Cognome":
						if (string.IsNullOrEmpty(Cognome))
							result = "Il campo 'Cognome' è obbligatorio";
						break;

					case "Nome":
						if (string.IsNullOrEmpty(Nome))
							result = "Il campo 'Nome' è obbligatorio";
						break;

					case "Fl_Capofila":
						if (Fl_Capofila == true)
						{
							ValidateCollege = true;
							Id_Collegio = Id_Collegio;
							OnNotifyPropertyChanged("Id_Collegio");
						}
						else
						{
							ValidateCollege = false;
							Id_Collegio = Id_Collegio;
							OnNotifyPropertyChanged("Id_Collegio");
						}
						break;

					case "Id_Collegio":
						if(ValidateCollege && (Id_Collegio == null || Id_Collegio == 0))
							result = "Il campo 'Collegio' è obbligatorio se la guida è Capofila.";
						break;
				}

				return result;
			}
		}

		#endregion// Error Handlers

	}
}
