using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Scv_Entities;
using Scv_Dal;
using Presentation.CustomControls.PaymentLib;

namespace Presentation
{
	class RefundViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events



		#region Private Fields

		private string objVisitReceipt = string.Empty;

		private string objPrenotationProtocol = string.Empty;

		private decimal objPaymentAmount = 0;

		private Movimento objMovement = null;

		private int objPaymentID = 0;

		private PaymentRange paymentRange = PaymentRange.PerVisit;

		private List<V_VisiteProgrammate> visits = null;

		private string visitInfo = string.Empty;

		public string VisitInfo
		{
			get { return visitInfo; }
			set { visitInfo = value; OnPropertyChanged(this, "visitInfo"); }
		}

		private Pagamento objPayment = null;

		#endregion// Private Fields



		#region Public Properties

		public string ObjVisitReceipt
		{
			get { return objVisitReceipt; }
			set { objVisitReceipt = value; OnPropertyChanged(this, "ObjVisitReceipt"); }
		}

		public string ObjPrenotationProtocol
		{
			get { return objPrenotationProtocol; }
			set { objPrenotationProtocol = value; OnPropertyChanged(this, "ObjPrenotationProtocol"); }
		}

		public decimal ObjPaymentAmount
		{
			get { return objPaymentAmount; }
			set { objPaymentAmount = value; OnPropertyChanged(this, "ObjPaymentAmount"); }
		}

		public Movimento ObjMovement
		{
			get { return objMovement; }
			set { objMovement = value; OnPropertyChanged(this, "ObjMovement"); }
		}

		public int ObjPaymentID
		{
			get { return objPaymentID; }
			set { objPaymentID = value; OnPropertyChanged(this, "ObjPaymentID"); }
		}

		public PaymentRange PaymentRange
		{
			get { return paymentRange; }
			set { paymentRange = value; OnPropertyChanged(this, "PaymentRange"); }
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

		#endregion// Public Properties



		#region Dal

		Pagamento_Dal dalPayment = new Pagamento_Dal();

		VisitaProgrammata_Dal dalVisits = new VisitaProgrammata_Dal();

		Parametri_Dal dalParameters = new Parametri_Dal();

		Prenotazione_Dal dalPrenotation = new Prenotazione_Dal();

		#endregion// Dal



		#region Constructors

		public RefundViewModel(int paymentID)
		{
			this.ObjPaymentID = paymentID;
			ObjMovement = new Movimento();
			ObjMovement.Dt_Movimento = DateTime.Now.Date;
			ObjMovement.Id_TipoMovimento = 14; //Rimborso
			ObjMovement.Id_TipoPagamento = 3; //Contanti
			ObjMovement.ValidateAuthorization = true;

			LoadData(paymentID);
		}

		#endregion// Constructors



		#region Private Methods

		private void LoadData(int paymentID)
		{
			Pagamento p = dalPayment.GetItem(paymentID);
			if (p != null)
			{
				Prenotazione pr = dalPrenotation.GetItem(p.Id_Prenotazione);
				if (pr != null)
					ObjPrenotationProtocol = pr.Protocollo;

				if (p.Id_VisitaProgrammata != null)
				{
					Visits.Add((dalVisits.GetVListById((int)p.Id_VisitaProgrammata)));
					VisitInfo = "Visita ore: " + Visits[0].Ora_Visita + ", lingua: " + Visits[0].LinguaVisita;
				}
				else
				{
					if (pr != null)
					{
						Visits.AddRange(dalVisits.GetVListByIdPrenotazione(pr.Id_Prenotazione));
						VisitInfo = Visits.Count.ToString() + " visite";
					}
				}

				ObjVisitReceipt = p.Ricevuta;

				ObjMovement.PrezzoVendita = p.Importo != null ? (decimal)p.Importo : 0;

				ObjPaymentAmount = p.Importo != null ? (decimal)p.Importo : 0;
			}
		}

		#endregion// Private Methods



		#region Event Handling

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handling

	}
}
