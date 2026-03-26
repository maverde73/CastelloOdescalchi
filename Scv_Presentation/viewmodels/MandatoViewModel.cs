using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Scv_Dal;
using System.Collections.ObjectModel;
using Scv_Entities;
using Telerik.Windows.Data;
using System.Windows;
using Scv_Model;
using System.Globalization;
using System.Configuration;
using System.Windows;

namespace Presentation
{
	public class MandatoViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events


		#region Properties

		bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
        //ONLINEPAYMENT
        bool onlinePaymentEnabled = false;
        public bool OnlinePaymentEnabled
        {
            get { return onlinePaymentEnabled; }
            set { onlinePaymentEnabled = value; OnPropertyChanged(this, "OnlinePaymentEnabled"); }
        }
		
        private Mandato_Dal dalMandato = new Mandato_Dal();

        private Mandato objMandato = null;
		public Mandato ObjMandato
		{
            get { return objMandato; }
			set { objMandato = value; OnPropertyChanged(this, "ObjMandato"); }
		}

		private ObservableCollection<MandatoDettaglio> objMandatoDettaglio = null;
		public ObservableCollection<MandatoDettaglio> ObjMandatoDettaglio
		{
			get
			{ return objMandatoDettaglio; }
			set { objMandatoDettaglio = value; OnPropertyChanged(this, "ObjMandatoDettaglio"); }
		}

		private DateTime? objUltimoMandato = (DateTime?)null;
		public DateTime? ObjUltimoMandato
		{
			get { return objUltimoMandato; }
			set { objUltimoMandato = value; OnPropertyChanged(this, "ObjUltimoMandato"); }
		}

		private DateTime? objSelectedDate = (DateTime?)null;
		public DateTime? ObjSelectedDate
		{
			get { return objSelectedDate; }
			set { objSelectedDate = value; OnPropertyChanged(this, "ObjSelectedDate"); }
		}

		private decimal objMaxNumero = 0;
		public decimal ObjMaxNumero
		{
			get { return objMaxNumero; }
			set { objMaxNumero = value; OnPropertyChanged(this, "ObjMaxNumero"); }
		}

		private decimal objMaxValore = 0;
		public decimal ObjMaxValore
		{
			get { return objMaxValore; }
			set { objMaxValore = value; OnPropertyChanged(this, "ObjMaxValore"); }
		}

		private PrintMandatoArgs args = null;
		public PrintMandatoArgs Args
		{
			get
			{
				if (args == null)
					args = new PrintMandatoArgs();
				return args;
			}
			set { args = value; OnPropertyChanged(this, "Args"); }
		}

        List<V_TipoVisita_TipoBiglietto> dsPrintTipiVisitaImporti = null;
        public List<V_TipoVisita_TipoBiglietto> DsPrintTipiVisitaImporti
        {
            get
            {
                if (dsPrintTipiVisitaImporti == null)
                    dsPrintTipiVisitaImporti = new List<V_TipoVisita_TipoBiglietto>();
                return dsPrintTipiVisitaImporti;
            }
            set { dsPrintTipiVisitaImporti = value; }
        }

        private System.Windows.Visibility btnRefreshOPVisibility = System.Windows.Visibility.Hidden;
        public System.Windows.Visibility BtnRefreshOPVisibility
        {
            get { return btnRefreshOPVisibility; }
            set { btnRefreshOPVisibility = value; OnPropertyChanged(this, "BtnRefreshOPVisibility");}
        }

        private System.Windows.Visibility btnUpdateVisibility = System.Windows.Visibility.Hidden;
        public System.Windows.Visibility BtnUpdateVisibility
        {
            get { return btnUpdateVisibility; }
            set { btnUpdateVisibility = value; OnPropertyChanged(this, "BtnUpdateVisibility"); }
        }
        


		#endregion // Properties


		#region Constructors

		public MandatoViewModel(DateTime date)
		{
            if (ConfigurationManager.AppSettings["enableonlinepayment"] != null)
                OnlinePaymentEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["enableonlinepayment"]);

