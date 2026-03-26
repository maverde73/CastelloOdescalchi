using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Scv_Entities;
using Scv_Dal;
using System.Collections.ObjectModel;
using Scv_Model;
using System.Windows;
using System.Configuration;

namespace Presentation.CustomControls.PaymentLib
{
	public class PaymentViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events

		#region Private Fields

		private List<LK_TipoPagamento> availablePaymentTypes = null;

		private ObservableCollection<PaymentItem> objPayments = null;

		private int objPaymentTypeID = 0;

		private decimal objTotalPaid = 0;

		private PaymentCheckResult result = null;

		private int? visitID = (int?)null;

		private int prenotationID = 0;

		private PaymentRange paymentRange = PaymentRange.PerVisit;

		private List<V_VisiteProgrammate> visits = null;

		private Pagamento objPayment = null;

		private string visitInfo = string.Empty;

        //ONLINEPAYMENT
        bool onlinePaymentEnabled = false;

		#endregion// Private Fields

		#region Dal

		Pagamento_Dal dalPayment = new Pagamento_Dal();

		VisitaProgrammata_Dal dalVisits = new VisitaProgrammata_Dal();

		Parametri_Dal dalParameters = new Parametri_Dal();

		#endregion// Dal

		#region Public Properties

		public List<LK_TipoPagamento> AvailablePaymentTypes
		{
			get
			{
				if (availablePaymentTypes == null)
					availablePaymentTypes = new List<LK_TipoPagamento>();
				return availablePaymentTypes;
			}
			set { availablePaymentTypes = value; OnPropertyChanged(this, "AvailableObjPaymentTypes"); }
		}

		public ObservableCollection<PaymentItem> ObjPayments
		{
			get
			{
				if (objPayments == null)
					objPayments = new ObservableCollection<PaymentItem>();
				return objPayments;
			}
			set { objPayments = value; OnPropertyChanged(this, "ObjPayments"); }
		}

		public int ObjPaymentTypeID
		{
			get { return objPaymentTypeID; }
			set { objPaymentTypeID = value; OnPropertyChanged(this, "ObjPaymentTypeID"); }
		}

		public decimal ObjTotalPaid
		{
			get { return objTotalPaid; }
			set { objTotalPaid = value; OnPropertyChanged(this, "ObjTotalPaid"); }
		}

		public PaymentCheckResult Result
		{
			get
			{
				if (result == null)
					result = new PaymentCheckResult();
				return result;
			}
			set { result = value; }
		}

		public Pagamento ObjPayment
		{
			get	{ return objPayment; }
			set { objPayment = value; OnPropertyChanged(this, "ObjPayment"); }
		}

		public int? VisitID
		{
			get { return visitID; }
			set { visitID = value; OnPropertyChanged(this, "VisitID"); }
		}

		public int PrenotationID
		{
			get { return prenotationID; }
			set { prenotationID = value; OnPropertyChanged(this, "PrenotationID"); }
		}

		public List<V_VisiteProgrammate> Visits
		{
			get
			{
				if (visits == null)
					visits = new List<V_VisiteProgrammate>();
				return visits;
			}
			set { visits = value; OnPropertyChanged(this, "Visits"); }
		}

		public PaymentRange PaymentRange
		{
			get { return paymentRange; }
			set { paymentRange = value; OnPropertyChanged(this, "PaymentRange"); }
		}

		public string VisitInfo
		{
			get { return visitInfo; }
			set { visitInfo = value; OnPropertyChanged(this, "visitInfo"); }
		}

        ObservableCollection<V_OnLinePaymentLog> list_OnLinePaymentLog = null;
        public ObservableCollection<V_OnLinePaymentLog> List_OnLinePaymentLog
        {
            get
            {
                if (list_OnLinePaymentLog == null)
                    list_OnLinePaymentLog = new ObservableCollection<V_OnLinePaymentLog>();
                return list_OnLinePaymentLog;
            }
            set { list_OnLinePaymentLog = value; OnPropertyChanged(this, "List_OnLinePaymentLog"); }
        }

		#endregion// Public Properties

		#region Constructors

		public PaymentViewModel(PaymentRange paymentRange, int prenotationID, int? visitID)
		{
			this.VisitID = visitID;
			this.PaymentRange = paymentRange;
			this.PrenotationID = prenotationID;

            if (ConfigurationManager.AppSettings["enableonlinepayment"] != null)
                onlinePaymentEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["enableonlinepayment"]);

