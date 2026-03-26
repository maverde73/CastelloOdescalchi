using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class Prenotazione: EntityObject, IDataErrorInfo, INotifyPropertyChanged
	{
		public Prenotazione()
		{
			this.Dt_VisiteDA = DateTime.Now;
			this.Dt_VisiteA = DateTime.Now;			
			this.Id_LinguaRisposta = 1;
			this.Id_TipoConferma = 1;
            this.Protocollo = string.Empty;
            this.Responsabile = string.Empty;
		}

		public int NProtocollo
		{
			get
			{
				int p = 0;
				int.TryParse(Protocollo, out p);
				return p;
			}
		}

		//Necessario per triggerare la Navigation Property, quando cambia il suo ID o ne viene creata una nuova
		//istanza. In questo modo anche gli altri campi verranno aggiornati nella presentation
		partial void OnResponsabileChanged()
		{
			OnPropertyChanged(this, "Responsabile");
		}

		partial void OnTot_VisitatoriChanged()
		{
			OnNestedPropertyChanged(this, "Tot_Visitatori");
		}

		partial void OnDt_VisiteDAChanged()
		{
			OnNestedPropertyChanged(this, "Dt_VisiteDA");
			OnPropertyChanged(this, "Dt_VisiteDA");
		}

		partial void OnDt_VisiteAChanged()
		{
			OnNestedPropertyChanged(this, "Dt_VisiteA");
			OnPropertyChanged(this, "Dt_VisiteA");
		}

		partial void OnId_TipoRispostaChanged()
		{
			OnNestedPropertyChanged(this, "Id_TipoRisposta");
			OnPropertyChanged(this, "Id_TipoRisposta");
		}

		public string  Error
		{
			get { return string.Empty; }
		}

		public string  this[string columnName]
		{
			get
			{
				string result = null;

				switch (columnName)
				{
					case "Richiedente.Id_Titolo":
						if(Richiedente.Id_Titolo == 0)
							result = "Il campo 'Titolo' è obbligatorio";
						break;

					case "Responsabile":
						if(string.IsNullOrEmpty(Responsabile))
							result = "Il campo 'Responsabile' è obbligatorio";
						break;

					case "Id_LinguaRisposta":
						if (Id_LinguaRisposta < 1)
							result = "Il campo 'Lingua di risposta' è obbligatorio";
							break;

					case "Tot_Visitatori":
						if(Tot_Visitatori < 1)
							result = "Il campo 'Tot visitatori' non può essere uguale a zero";
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
