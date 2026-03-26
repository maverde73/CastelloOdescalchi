using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
    public partial class Pagamento : EntityObject, IDataErrorInfo, INotifyPropertyChanged
    {
        #region Private Fields

        private bool validateAutorizzazione = false;

        private bool showPayDifferenceButton = false;

        private bool showDeleteButton = false;

        private bool showUpdatePaymentButton = false;

        //ONLINEPAYMENT
        private bool showPaymentButton = false;

        #endregion// Private Fields




        #region Public Properties

        public decimal? Importo
        {
            get { return (Importo_Interi != null ? (decimal)Importo_Interi : 0) + (Importo_Ridotti != null ? (decimal)Importo_Ridotti : 0) + (Importo_Scontati != null ? (decimal)Importo_Scontati : 0) + (Importo_Cumulativi != null ? (decimal)Importo_Cumulativi : 0); }
        }

        public int? Nr_Biglietti
        {
            get { return (Nr_InteriPagati != null ? (int)Nr_InteriPagati : 0) + (Nr_RidottiPagati != null ? (int)Nr_RidottiPagati : 0) + (Nr_ScontatiPagati != null ? (int)Nr_ScontatiPagati : 0) + (Nr_CumulativiPagati != null ? (int)Nr_CumulativiPagati : 0); }
        }

        public bool ValidateAutorizzazione
        {
            get { return validateAutorizzazione; }
            set
            {
                validateAutorizzazione = value;
                OnPropertyChanged(this, "ValidateAutorizzazione");
                OnPropertyChanged(this, "Autorizzazione");
            }
        }

        public string PaymentRange
        {
            get { return Id_VisitaProgrammata == null ? "Pagamento protocollo" : "Pagamento visita"; }
        }

        public bool ShowPaymentButton
        {
            //ONLINEPAYMENT
            get
            {
                //showPaymentButton = Id_Pagamento != null && Id_Pagamento > 0 && Dt_Pagamento != null ? false : true;
                return showPaymentButton;
            }
            set { showPaymentButton = value; OnPropertyChanged(this, "ShowPaymentButton"); }
        }

        public bool ShowPayDifferenceButton
        {
            get { return showPayDifferenceButton; }
            set { showPayDifferenceButton = value; OnPropertyChanged(this, "ShowPayDifferenceButton"); }
        }

        public bool ShowDeleteButton
        {
            get { return showDeleteButton; }
            set { showDeleteButton = value; OnPropertyChanged(this, "ShowDeleteButton"); }
        }

        public bool ShowUpdatePaymentButton
        {
            get { return showUpdatePaymentButton; }
            set { showUpdatePaymentButton = value; OnPropertyChanged(this, "ShowUpdatePaymentButton"); }
        }


        #endregion//* Public Properties



        #region Overrides

        partial void OnNr_InteriPagatiChanged()
        {
            OnPropertyChanged(this, "Pagamento");
        }

        partial void OnNr_RidottiPagatiChanged()
        {
            OnPropertyChanged(this, "Pagamento");
        }

        #endregion// Overrides




        #region Error Handling

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
                    case "Autorizzazione":
                        if (ValidateAutorizzazione && string.IsNullOrEmpty(Autorizzazione))
                            result = "Autorizzazione al pagamento necessaria";
                        break;

                    //case "Id_TipoPagamento":
                    //    if (Id_TipoPagamento == null || Id_TipoPagamento == 0)
                    //        result = "Non è stato scelto un tipo di pagamento";
                    //    break;

                }

                return result;
            }
        }

        #endregion// Error Handling




        #region Event Handling

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        #endregion// Event Handling
    }
}
