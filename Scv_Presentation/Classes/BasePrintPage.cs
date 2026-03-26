using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Scv_Model;
using System.Data.Objects.DataClasses;
using System.ComponentModel;
using Telerik.Windows.Controls;
using System.Data;
using Scv_Entities;
using System.Configuration;
using Scv_Model;


namespace Presentation
{
	public class BasePrintPage : Window, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion //Events


        #region Private Fields

        private List<V_Movimento> dsWarehouseMovements = null;
		private List<V_Mandato> dsGuidesMovements = null;
		private List<V_MandatoGuida> dsGuidesPayroll = null;
		private List<GuidePayRollPrintItem> dsPayRollPrintItems = null;
		private List<Invoice> dsInvoices = null;
		private List<EmailPreviewItem> dsEmailPrint = null;
		private List<MandatoDettaglio> dsMandatoDettaglio = null;
		private List<V_Articoli> dsInventario = null;
		private List<V_GuideIncaricate> dsGuideIncaricate = null;
		private List<PrintMandatoArgs> dsPrintMandato = null;
        private List<V_TipoVisita_TipoBiglietto> dsPrintTipiVisitaImporti = null;
		private List<ReceiptVisitPage> dsPrintReceipt = null;
		private List<V_AssegnazioniGuideVisite> dsAssegnazioneGuideVisite = null;
		private List<V_SintesiArticoli> dsSintesiArticoli = null;
		private List<V_EvidenzeGiornaliere> dsEvidenzeGiornaliere = null;
        private List<V_TipoVisita_TipoBiglietto_Grouped> dsStats = null;
		private PrintGuidePayrollArgs printGuidePayrollArgs = null;
        private PrintGuidesMovementsArgs printGuidesMovementsArgs = null;
        private PrintDeliveredBillsArgs printDeliveredBillsArgs = null;
		private PrintMovementArgs printMovementArgs = null;
		private FirmaGuideArgs firmaGuideArgs = null;
		private PrintAssegnazioniGuideVisiteArgs printAssegnazioneGuideVisiteArgs = null;
		private List<PrintReceiptArgs> printReceiptArgs = null;

        #endregion// Private Fields


        #region Public Properties

        public List<V_Movimento> DsWarehouseMovements
		{
			get 
            {
                if (dsWarehouseMovements == null)
                    dsWarehouseMovements = new List<V_Movimento>();
                return dsWarehouseMovements; 
            }
			set { dsWarehouseMovements = value; OnPropertyChanged(this, "DsWarehouseMovements"); }
		}

		public List<V_Mandato> DsGuidesMovements
		{
			get 
            {
                if (dsGuidesMovements == null)
                    dsGuidesMovements = new List<V_Mandato>();
                return dsGuidesMovements; 
            }
			set { dsGuidesMovements = value; OnPropertyChanged(this, "DsGuidesMovements"); }
		}

		public List<V_MandatoGuida> DsGuidesPayroll
		{
			get 
            {
                if (dsGuidesPayroll == null)
                    dsGuidesPayroll = new List<V_MandatoGuida>();
                return dsGuidesPayroll;            
            }
			set { dsGuidesPayroll = value; OnPropertyChanged(this, "DsGuidesPayroll"); }
		}

		public List<GuidePayRollPrintItem> DsPayRollPrintItems
		{
			get
			{
				if (dsPayRollPrintItems == null)
					dsPayRollPrintItems = new List<GuidePayRollPrintItem>();
				return dsPayRollPrintItems;
			}
			set { dsPayRollPrintItems = value; OnPropertyChanged(this, "GuidePayRollPrintItem"); }
		}

		public List<Invoice> DsInvoices
		{
			get
			{
				if (dsInvoices == null)
					dsInvoices = new List<Invoice>();
				return dsInvoices;
			}
			set { dsInvoices = value; OnPropertyChanged(this, "DsInvoice"); }
		}

		public List<EmailPreviewItem> DsEmailPrint
		{
			get
			{
				if (dsEmailPrint == null)
					dsEmailPrint = new List<EmailPreviewItem>();
				return dsEmailPrint;
			}
			set { dsEmailPrint = value; OnPropertyChanged(this, "DsEmailPrint"); }
		}

		public List<MandatoDettaglio> DsMandatoDettaglio
		{
			get
			{
				if (dsMandatoDettaglio == null)
					dsMandatoDettaglio = new List<MandatoDettaglio>();
				return dsMandatoDettaglio;
			}
			set { dsMandatoDettaglio = value; OnPropertyChanged(this, "DsMandatoDettaglio"); }
		}

		public List<V_Articoli> DsInventario
		{
			get
			{
				if (dsInventario == null)
					dsInventario = new List<V_Articoli>();
				return dsInventario;
			}
			set { dsInventario = value; OnPropertyChanged(this, "DsInventario"); }
		}

