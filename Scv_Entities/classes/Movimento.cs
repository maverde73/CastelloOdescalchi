using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;
namespace Scv_Entities
{
	public partial class Movimento : EntityObject, IDataErrorInfo, INotifyPropertyChanged
	{
        private bool validatePayment = false;
        public bool ValidatePayment
        {
            get { return validatePayment; }
            set 
            { 
                validatePayment = value;
                OnPropertyChanged(this, "ValidatePayment");
                OnPropertyChanged(this, "Id_TipoPagamento");
            }
        }

        private bool validateAuthorization = false;
        public bool ValidateAuthorization
        {
            get { return validateAuthorization; }
            set 
            { 
                validateAuthorization = value;
                OnPropertyChanged(this, "ValidateAuthorization");
                OnPropertyChanged(this, "Nota");
            }
        }

		private bool validatePosNumber = false;
		public bool ValidatePosNumber
		{
			get { return validatePosNumber; }
			set
			{
				validatePosNumber = value;
				OnPropertyChanged(this, "ValidatePosNumber");
				OnPropertyChanged(this, "Nr_Pos");
			}
		}

        private bool validateStore = false;
        public bool ValidateStore
        {
            get { return validateStore; }
            set
            {
                validateStore = value;
                OnPropertyChanged(this, "ValidateStore");
                OnPropertyChanged(this, "Id_EsercizioVendita");
            }
        }

		private bool validateGuide = false;
		public bool ValidateGuide
		{
			get { return validateGuide; }
			set
			{
				validateGuide = value;
				OnPropertyChanged(this, "ValidateGuide");
				OnPropertyChanged(this, "Id_Guida");
			}
		}

		private bool validateRefundName = false;
		public bool ValidateRefundName
		{
			get { return validateRefundName; }
			set
			{
				validateRefundName = value;
				OnPropertyChanged(this, "ValidateRefundName");
				OnPropertyChanged(this, "NominativoRimborso");
			}
		}

		private bool validatePrice = false;
		public bool ValidatePrice
		{
			get { return validatePrice; }
			set
			{
				validatePrice = value;
				OnPropertyChanged(this, "ValidatePrice");
				OnPropertyChanged(this, "PrezzoVendita");
			}
		}

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

		public bool IsMoney
		{
			get
			{
				return (
					Id_TipoMovimento == 3
					||
					Id_TipoMovimento == 8
					||
					Id_TipoMovimento == 9
					||
					Id_TipoMovimento == 11
					||
					Id_TipoMovimento == 12
				);
			}
		}

		private int esistenzaUfficio = 0;
		public int EsistenzaUfficio
		{
			get { return esistenzaUfficio; }
			set { esistenzaUfficio = value; OnPropertyChanged(this, "EsistenzaUfficio"); }
		}

		private int esistenzaMagazzino = 0;
		public int EsistenzaMagazzino
		{
			get { return esistenzaMagazzino; }
			set { esistenzaMagazzino = value; OnPropertyChanged(this, "EsistenzaMagazzino"); }
		}

		private decimal totaleComplessivo = 0;
		public decimal TotaleComplessivo
		{
			get { return totaleComplessivo; }
			set { totaleComplessivo = value; OnPropertyChanged(this, "TotaleComplessivo"); }
		}

		private decimal totale = 0;
		public decimal Totale
		{
			get { return totale; }
			set { totale = value; OnPropertyChanged(this, "Totale"); }
		}

		partial void OnId_TipoMovimentoChanged()
		{
			OnPropertyChanged(this, "Id_TipoMovimento");
		}

        partial void OnId_TipoPagamentoChanged()
        {
            OnPropertyChanged(this, "Id_TipoPagamento");
        }

        partial void OnId_EsercizioVenditaChanged()
        {
            OnPropertyChanged(this, "Id_EsercizioVendita");
        }

		//partial void OnNominativoRimborsoChanged()
		//{
		//    OnPropertyChanged(this, "NominativoRimborso");
		//}

		partial void OnPrezzoPubblicoChanged()
		{
			OnPropertyChanged(this, "PrezzoPubblico");
		}

		partial void OnPrezzoVenditaChanged()
		{
			OnPropertyChanged(this, "PrezzoVendita");
		}

		partial void OnScontoChanged()
		{
			OnPropertyChanged(this, "Sconto");
		}

		partial void OnNr_PezziChanged()
		{
			OnPropertyChanged(this, "Nr_Pezzi");
		}

        partial void OnNotaChanged()
        {
            OnPropertyChanged(this, "Nota");
        }

		partial void OnNr_PosChanged()
		{
			OnPropertyChanged(this, "Nr_Pos");
		}

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
					case "Nr_Pezzi":
                        if (Editing && ((Nr_Pezzi == null || Nr_Pezzi < 1) && (Id_Articolo != null && Id_Articolo > 0)))
							result = "Il numero pezzi deve essere maggiore di zero.";
						break;

					case "Id_Articolo":
                        if (Editing && ((Id_Articolo == null || Id_Articolo < 1)) && (Nr_Pezzi != null && Nr_Pezzi > 1))
							result = "Il campo 'Articolo' è obbligatorio.";
						break;

                    case "Dt_Movimento":
                        if (Dt_Movimento == null)
                            result = "Il campo 'Data movimento' è obbligatorio.";
                        break;

                    case "Id_TipoMovimento":
                        if (Id_TipoMovimento == 0)
                            result = "Il campo 'Tipo movimento' è obbligatorio.";
                        break;

                    case "Id_TipoPagamento":
                        if (ValidatePayment && (Id_TipoPagamento == null || Id_TipoPagamento == 0))
                            result = "Il campo 'Tipo pagamento' è obbligatorio.";
                        break;

                    case "Id_EsercizioVendita":
                        if (ValidateStore && (Id_EsercizioVendita == null || Id_EsercizioVendita == 0))
                            result = "Il campo 'Esercizio vendita' è obbligatorio.";
                        break;

					case "Id_Guida":
						if (ValidateGuide && (Id_Guida == null || Id_Guida == 0))
							result = "Il campo 'Guida' è obbligatorio.";
						break;

					case "NominativoRimborso":
						if (ValidateRefundName && string.IsNullOrEmpty(NominativoRimborso))
							result = "Il campo 'Nominativo' è obbligatorio.";
						break;

					case "PrezzoVendita":
						if (ValidatePrice && (PrezzoVendita == null || PrezzoVendita == 0))
							result = "Il campo 'Importo' è obbligatorio.";
						break;

                    case "Nota":
                        if (ValidateAuthorization && string.IsNullOrEmpty(Nota))
                            result = "Il campo 'Autorizzazione/Nota' è obbligatorio.";
                        break;

					case "Nr_Pos":
						if (ValidatePosNumber && string.IsNullOrEmpty(Nr_Pos))
							result = "Il campo 'Numero POS' è obbligatorio.";
						break;

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
