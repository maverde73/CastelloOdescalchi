using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class Articolo : EntityObject, IDataErrorInfo, INotifyPropertyChanged
	{
		public string Error
		{
			get { return null; }
		}

		public string this[string columnName]
		{
			get
			{
				string result = null;

				switch (columnName)
				{
					case "Descrizione":
						if (string.IsNullOrEmpty(Descrizione))
							result = "Il campo 'Descrizione' è obbligatorio";
						break;
				}

				return result;
			}
		}

		event PropertyChangedEventHandler PropertyChanged;
		private void OnNotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