            ObjSelectedDate = date;
		}

		#endregion// Constructors


		#region Event Handling

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handling


		#region Main Methods

		public void LoadData(DateTime date)
		{
			ObjMandato = dalMandato.GetMandato(DateTime.Parse(date.ToShortDateString()));
			LoadDetails();
            ObjUltimoMandato = dalMandato.GetLastDateMandato(date);
			LoadArgs();

            DsPrintTipiVisitaImporti = dalMandato.GetTipiVisiteImporti(date);
            BtnRefreshOPVisibility = System.Windows.Visibility.Hidden;
            BtnUpdateVisibility = System.Windows.Visibility.Hidden;

			if (ObjMandato == null)
			{
				if (date < dalMandato.GetLastDateMandato(null))
				{
					MessageBox.Show("Data mandato inesistente. Inserire una data valida.");
				}
				else if (date > DateTime.Now.Date)
				{
					MessageBox.Show("Impossibile creare un mandato in data futura.");
				}
				else
				{
					string msg = string.Empty;
					if (ObjUltimoMandato != null)
						msg = "Ultimo mandato elaborato in data " + ObjUltimoMandato.Value.ToShortDateString() + ".\n";

					msg += "Si vuole creare un mandato in data " + date.ToShortDateString() + "?";

					if (MessageBox.Show(msg, "Creazione mandato", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						Mandato obj = new Mandato();
						obj.Dt_Mandato = DateTime.Parse(date.ToShortDateString());
                        int id = dalMandato.InsertOrUpdate(obj, new List<MandatoDettaglio>(), ObjSelectedDate.Value.Date, OnlinePaymentEnabled);
						ObjMandato = dalMandato.GetMandato(DateTime.Parse(date.ToShortDateString()));
						LoadDetails();
					}
				}
			}
			else
			{
                BtnUpdateVisibility = System.Windows.Visibility.Visible;
                if (ObjSelectedDate == dalMandato.GetLastDateMandato(null))
                {
                    //L'ULTIMO MANDATO EFFETTUATO E' QUELLO CORRENTE.
                    //SI PUO' AGGIORNARE IL MANDATO IN RIFERIMENTO 
                    //AI PAGAMENTI ONLINE
                   BtnRefreshOPVisibility = System.Windows.Visibility.Visible;
                }
                else
                   BtnRefreshOPVisibility = System.Windows.Visibility.Hidden;


				if (Args.taglioValTotale != Args.TotaleCassa)
				{
					MessageBox.Show("Totale cassa (contanti e assegni) euro " + Args.taglioValTotale.ToString() + " diverso da Sintesi incassi euro " + Args.TotaleCassa.ToString() + ". E' necessario modificare i tagli!", "Errore tagli", MessageBoxButton.OK);
				}
			}
		}

		private void LoadDetails()
		{
            if (ObjMandato != null)
                ObjMandatoDettaglio = dalMandato.GetMandatoDettaglioList(ObjMandato.Id_Mandato, ObjMandato.Dt_Mandato);
            else
            {
                if(ObjMandatoDettaglio != null)
                    ObjMandatoDettaglio.Clear();
            }


		}

		public void LoadArgs()
		{
			Mandato_Dal dal = new Mandato_Dal();

			Args = new PrintMandatoArgs();

            if (ObjMandato != null)
                Args.IdMandato = ObjMandato.Id_Mandato;
            else
                Args.IdMandato = 0;

            Args.OnlinePaymentEnabled = OnlinePaymentEnabled;

			Args = dal.GetTotaleBiglietti(Args, DateTime.Parse(ObjSelectedDate.Value.ToShortDateString()));
			Args = dal.GetBigliettiIncassati(Args, DateTime.Parse(ObjSelectedDate.Value.ToShortDateString()));
			Args = dal.GetBigliettiRimborsati(Args, DateTime.Parse(ObjSelectedDate.Value.ToShortDateString()));
			Args = dal.GetVenditaArticoli(Args, DateTime.Parse(ObjSelectedDate.Value.ToShortDateString()));
			Args = dal.GetAltroArticoli(Args, DateTime.Parse(ObjSelectedDate.Value.ToShortDateString()));
			Args = dal.GetDepositoAnticipi(Args, DateTime.Parse(ObjSelectedDate.Value.ToShortDateString()));
			Args = dal.GetBigliettiPrelevati(Args, DateTime.Parse(ObjSelectedDate.Value.ToShortDateString()));
			Args = dal.GetBigliettiRinunciati(Args, DateTime.Parse(ObjSelectedDate.Value.ToShortDateString()));
			Args = dal.GetRimanenza(Args, DateTime.Parse(ObjSelectedDate.Value.ToShortDateString()));
			Args = dal.GetTagli(Args, DateTime.Parse(ObjSelectedDate.Value.ToShortDateString()));			
			Args.NrProgressivo = ObjMandato != null ? ObjMandato.Nr_Mandato : 0;
            Args.DtMandato = ObjSelectedDate.Value.ToShortDateString();

			CultureInfo culture = CultureInfo.CurrentCulture;

			decimal interi = 0;
			decimal ridotti = 0;

			decimal.TryParse(new Parametri_Dal().GetItem("biglietto_intero").Valore, out interi);
			decimal.TryParse(new Parametri_Dal().GetItem("biglietto_ridotto").Valore, out ridotti);

			Args.LblInteri = "- Interi (" + interi.ToString("C") + ")";
			Args.LblRidotti = "- Ridotti (" + ridotti.ToString("C") + ")";
		}

		#endregion // Main Methods


		#region Details Methods


		#endregion// Details Methods
	}
}
