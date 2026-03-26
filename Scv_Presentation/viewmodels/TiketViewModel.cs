using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Scv_Dal;
using Scv_Entities;
using Scv_Model;
using Presentation.CustomControls.PaymentLib;

namespace Presentation
{
	public class TicketViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events

		#region Private Fields

		private VisitaProgrammata_Dal dalScheduledVisit = new VisitaProgrammata_Dal();
		private VisitaPrenotata_Dal dalPrenotatedVisit = new VisitaPrenotata_Dal();
		private Prenotazione_Dal dalPrenotation = new Prenotazione_Dal();
		private Pagamento_Dal dalPayment = new Pagamento_Dal();
        private Parametri_Dal dalParametri = new Parametri_Dal();
        private Biglietto_Dal dalBiglietto = new Biglietto_Dal();
       

		private int prenotatedFullPriceTickets = 0;

		private int prenotatedReducedTickets = 0;

        private int prenotatedDiscountTickets = 0;

        private int prenotatedCumulativeTickets = 0;

		private int prenotatedFreeTickets = 0;

		private int deliverableFullPriceTickets = 0;

		private int deliverableReducedTickets = 0;

        private int deliverableDiscountTickets = 0;

        private int deliverableCumulativeTickets = 0;

		private int deliverableFreeTickets = 0;

		private int deliveredFullPriceTickets = 0;

		private int deliveredReducedTickets = 0;

        private int deliveredDiscountTickets = 0;

        private int deliveredCumulativeTickets = 0;

		private int deliveredFreeTickets = 0;

		private int totalDeliverableTickets = 0;

		private decimal toPay = 0;

		private string date = string.Empty;

		private string hour = string.Empty;

		private int ticketsStartNumber = 0;

		private int ticketsEndNumber = 0;

		private int ticketMinNumber = 0;

		private string protocol = string.Empty;

		#endregion// Private Fields

		#region Public Properties
		public int PrenotatedFullPriceTickets
		{
			get { return prenotatedFullPriceTickets; }
			set { prenotatedFullPriceTickets = value; OnPropertyChanged(this, "PrenotatedFullPriceTickets"); }
		}

		public int PrenotatedReducedTickets
		{
			get { return prenotatedReducedTickets; }
			set { prenotatedReducedTickets = value; OnPropertyChanged(this, "PrenotatedReducedTickets"); }
		}

        public int PrenotatedDiscountTickets
        {
            get { return prenotatedDiscountTickets; }
            set { prenotatedDiscountTickets = value; OnPropertyChanged(this, "PrenotatedDiscountTickets"); }
        }

        public int PrenotatedCumulativeTickets
        {
            get { return prenotatedCumulativeTickets; }
            set { prenotatedCumulativeTickets = value; OnPropertyChanged(this, "PrenotatedCumulativeTickets"); }
        }

		public int PrenotatedFreeTickets
		{
			get { return prenotatedFreeTickets; }
			set { prenotatedFreeTickets = value; OnPropertyChanged(this, "PrenotatedFreeTickets"); }
		}

		public int DeliverableFullPriceTickets
		{
			get { return deliverableFullPriceTickets; }
			set { deliverableFullPriceTickets = value; OnPropertyChanged(this, "DeliverableFullPriceTickets"); }
		}

		public int DeliverableReducedTickets
		{
			get { return deliverableReducedTickets; }
			set { deliverableReducedTickets = value; OnPropertyChanged(this, "DeliverableReducedTickets"); }
		}

        public int DeliverableDiscountTickets
        {
            get { return deliverableDiscountTickets; }
            set { deliverableDiscountTickets = value; OnPropertyChanged(this, "DeliverableDiscountTickets"); }
        }

        public int DeliverableCumulativeTickets
        {
            get { return deliverableCumulativeTickets; }
            set { deliverableCumulativeTickets = value; OnPropertyChanged(this, "DeliverableCumulativeTickets"); }
        }

