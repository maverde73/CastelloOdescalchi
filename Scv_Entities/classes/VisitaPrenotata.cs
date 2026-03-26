using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class VisitaPrenotata : EntityObject, IDataErrorInfo, INotifyPropertyChanged
	{
		private bool isEmpty = true;
		public bool IsEmpty
		{
			get { return isEmpty; }
			set { isEmpty = value; OnPropertyChanged(this, "IsEmpty"); }
		}

		private bool editing = false;
		public bool Editing
		{
			get { return editing; }
			set { editing = value; }
		}

		private bool isErasable = false;
		public bool IsErasable
		{
			get { return isErasable; }
			set { isErasable = value; OnPropertyChanged(this, "IsErasable"); }
		}

		private bool isLoadedFromDb = false;
		public bool IsLoadedFromDb
		{
			get { return isLoadedFromDb; }
			set { isLoadedFromDb = value; OnPropertyChanged(this, "IsLoadedFromDb"); }
		}


		partial void OnNr_VisitatoriChanged()
		{
			OnPropertyChanged(this, "Nr_Visitatori");
		}

		partial void OnId_LinguaChanged()
		{
			OnPropertyChanged(this, "Id_Lingua");
		}

		public VisitaPrenotata()
		{
			this.Id_Lingua = 1;
			this.Id_TipoVisita = 1;
			//this.Id_TipoRisposta = 9;
		}

		public VisitaPrenotata(int LanguageID, short visitorsNumber)
		{
			this.Id_Lingua = LanguageID;
			this.Nr_Visitatori = visitorsNumber;
			this.Id_Lingua = 1;
			this.Id_TipoVisita = 1;
		}
				

		public string  Error
		{
			//get { throw new NotImplementedException(); }
			get { return string.Empty; }
		}

		public string  this[string columnName]
		{
			get
			{
				string result = null;

				switch (columnName)
				{
					case "Id_Lingua":
						if (Editing && this.Id_Lingua < 1)
							result = "Il campo 'Lingua visita' è obbligatorio";
						break;

					case "Nr_Visitatori":
						if (Editing && Nr_Visitatori  < 1)
							result = "Il campo 'Numero visitatiori' è obbligatorio";
						break;
				}

				return result;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(object sender, string propertyName)
		{
			if(PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler NestedPropertyChanged;
		private void OnNestedPropertyChanged(object sender, string propertyName)
		{
			if (NestedPropertyChanged != null)
				NestedPropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

	}
}