		public List<V_GuideIncaricate> DsGuideIncaricate
		{
			get
			{
				if (dsGuideIncaricate == null)
					dsGuideIncaricate = new List<V_GuideIncaricate>();
				return dsGuideIncaricate;
			}
			set { dsGuideIncaricate = value; OnPropertyChanged(this, "DsGuideIncaricate"); }
		}

		public List<PrintMandatoArgs> DsPrintMandato
		{
			get
			{
				if (dsPrintMandato == null)
					dsPrintMandato = new List<PrintMandatoArgs>();
				return dsPrintMandato;
			}
			set { dsPrintMandato = value; }
		}

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

		public List<ReceiptVisitPage> DsPrintReceipt
		{
			get
			{
				if (dsPrintReceipt == null)
					dsPrintReceipt = new List<ReceiptVisitPage>();
				return dsPrintReceipt;
			}
			set { dsPrintReceipt = value; }
		}

		public List<V_AssegnazioniGuideVisite> DsAssegnazioneGuideVisite
		{
			get
			{
				if (dsAssegnazioneGuideVisite == null)
					dsAssegnazioneGuideVisite = new List<V_AssegnazioniGuideVisite>();
				return dsAssegnazioneGuideVisite;
			}
			set { dsAssegnazioneGuideVisite = value; }
		}

		public List<V_SintesiArticoli> DsSintesiArticoli
		{
			get
			{
				if (dsSintesiArticoli == null)
					dsSintesiArticoli = new List<V_SintesiArticoli>();
				return dsSintesiArticoli;
			}
			set { dsSintesiArticoli = value; }
		}

		public List<V_EvidenzeGiornaliere> DsEvidenzeGiornaliere
		{
			get
			{
				if (dsEvidenzeGiornaliere == null)
					dsEvidenzeGiornaliere = new List<V_EvidenzeGiornaliere>();
				return dsEvidenzeGiornaliere;
			}
			set { dsEvidenzeGiornaliere = value; }
		}

        public List<V_TipoVisita_TipoBiglietto_Grouped> DsStats
        {
            get
            {
                if (dsStats == null)
                    dsStats = new List<V_TipoVisita_TipoBiglietto_Grouped>();
                return dsStats;
            }
            set { dsStats = value; }
        }

        public string StatsAnni = "";

        public string StatsMesi = "";

        public string StatsTipiVisite = "";

		public PrintGuidePayrollArgs PrintGuidePayrollArgs
		{
			get
			{
				if (printGuidePayrollArgs == null)
					printGuidePayrollArgs = new PrintGuidePayrollArgs();
				return printGuidePayrollArgs;
			}
			set { printGuidePayrollArgs = value; }
		}

        public PrintGuidesMovementsArgs PrintGuidesMovementsArgs
        {
            get
            {
                if (printGuidesMovementsArgs == null)
                    printGuidesMovementsArgs = new PrintGuidesMovementsArgs();
                return printGuidesMovementsArgs;
            }
            set { printGuidesMovementsArgs = value; }
        }

        public PrintDeliveredBillsArgs PrintDeliveredBillsArgs
        {
            get
            {
                if (printDeliveredBillsArgs == null)
                    printDeliveredBillsArgs = new PrintDeliveredBillsArgs();
                return printDeliveredBillsArgs;
            }
            set { printDeliveredBillsArgs = value; }
        }

        public PrintMovementArgs PrintMovementArgs
        {
            get
            {
                if (printMovementArgs == null)
                    printMovementArgs = new PrintMovementArgs();
                return printMovementArgs;
            }
            set { printMovementArgs = value; }
        }

		public FirmaGuideArgs FirmaGuideArgs
		{
			get
			{
				if (firmaGuideArgs == null)
					firmaGuideArgs = new FirmaGuideArgs();
				return firmaGuideArgs;
			}
			set { firmaGuideArgs = value; }
		}

		public PrintAssegnazioniGuideVisiteArgs PrintAssegnazioneGuideVisiteArgs
		{
			get
			{
				if (printAssegnazioneGuideVisiteArgs == null)
					printAssegnazioneGuideVisiteArgs = new PrintAssegnazioniGuideVisiteArgs();
				return printAssegnazioneGuideVisiteArgs;
			}
			set { printAssegnazioneGuideVisiteArgs = value; }
		}

		public List<PrintReceiptArgs> PrintReceiptArgs
		{
			get
			{
				if (printReceiptArgs == null)
					printReceiptArgs = new List<PrintReceiptArgs>();
				return printReceiptArgs;
			}
			set { printReceiptArgs = value; }
		}

        private Pagamento objPagamento = new Pagamento();
        public Pagamento ObjPagamento
        {
            get
            {
                return objPagamento;
            }
            set 
            { 
                objPagamento = value; 
            }
        }


		#endregion // PublicProperties


		#region Constructors

		public BasePrintPage()
        {
            this.Topmost = Convert.ToBoolean(ConfigurationManager.AppSettings["windTopMost"]);
        }

		#endregion // Constructors


		#region Event handlers


		#endregion // Event handlers



		protected void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}
	}
}