		public int DeliverableFreeTickets
		{
			get { return deliverableFreeTickets; }
			set { deliverableFreeTickets = value; OnPropertyChanged(this, "DeliverableFreeTickets"); }
		}

		public int DeliveredFullPriceTickets
		{
			get { return deliveredFullPriceTickets; }
			set { deliveredFullPriceTickets = value; OnPropertyChanged(this, "DeliveredFullPriceTickets"); }
		}

		public int DeliveredReducedTickets
		{
            get { return deliveredReducedTickets; }
            set { deliveredReducedTickets = value; OnPropertyChanged(this, "DeliveredReducedTickets"); }
		}

        public int DeliveredDiscountTickets
        {
            get { return deliveredDiscountTickets; }
            set { deliveredDiscountTickets = value; OnPropertyChanged(this, "DeliveredDiscountTickets"); }
        }

        public int DeliveredCumulativeTickets
        {
            get { return deliveredCumulativeTickets; }
            set { deliveredCumulativeTickets = value; OnPropertyChanged(this, "DeliveredCumulativeTickets"); }
        }

		public int DeliveredFreeTickets
		{
			get { return deliveredFreeTickets; }
			set { deliveredFreeTickets = value; OnPropertyChanged(this, "DeliveredFreeTickets"); }
		}

		public int TotalPrenotatedTickets
		{
			get { return PrenotatedFullPriceTickets + PrenotatedReducedTickets + PrenotatedFreeTickets + PrenotatedDiscountTickets + PrenotatedCumulativeTickets; }
		}

		public int TotalDeliverableTickets
		{
			get { return totalDeliverableTickets; }
			set { totalDeliverableTickets = value; OnPropertyChanged(this, "TotalDeliverableTickets"); }
		}

		public int TotalDeliveredTickets
		{
            get { return DeliveredFullPriceTickets + DeliveredReducedTickets + DeliveredFreeTickets + DeliveredDiscountTickets + DeliveredCumulativeTickets; }
		}

		public decimal ToPay
		{
			get { return toPay; }
			set { toPay = value; OnPropertyChanged(this, "ToPay"); }
		}

		public string Date
		{
			get { return date; }
			set { date = value; OnPropertyChanged(this, "Date"); }
		}

		public string Hour
		{
			get { return hour; }
			set { hour = value; OnPropertyChanged(this, "Hour"); }
		}

		public int TicketsStartNumber
		{
            get { return ticketsStartNumber; }
            set { ticketsStartNumber = value; OnPropertyChanged(this, "TicketsStartNumber"); }
		}

        public int TicketsEndNumber
        {
            get { return ticketsEndNumber; }
            set { ticketsEndNumber = value; OnPropertyChanged(this, "TicketsEndNumber"); }
        }

        public decimal CostoBigliettoIntero = 0;

        public decimal CostoBigliettoRidotto = 0;

        public decimal CostoBigliettoScontato = 0;

        public decimal CostoBigliettoCumulativo = 0;

		public string Protocol
		{
			get { return protocol; }
			set { protocol = value; }
		}

        public string DescTipoVisita = "";

		#endregion// Public Properties

		#region Constructor

		public TicketViewModel(int detailID)
		{
			LoadData(detailID);
		}

		#endregion

		#region Event Handling

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

