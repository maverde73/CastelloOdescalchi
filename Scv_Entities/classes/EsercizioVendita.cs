using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.Collections.ObjectModel;
using System.ComponentModel;
namespace Scv_Entities
{
	public partial class EsercizioVendita : EntityObject, IDataErrorInfo, INotifyPropertyChanged
	{
		public EsercizioVendita()
		{

		}

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
					case "Descrizione":
						if (string.IsNullOrEmpty(Descrizione))
							result = "Il campo 'Descrizione' è obbligatorio";
						break;

					case "Sconto":
						if (Sconto == 0)
							result = "Il campo 'Sconto' è obbligatorio";
						break;

					case "Contatto":
						if (string.IsNullOrEmpty(Contatto))
							result = "Il campo 'Nome contatto' è obbligatorio";
						break;

					case "Tel_Ufficio":
						if (string.IsNullOrEmpty(Tel_Ufficio))
							result = "Il campo 'Tel. Uff.' è obbligatorio";
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