			LoadAvailablePaymentTypes(paymentRange);
			AddNewPaymentRow(0);
			LoadData();
		}

        //ONLINEPAYMENT
        public PaymentViewModel(int prenotationID,bool resetOrderNumber = true)
        {
            this.PrenotationID = prenotationID;
            LoadDataForPaymentOrder(resetOrderNumber);
        }

		#endregion// Constructors

		#region Event Handlers

		private void OnPropertyChanged(object sender, string propertyName)
		{
			switch (propertyName)
			{
				case "ObjFullPriceTickets":
				case "ObjReducedTickets":
                case "ObjDiscountTickets":
                case "ObjCumulativeTickets":
					UpdateTotals();
					break;
			}

			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handlers

		#region Public Methods

		public void LoadData()
		{
			Visits.Clear();

			switch (PaymentRange)
			{
				case PaymentRange.PerVisit:
					ObjPayment = dalPayment.GetItemByIdVisitaProgrammata((int)visitID);
					Visits.Add((dalVisits.GetVListById((int)visitID)));
					VisitInfo = "Visita ore: " + Visits[0].Ora_Visita + ", lingua: " + Visits[0].LinguaVisita + " - Protocollo: " + new Prenotazione_Dal().Get_V_Item(PrenotationID).NProtocollo;
					break;

				case PaymentRange.PerPrenotation:
					ObjPayment = dalPayment.GetItemByIdPrenotazione(prenotationID);
					Visits.AddRange(dalVisits.GetVListByIdPrenotazione(prenotationID));
					VisitInfo = Visits.Count.ToString() + " visite - Protocollo: " + new Prenotazione_Dal().Get_V_Item(PrenotationID).NProtocollo;
                    if(ObjPayment != null)
                        LoadList_OnLinePaymentLog(ObjPayment.Id_Pagamento);
					break;
			}

			Result = CheckTickets();

			if (!Result.Success)
			{
				if (MessageBox.Show(result.Message, "Variazione Numero Biglietti", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
				{
					AllineaPagamento();
					LoadData();
				}
				else
					ObjPayment.ShowPayDifferenceButton = true;
			}

			if (ObjPayment != null)
			{
                //ONLINEPAYMENT
                ObjPayment.ShowDeleteButton = dalPayment.GetDeliveredTickets(PrenotationID, VisitID) == 0;

                if (ObjPayment.Id_TipoPagamento != 1)
                {
                    ObjPayment.ShowUpdatePaymentButton = true/*dalPayment.GetDeliveredTickets(PrenotationID, VisitID) == 0*/;
                    if (ObjPayment.Dt_Pagamento != null)
                    {
                        if (ObjPayment.Dt_Pagamento.Value.Date != DateTime.Now.Date)
                            ObjPayment.ShowUpdatePaymentButton = false;
                    }
                }
                else
                {
                    //LA MODIFIFICA DEL PAGAMENTO ONLINE NON E' CONSENTITA
                    //AD ESSERE CONSENTITA E' SOLO LA MODIFICA DELL'ORDINE DI PAGAMENTO ONLINE(Dt_Pagamento == null) 
                    ObjPayment.ShowUpdatePaymentButton = (ObjPayment.Dt_Pagamento == null);
                    if (ObjPayment.Dt_Pagamento == null)
                        ObjPayment.ShowPaymentButton = true;
                }
			}
			else
			{
				decimal FullPrice = 0;
				decimal ReducedPrice = 0;
                decimal DiscountPrice = 0;
                decimal CumulativePrice = 0;

                LK_TipoVisita tv = dalParameters.GetTipoVisitaByIdPrenotazione(PrenotationID);

                if (tv != null)
                {
                    if (tv.PrezzoIntero != null)
                        FullPrice = (decimal)tv.PrezzoIntero;

                    if (tv.PrezzoRidotto != null)
                        ReducedPrice = (decimal)tv.PrezzoRidotto;

                    if (tv.PrezzoScontato != null)
                        DiscountPrice = (decimal)tv.PrezzoScontato;

                    if (tv.PrezzoCumulativo != null)
                        CumulativePrice = (decimal)tv.PrezzoCumulativo;
                }


				ObjPayment = new Pagamento();
				ObjPayment.Id_VisitaProgrammata = paymentRange == PaymentRange.PerVisit ? visitID : (int?)null;
				ObjPayment.Id_Prenotazione = prenotationID;
				ObjPayment.Id_TipoPagamento = 3;
				ObjPayment.Dt_Pagamento = DateTime.Now.Date;
				ObjPayment.Nr_InteriPagati = (int)visits.Sum(x => (x.Nr_Interi != null ? x.Nr_Interi : 0));
				ObjPayment.Nr_RidottiPagati = (int)visits.Sum(x => (x.Nr_Ridotti != null ? x.Nr_Ridotti : 0));
                ObjPayment.Nr_ScontatiPagati = (int)visits.Sum(x => (x.Nr_Scontati != null ? x.Nr_Scontati : 0));
                ObjPayment.Nr_CumulativiPagati = (int)visits.Sum(x => (x.Nr_Cumulativi != null ? x.Nr_Cumulativi : 0));
				ObjPayment.Importo_Interi = ObjPayment.Nr_InteriPagati * FullPrice;
                ObjPayment.Importo_Ridotti = ObjPayment.Nr_RidottiPagati * ReducedPrice;
                ObjPayment.Importo_Scontati = ObjPayment.Nr_ScontatiPagati * DiscountPrice;
                ObjPayment.Importo_Cumulativi = CumulativePrice;
                ObjPayment.PrezzoIntero = FullPrice;
                ObjPayment.PrezzoRidotto = ReducedPrice;
                ObjPayment.PrezzoCumulativo = CumulativePrice;
                ObjPayment.PrezzoScontato = DiscountPrice;
			}

			ObjPayment.FL_PagamentoParziale = PaymentRange == PaymentLib.PaymentRange.PerVisit;

		}

        //ONLINEPAYMENT
        public void LoadDataForPaymentOrder(bool resetOrderNumber = true)
        {
           var pagamenti = dalPayment.GetListByIdPrenotazione(prenotationID);

           if (pagamenti.Count > 0)
           {
               var pagamento = pagamenti[0];
               if (pagamento != null)
               {
                   if (pagamento.Id_TipoPagamento == 1 && pagamento.Dt_Pagamento == null)
                   {
                       //ORDINE DI PAGAMENTO DA AGGIORNARE E RI-INVIARE
                       if(resetOrderNumber)
                        pagamento.NumeroOrdine = null;
                       ObjPayment = pagamento;
                   }
               }
           }
           else
           {
               //NUOVO ORDINE DI PAGAMENTO
               ObjPayment = new Pagamento();
               ObjPayment.Id_VisitaProgrammata = null;
               ObjPayment.Id_Prenotazione = prenotationID;
               ObjPayment.Id_TipoPagamento = 1;
               ObjPayment.Dt_Pagamento = null;
           }

           if (ObjPayment != null)
           {
               //AGGIORNAMENTO DEI DATI CONTABILI DELL'ORDINE DI PAGAMENTO
               decimal FullPrice = 0;
               decimal Discount = 0;
               decimal.TryParse(dalParameters.GetItem("biglietto_intero").Valore, out FullPrice);
               decimal.TryParse(dalParameters.GetItem("biglietto_ridotto").Valore, out Discount);

               Visits.Clear();
               Visits.AddRange(dalVisits.GetVListByIdPrenotazione(prenotationID));

               ObjPayment.Nr_InteriPagati = (int)visits.Sum(x => (x.Nr_Interi != null ? x.Nr_Interi : 0));
               ObjPayment.Nr_RidottiPagati = (int)visits.Sum(x => (x.Nr_Ridotti != null ? x.Nr_Ridotti : 0));
               ObjPayment.Importo_Interi = ObjPayment.Nr_InteriPagati * FullPrice;
               ObjPayment.Importo_Ridotti = ObjPayment.Nr_RidottiPagati * Discount;
           }


        }

		public void AddNewPaymentRow(decimal amount)
		{
			PaymentItem itm = new PaymentItem();
			itm.ID = CreatePaymentID();
			itm.PaymentTypeID = 0;
			itm.Amount = amount;
			ObjPayments.Add(itm);
		}

		public void UpdateTotals()
		{
			ObjTotalPaid = ObjPayments.Sum(x => x.Amount);

			Parametri_Dal dalParams = new Parametri_Dal();

			decimal fullPrice = 0;
			decimal discount = 0;

            //decimal.TryParse(dalParams.GetItem("biglietto_intero").Valore, out fullPrice);
            //decimal.TryParse(dalParams.GetItem("biglietto_ridotto").Valore, out discount);
		}

		public void RemovePaymentRow(int paymentID)
		{
			foreach (PaymentItem o in ObjPayments)
			{
				if (o.ID == paymentID)
				{
					ObjPayments.Remove(o);
					break;
				}
			}
		}

		public void AllineaPagamento()
		{
			decimal intero = 0;
			decimal ridotto = 0;
			Parametri_Dal dalParameters = new Parametri_Dal();
			Pagamento_Dal dalPayment = new Pagamento_Dal();

			decimal.TryParse(dalParameters.GetItem("biglietto_intero").Valore, out intero);
			decimal.TryParse(dalParameters.GetItem("biglietto_ridotto").Valore, out ridotto);

			ObjPayment.Nr_InteriPagati += result.DiffFullPrice;
			ObjPayment.Nr_RidottiPagati += result.DiffDiscount;
			ObjPayment.Importo_Interi += (intero * result.DiffFullPrice);
			ObjPayment.Importo_Ridotti += (ridotto * result.DiffDiscount);
			
			dalPayment.InsertOrUpdate(new List<Pagamento>() { ObjPayment });
		}

        public void LoadList_OnLinePaymentLog(int paymentID)
        {
            List_OnLinePaymentLog = new ObservableCollection<V_OnLinePaymentLog>(dalPayment.LoadOnLinePaymentLogEntries(paymentID));
        }

        //ONLINEPAYMENT
        public Pagamento GetPayment(int paymentID)
        {
            return dalPayment.GetItem(paymentID);
        }

        //ONLINEPAYMENT
        public Pagamento GetPaymentByIdPrenotazione(int prenotationID)
        {
            return dalPayment.GetItemByIdPrenotazione(prenotationID);
        }

        //ONLINEPAYMENT
        public bool CheckIfOnlinePaymentIsAllowed(out DateTime checkDate)
        {
            //LA CREAZIONE DELL'ORDINE DI PAGAMENTO E' CONSENTITA SE LA DATA DELLA VISITA
            //E' SUCCESSIVA ALLA DATA OTTENUTA AGGIUNGENDO ALLA DATA ATTUALE UN NUMERO
            //DI GIORNI INDICATI DAL PARAMETRO 'onlinePaymentDaysAfterAllowed'
            var visitDate = dalVisits.GetVListByIdPrenotazione(prenotationID)[0].Dt_Visita;
            int onlinePaymentDaysAfterAllowed = Convert.ToInt32(dalParameters.GetItem("onlinePaymentDaysAfterAllowed").Valore);

            checkDate = DateTime.Now.AddDays(onlinePaymentDaysAfterAllowed).Date;
            return (visitDate.Date > DateTime.Now.AddDays(onlinePaymentDaysAfterAllowed).Date);
        }

		#endregion// Public Methods

		#region Private Methods

		private PaymentCheckResult CheckTickets()
		{
			PaymentCheckResult result = new PaymentCheckResult();

			if (ObjPayment != null)
			{
				int interi = 0;
				int ridotti = 0;

				foreach (V_VisiteProgrammate v in Visits)
				{
					interi += v.Nr_Interi != null ? (int)v.Nr_Interi : 0;
					ridotti += v.Nr_Ridotti != null ? (int)v.Nr_Ridotti : 0;
				}

				result.DiffFullPrice = interi - (ObjPayment.Nr_InteriPagati != null ? (int)ObjPayment.Nr_InteriPagati : 0);
				result.DiffDiscount = ridotti - (ObjPayment.Nr_RidottiPagati != null ? (int)ObjPayment.Nr_RidottiPagati : 0);

			}

			return result;
		}


		#endregion// Private Methods

		#region Payment Methods

        //ONLINEPAYMENT
		private void LoadAvailablePaymentTypes(PaymentRange paymentRange)
		{
			List<LK_TipoPagamento> list = new List<LK_TipoPagamento>();
			List<LK_TipoPagamento> tmpList = new List<LK_TipoPagamento>();

			Pagamento_Dal dal = new Pagamento_Dal();

			List<int> paymentTypeIDs = new List<int>();
			switch (paymentRange)
			{
				case PaymentLib.PaymentRange.PerPrenotation:
                    var payment = dalPayment.GetItemByIdPrenotazione(prenotationID);
                    bool excludeOnLinePayment = !onlinePaymentEnabled;

                    //NON E' CONSENTITA LA MODIFICA DEL TIPO PAGAMENTO PASSANDO DA ALTRI
                    //TIPI AL TIPO ONLINE 
                     if (payment != null)
                         excludeOnLinePayment = (payment.Id_TipoPagamento != 1);
                     if (!excludeOnLinePayment)
                         paymentTypeIDs.AddRange(new List<int>() { 1, 2, 3, 4, 5 });
                     else
                         paymentTypeIDs.AddRange(new List<int>() { 2, 3, 4, 5 });
					break;

				case PaymentLib.PaymentRange.PerVisit:
					paymentTypeIDs.AddRange(new List<int>() { 3, 4, 5 });
					break;
			}

			tmpList.AddRange(dal.GetList());

			foreach (LK_TipoPagamento obj in tmpList)
				if (paymentTypeIDs.Contains(obj.Id_TipoPagamento))
					list.Add(obj);

			AvailablePaymentTypes = list;
		}

		public int CreatePaymentID()
		{
			return ObjPayments.Count > 0 ? ObjPayments.Max(x => x.ID) + 1 : 1;
		}

		#endregion// Payment Types Methods
	}
}