            //switch (propertyName)
            //{
            //    case "DeliverableFullPriceTickets":
            //        SetTicketNumbers("TI", DeliverableFullPriceTickets);
            //        break;
            //    case "DeliverableReducedTickets":
            //        SetTicketNumbers("TR", DeliverableReducedTickets);
            //        break;
            //    case "DeliverableDiscountTickets":
            //        SetTicketNumbers("TD", DeliverableDiscountTickets);
            //        break;
            //    case "DeliverableCumulativeTickets":
            //        SetTicketNumbers("TC", DeliverableCumulativeTickets);
            //        break;
            //    case "DeliverableFreeTickets":
            //        SetTicketNumbers("TO", DeliverableFreeTickets);
            //        break;
            //}
		}

		#endregion// Event Handling



		#region Methods

		public void LoadData(int detailID)
		{
			VisitaProgrammata vp = dalScheduledVisit.GetItem(detailID);
			LK_Progressivi_Dal dalProgressives = new LK_Progressivi_Dal();

			PrenotatedFullPriceTickets = 0;
			PrenotatedReducedTickets = 0;
            PrenotatedDiscountTickets = 0;
            PrenotatedCumulativeTickets = 0;
			PrenotatedFreeTickets = 0;

			DeliveredFullPriceTickets = 0;
			DeliveredReducedTickets = 0;
            DeliveredDiscountTickets = 0;
            DeliveredCumulativeTickets = 0;
			DeliveredFreeTickets = 0;

			if (vp != null)
			{
				VisitaPrenotata v = dalPrenotatedVisit.GetItem(vp.Id_VisitaPrenotata);
				if (v != null)
				{
					Prenotazione pr = dalPrenotation.GetItem(v.Id_Prenotazione);
                    var tipoVisita = dalParametri.GetTipoVisitaByIdPrenotazione(pr.Id_Prenotazione);
                    DescTipoVisita = tipoVisita.Descrizione;
                    CostoBigliettoIntero = tipoVisita.PrezzoIntero != null ? (decimal)tipoVisita.PrezzoIntero : 0;
                    CostoBigliettoRidotto = tipoVisita.PrezzoRidotto != null ? (decimal)tipoVisita.PrezzoRidotto : 0;
                    CostoBigliettoScontato = tipoVisita.PrezzoScontato != null ? (decimal)tipoVisita.PrezzoScontato : 0;
                    CostoBigliettoCumulativo = tipoVisita.PrezzoCumulativo != null ? (decimal)tipoVisita.PrezzoCumulativo : 0;
 
					if (pr != null)
					{
						Protocol = pr.Protocollo;
						LK_Progressivi pg = dalProgressives.GetProgressiviBySymbol("TK");
						if (pg != null)
						{
                            TicketsStartNumber = pg.Progr_UltimoUscito;
						}

						PrenotatedFullPriceTickets += vp.Nr_Interi != null ? (int)vp.Nr_Interi : 0;
						PrenotatedReducedTickets += vp.Nr_Ridotti != null ? (int)vp.Nr_Ridotti : 0;
                        PrenotatedDiscountTickets += vp.Nr_Scontati != null ? (int)vp.Nr_Scontati : 0;
                        PrenotatedCumulativeTickets += vp.Nr_Cumulativi != null ? (int)vp.Nr_Cumulativi : 0;
                        PrenotatedFreeTickets += vp.Nr_Omaggio != null ? (int)vp.Nr_Omaggio : 0;

						DeliveredFullPriceTickets += vp.Nr_InteriConsegnati != null ? (int)vp.Nr_InteriConsegnati : 0;
						DeliveredReducedTickets += vp.Nr_RidottiConsegnati != null ? (int)vp.Nr_RidottiConsegnati : 0;
                        DeliveredDiscountTickets += vp.Nr_ScontatiConsegnati != null ? (int)vp.Nr_ScontatiConsegnati : 0;
                        DeliveredCumulativeTickets += vp.Nr_CumulativiConsegnati != null ? (int)vp.Nr_CumulativiConsegnati : 0;
						DeliveredFreeTickets += vp.Nr_OmaggioConsegnati != null ? (int)vp.Nr_OmaggioConsegnati : 0;

						DeliverableFullPriceTickets = PrenotatedFullPriceTickets - DeliveredFullPriceTickets;
						DeliverableReducedTickets = PrenotatedReducedTickets - DeliveredReducedTickets;
                        DeliverableDiscountTickets = PrenotatedDiscountTickets - DeliveredDiscountTickets;
                        DeliverableCumulativeTickets = PrenotatedCumulativeTickets - DeliveredCumulativeTickets;
						DeliverableFreeTickets = PrenotatedFreeTickets - DeliveredFreeTickets;

						Pagamento pa = dalPayment.GetItemByIdPrenotazione(pr.Id_Prenotazione);
                        if (pa == null)
                            pa = dalPayment.GetItemByIdVisitaProgrammata(detailID);


						PaymentRange prg = PaymentRange.PerVisit;

						ToPay = 1;

						if (pa != null && pa.Dt_Pagamento != null)
						{

							if (pa.Id_VisitaProgrammata == null)
								prg = PaymentRange.PerPrenotation;

							switch (prg)
							{
								case PaymentRange.PerPrenotation:
									ToPay = dalScheduledVisit.GetDelta(pr.Protocollo, (pa != null ? (pa.Importo != null ? (decimal)pa.Importo : 0) : 0));
									break;

								case PaymentRange.PerVisit:
                                    decimal importo = ((vp.Nr_Interi != null ? (decimal)vp.Nr_Interi * CostoBigliettoIntero : 0) + (vp.Nr_Ridotti != null ? (decimal)vp.Nr_Ridotti * CostoBigliettoRidotto : 0) + (vp.Nr_Scontati != null ? (decimal)vp.Nr_Scontati * CostoBigliettoScontato : 0) + (vp.Nr_Cumulativi != null ? CostoBigliettoCumulativo : 0));
                                    if (pa != null)
										ToPay = (int)pa.Importo - importo;
									else
                                        ToPay = importo;
									break;
							}

						}
						Date = vp.Dt_Visita != null ? vp.Dt_Visita.Date.ToShortDateString() : string.Empty;
						Hour = vp.Ora_Visita;
					}
				}
			}
		}

		#endregion// Private Methods

		#region Public Methods

		public TicketNumberResult UpdateData()
		{
			TicketNumberResult result = new TicketNumberResult();

			if (DeliverableFullPriceTickets > (PrenotatedFullPriceTickets + DeliveredFullPriceTickets))
			{
				result.Success = false;
				result.Diff = DeliverableFullPriceTickets - (PrenotatedFullPriceTickets + DeliveredFullPriceTickets);
				result.Message += "\n- Il numero di biglietti interi da consegnare fa superare di " + result.Diff.ToString() + " il numero di biglietti interi prenotati.";
			}

			if (DeliverableReducedTickets > (PrenotatedReducedTickets + DeliveredReducedTickets))
			{
				result.Success = false;
				result.Diff = DeliverableReducedTickets - (PrenotatedReducedTickets + DeliveredReducedTickets);
				result.Message += "\n- Il numero di biglietti ridotti da consegnare fa superare di " + result.Diff.ToString() + " il numero di biglietti ridotti prenotati.";
			}

			if (DeliverableFreeTickets > (PrenotatedFreeTickets + DeliveredFreeTickets))
			{
				result.Success = false;
				result.Diff = DeliverableFreeTickets - (PrenotatedFreeTickets + DeliveredFreeTickets);
				result.Message += "\n- Il numero di biglietti omaggio da consegnare fa superare di " + result.Diff.ToString() + " il numero di biglietti omaggio prenotati.";
			}

			return result;
		}

        public void AddTicket(long code, int idVisitaProgrammata, int idTipoBiglietto, int pax)
        {
            Biglietto ticket = new Biglietto();
            ticket.Codice = code;
            ticket.Id_VisitaProgrammata = idVisitaProgrammata;
            ticket.Id_TipoBiglietto = idTipoBiglietto;
            ticket.Pax = pax;
            ticket.Passed = 0;
            ticket.DataOraEmissione = DateTime.Now;
            dalBiglietto.InsertOrUpdate(ticket);
        }

		#endregion// Public Methods
	} 

}
