using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data.Objects.DataClasses;

namespace Scv_Entities
{
	public partial class Richiedente : EntityObject, IDataErrorInfo, INotifyPropertyChanged
	{

		public Richiedente()
		{			
			//this.Id_LinguaAbituale = 1;
			this.Dt_Update = DateTime.Now;
            this.Is_PF = true;
            this.Sesso = "M";
		}
		public Richiedente(int languageID)
		{
			this.Id_LinguaAbituale = languageID;
		}


		public bool ValidateAddress { get; set; }

		public string Nominativo
		{
			get { return (!string.IsNullOrEmpty(Cognome) ? Cognome + " " : string.Empty) + Nome; }
		}



		//Necessario per ottenere che, scrivendo un cognome nel search box, se non vengono trovati
		//match con quanto digitato, lo stesso testo si componga nella casella cognome nel
		//richiedente della prenotazione
		partial void OnId_RichiedenteChanged()
		{
			OnPropertyChanged(this, "Id_Richiedente");
		}
		partial void  OnCognomeChanged()
		{
			OnPropertyChanged(this, "Cognome");
		}

		partial void OnNomeChanged()
		{
			OnPropertyChanged(this, "Nome");
		}

		partial void OnId_TitoloChanged()
		{
			OnPropertyChanged(this, "LK_Titolo");
		}

		partial void OnId_LinguaAbitualeChanged()
		{
			OnPropertyChanged(this, "Id_LinguaAbituale");			
		}

		partial void OnEmailChanged()
		{
			OnPropertyChanged(this, "Email");
		}

		partial void OnTel_CasaChanged()
		{
			OnPropertyChanged(this, "Tel_Casa");
		}

		partial void OnTel_CellulareChanged()
		{
			OnPropertyChanged(this, "Tel_Cellulare");
		}

		partial void OnTel_UfficioChanged()
		{
			OnPropertyChanged(this, "Tel_Ufficio");
		}
		
        partial void OnId_CittaChanged()
        {
            OnPropertyChanged(this, "LK_Citta");
        }

        partial void OnId_OrganizzazioneChanged()
        {
            OnPropertyChanged(this, "LK_Organizzazione");
        }

		public string SearchField
		{
			get { return Nome + Cognome; }
		}

		public string ShowField
		{
			get { return Cognome + ", " + Nome; }
		}

		public string Error
		{
			get { return "false"; }
		}

        public string this[string columnName]
        {
            get
            {
                string result = null;

				switch (columnName)
				{
				    case "Cognome":
						//Id_LinguaAbituale = Id_LinguaAbituale;
						//OnPropertyChanged(this, "Id_LinguaAbituale");

				        if (string.IsNullOrEmpty(Cognome))
				            result = "Il campo 'Cognome o Ragione Sociale' è obbligatorio";
				        break;

                    //case "Nome":
                    //    if (string.IsNullOrEmpty(Nome))
                    //        result = "Il campo 'Nome' è obbligatorio";
                    //    break;

					//case "Id_Titolo":
					//    if (string.IsNullOrEmpty(Nome))
					//        result = "Il campo 'Titolo' è obbligatorio";
					//    break;

					//case "Id_LinguaAbituale":
					//    if (Id_LinguaAbituale == (int?)null || Id_LinguaAbituale == 0)
					//        result = "Il campo 'Lingua abituale' è obbligatorio";
					//    break;
				}

                return result;
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
