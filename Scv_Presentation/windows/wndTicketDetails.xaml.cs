using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Configuration;
using Scv_Dal;
using Scv_Entities;
using ShiftManager;
using System.Data.Objects.DataClasses;
using Scv_Model;
using Telerik.Windows.Controls;
using Thera.Biglietteria.Boca;
using System.IO;
using System.Threading;
using Presentation.LocalPrinter;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for wndTicketDetails.xaml
	/// </summary>
	public partial class wndTicketDetails : BaseDetailPage, INotifyPropertyChanged
	{

		#region Events

		#endregion


		#region Private Fields

		TicketViewModel tVM = null;

        IBocaPrinter _printer = null;

        bool usePrinter = false;

        bool printerFound = true;

		#endregion// Private fields


		#region Public Properties


		#endregion // Properties


		#region Constructors

		public wndTicketDetails(int detailID, LK_User user)
			: base(detailID)
		{
			InitializeComponent();

            Unloaded += new RoutedEventHandler(OnUnloaded);

			this.User = user;

            usePrinter = Convert.ToBoolean(ConfigurationManager.AppSettings["usePrinter"]);

			tVM = new TicketViewModel(this.DetailID);
			this.DataContext = tVM;

			VisitaPrenotata_Dal dalPrenotedVisit = new VisitaPrenotata_Dal();
			VisitaProgrammata_Dal dalScheduled = new VisitaProgrammata_Dal();
			Pagamento_Dal dalPayment = new Pagamento_Dal();
			VisitaProgrammata vScheduled = dalScheduled.GetItem(DetailID);

			if (tVM.ToPay > 0)
			{
				DoOpenVisit(DetailID);
				tVM.LoadData(DetailID);
			}

			SetBehavior();

			nudFullPriceTickets.ValueChanged += new EventHandler<Telerik.Windows.Controls.RadRangeBaseValueChangedEventArgs>(nudTickets_ValueChanged);
			nudReducedTickets.ValueChanged += new EventHandler<Telerik.Windows.Controls.RadRangeBaseValueChangedEventArgs>(nudTickets_ValueChanged);
			nudFreeTickets.ValueChanged += new EventHandler<Telerik.Windows.Controls.RadRangeBaseValueChangedEventArgs>(nudTickets_ValueChanged);

			btnPrint.Click += new RoutedEventHandler(btnPrint_Click);
			btnOpenVisit.Click += new RoutedEventHandler(btnOpenVisit_Click);
		}

		#endregion // Constructors


		#region Event Handling

		private void btnPrint_Click(object sender, RoutedEventArgs e)
		{
			string message = string.Empty;

			if (
				    tVM.DeliverableFullPriceTickets == 0
				    &&
				    tVM.DeliverableReducedTickets == 0
				    &&
				    tVM.DeliverableFreeTickets == 0
			    )
			{
				MessageBox.Show("Impossibile stampare: non è stato inserito alcun biglietto da consegnare.", "Errore numero biglietti", MessageBoxButton.OK);
				return;
			}

			if (tVM.DeliverableFullPriceTickets < (tVM.PrenotatedFullPriceTickets - tVM.DeliveredFullPriceTickets))
				message += "\n- I biglietti interi da consegnare sono inferiori al totale della visita.";

			if (tVM.DeliverableReducedTickets < (tVM.PrenotatedReducedTickets - tVM.DeliveredReducedTickets))
				message += "\n- I biglietti ridotti da consegnare sono inferiori al totale della visita.";

			if (tVM.DeliverableFreeTickets < (tVM.PrenotatedFreeTickets - tVM.DeliveredFreeTickets))
				message += "\n- I biglietti omaggio da consegnare sono inferiori al totale della visita.";

			if (message.Length > 0)
			{
				message = message + "\nCONTINUARE?";
				if (MessageBox.Show(message, "Avviso numero biglietti", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
					return;
			}

			List<PrintableTicket> FullPriceTicketlist = new List<PrintableTicket>();
            List<PrintableTicket> ReducedTicketlist = new List<PrintableTicket>();
			List<PrintableTicket> DiscountTicketlist = new List<PrintableTicket>();
            List<PrintableTicket> CumulativeTicketlist = new List<PrintableTicket>();
			List<PrintableTicket> FreeTicketlist = new List<PrintableTicket>();
			int ticketNumber = tVM.TicketsStartNumber;


			for (int i = 0; i < tVM.DeliverableFullPriceTickets; i++)
                FullPriceTicketlist.Add(new PrintableTicket(TicketType.FullPrice, ticketNumber++, tVM.Protocol));

			for (int i = 0; i < tVM.DeliverableReducedTickets; i++)
                ReducedTicketlist.Add(new PrintableTicket(TicketType.Reduced, ticketNumber++, tVM.Protocol));

            for (int i = 0; i < tVM.DeliverableDiscountTickets; i++)
                DiscountTicketlist.Add(new PrintableTicket(TicketType.Discount, ticketNumber++, tVM.Protocol));

            //for (int i = 0; i < tVM.DeliverableCumulativeTickets; i++)
            //    CumulativeTicketlist.Add(new PrintableTicket(TicketType.Cumulative, ticketNumber++, tVM.Protocol));

            if (tVM.DeliverableCumulativeTickets > 0)
            {
                CumulativeTicketlist.Add(new PrintableTicket(TicketType.Cumulative, ticketNumber++, tVM.Protocol, tVM.DeliverableCumulativeTickets));
            }

			for (int i = 0; i < tVM.DeliverableFreeTickets; i++)
                FreeTicketlist.Add(new PrintableTicket(TicketType.Free, ticketNumber++, tVM.Protocol));

            bool allTicketsPrinted = true;

            if (usePrinter && printerFound)
            {
                int deliverableTickets = (tVM.DeliverableFullPriceTickets + tVM.DeliverableReducedTickets + tVM.DeliverableDiscountTickets + tVM.DeliveredCumulativeTickets + tVM.DeliverableFreeTickets);
                allTicketsPrinted = false;
                
                var printedTickets = _Print(FullPriceTicketlist,ReducedTicketlist, DiscountTicketlist, CumulativeTicketlist, FreeTicketlist);

                allTicketsPrinted = (printedTickets == deliverableTickets);
                
                if (!allTicketsPrinted)
                {
                    MessageBox.Show("Stampante non in linea o carta esaurita!");
                    return;
                }

            }

            if ((printerFound && allTicketsPrinted) || !usePrinter)
            {
                if (SaveDeliveredTickets())
                {
                    //MessageBox.Show("Biglietti salvati correttamente");
                }
            }


			this.Close();
		}

		private void btnOpenVisit_Click(object sender, RoutedEventArgs e)
		{
			DoOpenVisit(DetailID);
			tVM.LoadData(DetailID);
			SetBehavior();
		}

		private bool SaveDeliveredTickets()
		{
			bool success = true;
			VisitaProgrammata_Dal dal = new VisitaProgrammata_Dal();
            //tVM.DeliveredFullPriceTickets += tVM.DeliverableFullPriceTickets;
            //tVM.DeliveredDiscountTickets += tVM.DeliverableReducedTickets;
            //tVM.DeliveredFreeTickets += tVM.DeliverableFreeTickets;

			//success = dal.SavePrintedTickets(DetailID, tVM.DeliveredFullPriceTickets, tVM.DeliveredDiscountTickets, tVM.DeliveredFreeTickets);
            //TODO: AGGIUNGERE NUOVI TIPI
            success = dal.SavePrintedTickets(DetailID, tVM.DeliverableFullPriceTickets, tVM.DeliverableReducedTickets, tVM.DeliverableFreeTickets, 0, 0);
			tVM = new TicketViewModel(DetailID);
			this.DataContext = tVM;

			return success;
		}

		private void nudTickets_ValueChanged(object sender, Telerik.Windows.Controls.RadRangeBaseValueChangedEventArgs e)
		{
			TicketNumberResult result = tVM.UpdateData();

			if (!result.Success)
			{
				result.Message = "ATTENZIONE: " + result.Message;
				MessageBox.Show(result.Message, "Errore inserimento biglietti", MessageBoxButton.OK);

				RadNumericUpDown nud = sender as RadNumericUpDown;
				if (nud != null)
					nud.Value -= result.Diff;
			}
		}

        public void OnUnloaded(Object sender, RoutedEventArgs e)
        {
            try
            {
                _printer.Close();
            }
            catch { }
        }

		#endregion // Event Handling


		#region Public Methods


		#endregion// Public Methods


		#region Private Methods

		private void DoOpenVisit(int detailID)
		{
			int prenotationID = 0;
			VisitaPrenotata_Dal dalPrenotedVisit = new VisitaPrenotata_Dal();
			VisitaProgrammata_Dal dalScheduled = new VisitaProgrammata_Dal();

			VisitaProgrammata vScheduled = dalScheduled.GetItem(DetailID);
			if (vScheduled != null)
			{
				VisitaPrenotata vp = dalPrenotedVisit.GetItem(vScheduled.Id_VisitaPrenotata);
				if (vp != null)
					prenotationID = vp.Id_Prenotazione;
			}

			bool schedule = true;
			string windowName = string.Format("Prenotation_{0}", prenotationID.ToString());
			BaseDetailPage frm = null;
			foreach (Window wnd in Application.Current.Windows)
			{
				if (wnd.Name == windowName && wnd.IsVisible)
				{
					frm = (BaseDetailPage)wnd;
					frm.Focus();
					schedule = false;
				}
			}
			if (schedule && prenotationID > 0)
			{
				frm = new wndScheduleTours(prenotationID, User);
				frm.Name = windowName;
				frm.User = User;
				frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				frm.CloseOnSave = true;
				//frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void SetBehavior()
		{
			nudFullPriceTickets.Minimum = 0;
			nudFullPriceTickets.Maximum = tVM.PrenotatedFullPriceTickets - tVM.DeliveredFullPriceTickets;

			nudReducedTickets.Minimum = 0;
            nudReducedTickets.Maximum = tVM.PrenotatedReducedTickets - tVM.DeliveredReducedTickets;

			nudFreeTickets.Minimum = 0;
			nudFreeTickets.Maximum = tVM.PrenotatedFreeTickets - tVM.DeliveredFreeTickets;

			if (
				tVM.DeliveredFullPriceTickets == tVM.PrenotatedFullPriceTickets
				&&
				tVM.DeliveredReducedTickets == tVM.PrenotatedReducedTickets
				&&
				tVM.DeliveredFreeTickets == tVM.PrenotatedFreeTickets
				)
			{
				pnlClosedLabel.Visibility = System.Windows.Visibility.Visible;
				btnOpenVisit.Visibility = System.Windows.Visibility.Collapsed;
			}
			else
			{
				pnlClosedLabel.Visibility = System.Windows.Visibility.Collapsed;

				if (tVM.ToPay <= 0)
				{
                   
                    string printerMessage = "";

                    if (usePrinter)
                        printerFound = _FindPrinter(out printerMessage);

                    if (printerFound)
                    {
                        pnlPrintButton.Visibility = System.Windows.Visibility.Visible;
                        pnlPrintLabel.Visibility = System.Windows.Visibility.Collapsed;
                        btnOpenVisit.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        pnlPrintButton.Visibility = System.Windows.Visibility.Collapsed;
                        MessageBox.Show(printerMessage);
                    }


				}
				else
				{
					pnlPrintButton.Visibility = System.Windows.Visibility.Collapsed;
					pnlPrintLabel.Visibility = System.Windows.Visibility.Visible;
					btnOpenVisit.Visibility = System.Windows.Visibility.Visible;
				}
			}
		}
		#endregion// Private Methods


		#region Overrides

		protected override void SetLayout()
		{
		}

		#endregion// Overrides


        #region Printer

        private void Print(string TicketString)
        {
            try
            {
                _printer.Print(TicketString);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        private string TicketPrintString(string VisitType,string TicketDescription, string Price, string BarCode, int pax, DateTime date, string protocol, string serial, string tourDate = "", string tourTime = "")
        {
            string TicketString = "";

            TicketString = File.ReadAllText("BocaPrintTemplate.txt", Encoding.UTF8);
            TicketString = TicketString.Replace("\r\n", "");
            TicketString = TicketString.Replace("~", VisitType).Replace("#", TicketDescription).Replace("@", Price).Replace("&", BarCode);
			TicketString = TicketString.Replace("TicketNotRefudable", "Biglietto non rimborsabile - Ticket not refundable");
			TicketString = TicketString.Replace("Protocol", "Protocollo: " + protocol);
			TicketString = TicketString.Replace("Serial", "Seriale: " + serial);
			TicketString = TicketString.Replace("<DATE>", tourDate == "" ? date.ToString("dd/MM/yyyy") : tourDate);
            TicketString = TicketString.Replace("<TIME>", tourTime == "" ? date.ToString("HH\\:mm\\:ss") : tourTime);
            TicketString = TicketString.Replace("??", pax.ToString());

            return TicketString;
        }



        private bool PrintTicket(bool cut, out DateTime date, PrintableTicket ticket)
        {
            date = DateTime.Now;
            try
            {
                DateTime startPrint = DateTime.Now;
                Print("<CB>");
                //Print(TicketPrintString("Descrizione", String.Format("{0:0.00}",30), type.Ticket_Code.ToString().PadLeft(12, '0'), type.Ticket_Pax, date));
                Print(TicketPrintString(tVM.DescTipoVisita,"Descrizione", String.Format("{0:0.00}", 30), "barcode", 1, date, ticket.TicketProtocol, ticket.TicketNumber.ToString(), tVM.Date, tVM.Hour));
                Print((cut ? "<p>" : "<q>"));

                return _printer.WaitStatus(startPrint, Thera.Biglietteria.Boca.PrinterStatus.TICKET_ACK, 2000);

            }
            catch
            {
                return false;
            }
        }

        private bool PrintTicket(bool cut,int pax, int ticketTypeID, ref DateTime date, PrintableTicket ticket, out long code)
        {
            date = date.AddSeconds(1);
            code = 0;
            try
            {
                DateTime startPrint = DateTime.Now;
                long val = 0;
                string ticketType = "Intero";
                decimal price = 0;
                string sPrice = "";

                switch(ticketTypeID)
                {
                    case 1:
                        ticketType = "Intero";
                        price = tVM.CostoBigliettoIntero;
                        sPrice = String.Format("{0:0.00}", price);
                        break;
                    case 2:
                        ticketType = "Ridotto";
                        price = tVM.CostoBigliettoRidotto;
                        sPrice = String.Format("{0:0.00}", price);
                        break;
                    case 3:
                        ticketType = "Scontato";
                        price = tVM.CostoBigliettoScontato;
                        sPrice = String.Format("{0:0.00}", price);
                        break;
                    case 4:
                        ticketType = "Cumulativo";
                        price = tVM.CostoBigliettoCumulativo;
                        sPrice = String.Format("{0:0.00}", price);
                        break;
                    case 5:
                        ticketType = "Omaggio";
                        break;
                    default:
                        ticketType = "Intero";
                        price = tVM.CostoBigliettoIntero;
                        sPrice = String.Format("{0:0.00}", price);
                        break;

                }

                BarCode.GenerateBarCode(pax, date, 0, ticketTypeID, out val);
                Print("<CB>");
                Print(TicketPrintString(tVM.DescTipoVisita,ticketType, sPrice, val.ToString().PadLeft(12, '0'), pax, date, ticket.TicketProtocol, ticket.TicketNumber.ToString(), tVM.Date, tVM.Hour));
                Print((cut ? "<p>" : "<q>"));
                code = val;
                return _printer.WaitStatus(startPrint, Thera.Biglietteria.Boca.PrinterStatus.TICKET_ACK, 2000);
            }
            catch
            {
                return false;
            }
        }


        private int _Print(List<PrintableTicket> fullPriceTickets, List<PrintableTicket> reducedTickets, List<PrintableTicket> discountTickets, List<PrintableTicket> cumulativeTickets, List<PrintableTicket> freeTickets)
        {
			int printedTickets = 0;
			int totalPrintableTickets = fullPriceTickets.Count + reducedTickets.Count + freeTickets.Count;
			DateTime start = DateTime.Now;
            Print("<S1>");
            //istruzioni aggiuntive per gestire assenza carta (vedi anche versione B)
            Print("<S1>");
            _printer.WaitStatus(start);
            Thread.Sleep(10);
            start = DateTime.Now;
            Print("<S1>");
            //fine istruzioni aggiuntive
            if (!_printer.WaitStatus(start))
            {
                //_dialogBox.ShowDialog(this, "Stampante non in linea o carta esaurita!", "OK");
				return printedTickets;
            }

            bool cut = false;

            DateTime date = DateTime.Now;

            long code = 0;

            foreach (PrintableTicket item in fullPriceTickets)
            {
				cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, 1, 1, ref date, item, out code))
                {
                    tVM.AddTicket(code, DetailID, 1, 1);
                    printedTickets++;
                }
            }

            foreach (PrintableTicket item in reducedTickets)
            {
				cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, 1, 2, ref date, item, out code))
                {
                    tVM.AddTicket(code, DetailID, 2, 1);
                    printedTickets++;
                }
            }

            foreach (PrintableTicket item in discountTickets)
            {
                cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, 1, 3, ref date, item, out code))
                {
                    tVM.AddTicket(code, DetailID, 3, 1);
                    printedTickets++;
                }
            }

            //foreach (PrintableTicket item in cumulativeTickets)
            //{
            //    cut = (printedTickets + 1) == totalPrintableTickets;
            //    if (PrintTicket(true, 1, 4, ref date, item, out code))
            //    {
            //        tVM.AddTicket(code, DetailID, 4, 1);
            //        printedTickets++;
            //    }
            //}

            foreach (PrintableTicket item in cumulativeTickets)
            {
                cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, item.Pax, 4, ref date, item, out code))
                {
                    tVM.AddTicket(code, DetailID, 4, item.Pax);
                    printedTickets++;
                }
            }

			foreach (PrintableTicket item in freeTickets)
            {
				cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, 1, 5, ref date, item, out code))
                {
                    tVM.AddTicket(code, DetailID, 5, 1);
                    printedTickets++;
                }
            }

			return printedTickets;
        }

        private bool _FindPrinter(out string message)
        {
            message = "Stampante non in linea";
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (_printer != null)
                _printer.Close();

            string[] ports = null;

            Thread.Sleep(1000);

            bool bUseDummyPrinter = false;
            string sDummyPrinter = ConfigurationManager.AppSettings["useDummyPrinter"];
            if (!string.IsNullOrEmpty(sDummyPrinter))
            {
                bUseDummyPrinter = bool.Parse(sDummyPrinter);
            }
            if (bUseDummyPrinter)
                ports = DummyPrinter.GetPrinterPorts();
            else
                ports = BocaPrinter.GetPrinterPorts();

            //_dialogBox.Hide();
            if (ports.Length > 0)
            {
                string port = ports[0];

                if (port != "")
                {
                    if (_printer == null)
                    {
                        if (_printer != null)
                        {
                            _printer.Close();
                        }
                        if (bUseDummyPrinter)
                            _printer = new DummyPrinter(port);
                        else
                            _printer = new BocaPrinter(port);
                        
                    }
                    else
                    {
                        if (_printer != null)
                        {
                            _printer.Close();
                        }
                        if (bUseDummyPrinter)
                            _printer = new DummyPrinter(port);
                        else
                            _printer = new BocaPrinter(port);
                    }

                    _printer.Open();
                }
                else
                    return false;
            }
            else
                return false;



            return true;
        }

        #endregion


		#region Error Handling

		private void Confirm_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
		}

		private void Confirm_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;
		}

		private void Validation_Error(object sender, ValidationErrorEventArgs e)
		{
		}

		#endregion// Error Handling

	}
}