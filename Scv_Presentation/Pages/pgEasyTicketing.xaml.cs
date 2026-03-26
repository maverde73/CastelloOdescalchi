using Presentation.LocalPrinter;
using Scv_Entities;
using Scv_Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;
using Thera.Biglietteria.Boca;


namespace Presentation.Pages
{
    /// <summary>
    /// Interaction logic for pgEasyTicketing.xaml
    /// </summary>
    public partial class pgEasyTicketing : BaseContentPage, INotifyPropertyChanged
    {

        #region Variables
        //EasyTicketingViewModel vm = null;
        ScheduledToursViewModel vmST = null;
        IBocaPrinter _printer = null;
        bool usePrinter = false;
        bool printerFound = true;
        #endregion

        public pgEasyTicketing()
        {
            InitializeComponent();

            //CM added 2016-31-05
            btnPrintGruoped.Click += BtnPrintGruoped_Click;
            //CM end

            CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgEasyTicketing_CommandEvent);
            Unloaded += new RoutedEventHandler(OnUnloaded);

            this.User = ApplicationState.GetValue<LK_User>("currentUser");
            usePrinter = Convert.ToBoolean(ConfigurationManager.AppSettings["usePrinter"]);

            vmST = new ScheduledToursViewModel();

            swAppointments.ActiveViewDefinition.DayStartTime = Convert.ToDateTime("01/01/2001 " + vmST.GetOraIniziale()).TimeOfDay;
            swAppointments.ActiveViewDefinition.DayEndTime = Convert.ToDateTime("01/01/2001 " + vmST.GetOraFinale()).AddMinutes(15).TimeOfDay;

            swAppointments.DataContext = vmST;

            //vm = new EasyTicketingViewModel();
            //vmST.Visits = vmST.Visits;
            //cmbPetitioner.DataContext = vm;
            cmbVisitTypes.DataContext = vmST;
            cmbLanguages.DataContext = vmST;
            cmbLanguages.SelectedValue = 1;
            cmbPaymentTypes.DataContext = vmST;
            grdTickets.DataContext = vmST;
            btnPrint.DataContext = vmST;

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            dispatcherTimer.Start();

            cmbPaymentTypes.SelectedIndex = 0;

            string printerMessage = "";

            btnPrint.IsEnabled = usePrinter;
            //CM added 2016-31-05
            btnPrintGruoped.IsEnabled = usePrinter;
            //CM end
            btnCancelTicket.IsEnabled = false;

            if (usePrinter)
            {
                printerFound = _FindPrinter(out printerMessage);
                btnPrint.IsEnabled = printerFound;
                //CM added 2016-31-05
                btnPrintGruoped.IsEnabled = printerFound;
                //CM end
                if (!printerFound)
                    MessageBox.Show("Stampante non in linea!");
            }
            else
            {
                printerFound = false;
                btnPrint.IsEnabled = true;
                //CM added 2016-31-05
                btnPrintGruoped.IsEnabled = true;
                //CM end
            }


        }


        #region Events

        public void OnUnloaded(Object sender, RoutedEventArgs e)
        {
            try
            {
                 _printer.Dispose();
            }
            catch(Exception ex) 
            {
                LogExc(GetRecursiveInnerException(ex));
            }
        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            btnTotal.Content = vmST.PrezzoTotale.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void DeleteVisit(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            if (btn != null)
            {
                int id_riga = 0;
                int.TryParse(btn.CommandParameter.ToString(), out id_riga);
                vmST.RemoveTicketRow(id_riga);
            }
        }

        private void UndoChangeOnRow(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            if (btn != null)
            {
                int id_riga = 0;
                int.TryParse(btn.CommandParameter.ToString(), out id_riga);

                var ticketRow = vmST.SrcTickets.FirstOrDefault(rx => rx.Id_Riga == id_riga);
                ticketRow.Interi = 0;
                ticketRow.Ridotti = 0;
                ticketRow.Omaggio = 0;
                ticketRow.Prezzo = 0;

            }
        }

        private void btnAddVP_Click(object sender, RoutedEventArgs e)
        {
            vmST.AddTicketRow();
        }

        private void pgEasyTicketing_CommandEvent(ContentPageCommandEventArgs e)
        {
            switch (e.CommandType)
            {
                case ContentPageCommandType.New:
                    vmST.ResetAll();
                    break;


            }
        }

        //private void cmbPetitioner_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Richiedente selection = e.AddedItems[0] as Richiedente;
        //    if (selection != null && selection.Id_Richiedente > 0)
        //    {
        //        txtSurname.Text = selection.Cognome;
        //        txtName.Text = selection.Nome;
        //        txtEmail.Text = selection.Email;
        //    }
        //}

        private void RadScheduleView_ShowDialog(object sender, ShowDialogEventArgs e)
        {
            if (e.DialogViewModel is AppointmentDialogViewModel)
                e.Cancel = true;
        }

        //private void swAppointments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    Appointment selectedAppointment = (Appointment)swAppointments.SelectedAppointment;
        //    var appInfo = selectedAppointment.Body.Split(',');
        //    DoOpenItem(Convert.ToInt32(appInfo[0]), Convert.ToInt32(appInfo[1]), WindowStartupLocation.CenterScreen);
        //}

        protected void frm_DetailWindowClosing(object sender, ClosingDetailWindowEventArgs e)
        {

        }

        private void contextMenu_Click(object sender, RadRoutedEventArgs e)
        {
            Appointment selectedAppointment = null;
            string[] appInfo = null;
            switch (((RadMenuItem)(e.Source)).CommandParameter.ToString())
            {
                case "open":
                    selectedAppointment = (Appointment)swAppointments.SelectedAppointment;
                    appInfo = selectedAppointment.Body.Split(',');
                    DoOpenItem(Convert.ToInt32(appInfo[0]), Convert.ToInt32(appInfo[1]), WindowStartupLocation.CenterScreen);
                    break;

                case "dispatch":
                    selectedAppointment = (Appointment)swAppointments.SelectedAppointment;
                    appInfo = selectedAppointment.Body.Split(',');
                    DoOpenScheduleTours(Convert.ToInt32(appInfo[0]), WindowStartupLocation.CenterScreen);
                    
                    break;

                case "printtickets":
                    selectedAppointment = (Appointment)swAppointments.SelectedAppointment;
                    appInfo = selectedAppointment.Body.Split(',');
                    vmST.OnSelectScheduledVisit(Convert.ToInt32(appInfo[0]), Convert.ToInt32(appInfo[1]));
                    cmbVisitTypes.IsEnabled = false;
                    btnPrint.IsEnabled = vmST.Canprint;
                    btnCancelTicket.IsEnabled = true;
                    cmbVisitTypes.SelectedItem = vmST.SelectedNewVisitType;
                    cmbPaymentTypes.SelectedValue = vmST.Id_TipoPagamento;
                    cmbLanguages.SelectedValue = vmST.SelectedId_Lingua;

                    if (!vmST.Canprint)
                        lblMessage.Content = "Biglietti emessi per questa visita!";
                    else if (vmST.Paid)
                        lblMessage.Content = "Biglietti già pagati per questa visita!";
                    else
                        lblMessage.Content = "";
                    
                    break;
            }
        }

        #endregion// Events

        #region Methods
        private int SaveVisit()
        {
            vmST.Id_TipoPagamento = (int)cmbPaymentTypes.SelectedValue;
            int idVisitaProgrammata = 0;
            if (vmST.SelectedVisit == null)
                idVisitaProgrammata = vmST.AddNewVisit(Convert.ToInt32(cmbLanguages.SelectedValue), "", "", "", User.Id_User, null);
            else
                idVisitaProgrammata = vmST.UpdateVisit(Convert.ToInt32(cmbLanguages.SelectedValue), "", "", "", User.Id_User);

            return idVisitaProgrammata;
        }


        private void DoOpenItem(int idPrenotazione, int idVisitaProgrammata, WindowStartupLocation startupLocation)
        {
            bool open = true;
            string windowName = string.Format("Prenotation_{0}", idPrenotazione);
            BaseDetailPage frm = null;
            foreach (Window wnd in Application.Current.Windows)
            {
                if (wnd.Name == windowName && wnd.IsVisible)
                {
                    frm = (BaseDetailPage)wnd;
                    frm.Focus();
                    open = false;
                }
            }
            if (open)
            {
                frm = new wndPrenotationDetail(idPrenotazione);
                frm.User = User;
                frm.Name = windowName;
                frm.WindowStartupLocation = startupLocation;
                frm.DetailWindowClosing += new CommonDelegates.ClosingDetailWindowEventHandler(frm_DetailWindowClosing);
                frm.ShowDialog();
                vmST.LoadAppointments();
            }

        }



        private void DoOpenScheduleTours(int idPrenotazione,  WindowStartupLocation startupLocation)
        {
            bool open = true;
            string windowName = string.Format("Prenotation_{0}", idPrenotazione);
            BaseDetailPage frm = null;
            foreach (Window wnd in Application.Current.Windows)
            {
                if (wnd.Name == windowName && wnd.IsVisible)
                {
                    frm = (BaseDetailPage)wnd;
                    frm.Focus();
                    open = false;
                }
            }
            if (open)
            {
                frm = new wndScheduleTours(idPrenotazione, User);
                frm.User = User;
                frm.Name = windowName;
                frm.WindowStartupLocation = startupLocation;
                frm.DetailWindowClosing += new CommonDelegates.ClosingDetailWindowEventHandler(frm_DetailWindowClosing);
                frm.ShowDialog();
                vmST.LoadAppointments();
            }

        }


        #endregion

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
           this.vmST.IdVP = SaveVisit();
        }

        private void btnCancelTicket_Click(object sender, RoutedEventArgs e)
        {
            if (vmST.SelectedVisit != null)
            {
                wndCancelTickets _wndCancelTicket = new wndCancelTickets(vmST.SelectedVisit.Id_Prenotazione, vmST.SelectedVisit.Id_VisitaProgrammata);
                _wndCancelTicket.ShowDialog();
            }
            else
            {
                if (this.vmST.IdVP > 0)
                {
                    wndCancelTickets _wndCancelTicket = new wndCancelTickets(0, this.vmST.IdVP);
                    _wndCancelTicket.ShowDialog();


                }
            }
        }


        //CM added stampa ragruppata 2016-31-05
        private void BtnPrintGruoped_Click(object sender, RoutedEventArgs e)
        {
            vmST.IdVP = SaveVisit();

            List<PrintableTicket> FullPriceTicketlist = new List<PrintableTicket>();
            List<PrintableTicket> ReducedTicketlist = new List<PrintableTicket>();
            List<PrintableTicket> DiscountTicketlist = new List<PrintableTicket>();
            List<PrintableTicket> CumulativeTicketlist = new List<PrintableTicket>();
            List<PrintableTicket> FreeTicketlist = new List<PrintableTicket>();

            vmST.GetTicketLastSerial();
            int ticketNumber = vmST.TicketsStartNumber;

            if (vmST.SrcTickets[0].Interi >0)
                FullPriceTicketlist.Add(new PrintableTicket(TicketType.FullPrice, ticketNumber++, vmST.Protocollo, vmST.SrcTickets[0].Interi));

            if (vmST.SrcTickets[0].Ridotti > 0)
                ReducedTicketlist.Add(new PrintableTicket(TicketType.Reduced, ticketNumber++, vmST.Protocollo, vmST.SrcTickets[0].Ridotti));

            if (vmST.SrcTickets[0].Scontati > 0)
                DiscountTicketlist.Add(new PrintableTicket(TicketType.Reduced, ticketNumber++, vmST.Protocollo, vmST.SrcTickets[0].Scontati));

            if (vmST.SrcTickets[0].Cumulativi > 0)
                CumulativeTicketlist.Add(new PrintableTicket(TicketType.Cumulative, ticketNumber++, vmST.Protocollo, vmST.SrcTickets[0].Cumulativi));

            if(vmST.SrcTickets[0].Omaggio > 0)
                FreeTicketlist.Add(new PrintableTicket(TicketType.Free, ticketNumber++, vmST.Protocollo, vmST.SrcTickets[0].Omaggio));

            bool allTicketsPrinted = true;

            if (usePrinter && printerFound)
            {

                int printableTickets = FullPriceTicketlist.Count + ReducedTicketlist.Count + DiscountTicketlist.Count + CumulativeTicketlist.Count + FreeTicketlist.Count;
                var printedTickets = _PrintGrouped(FullPriceTicketlist, ReducedTicketlist, DiscountTicketlist, CumulativeTicketlist, FreeTicketlist);
                //int printedTickets = printableTickets;
                allTicketsPrinted = (printedTickets == printableTickets);

                if (!allTicketsPrinted)
                {
                    MessageBox.Show("Stampante non in linea o carta esaurita!");
                    return;
                }
                else
                {

                    vmST.UpdatePrinted(ticketNumber);
                    vmST.LoadAppointments();

                    btnPrint.IsEnabled = false;

                    //CM added 2016-31-05
                    btnPrintGruoped.IsEnabled = false;
                    //CM end

                    btnCancelTicket.IsEnabled = true;

                    lblMessage.Content = "Biglietti emessi per questa visita!";

                    //vmST.SrcTickets.Clear();
                    //vmST.AddTicketRow();
                    //cmbVisitTypes.IsEnabled = true;
                    //cmbPaymentTypes.IsEnabled = true;
                    //cmbLanguages.IsEnabled = true;
                }

            }


            if (!usePrinter && !printerFound)
            {
                vmST.UpdatePrinted(ticketNumber);
                vmST.LoadAppointments();

                btnPrint.IsEnabled = false;
                //CM added 2016-31-05
                btnPrintGruoped.IsEnabled = false;
                //CM end
                btnCancelTicket.IsEnabled = true;

                lblMessage.Content = "Biglietti emessi per questa visita!";
            }

        }
        //CM end


        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            vmST.IdVP = SaveVisit();

            List<PrintableTicket> FullPriceTicketlist = new List<PrintableTicket>();
            List<PrintableTicket> ReducedTicketlist = new List<PrintableTicket>();
            List<PrintableTicket> DiscountTicketlist = new List<PrintableTicket>();
            List<PrintableTicket> CumulativeTicketlist = new List<PrintableTicket>();
            List<PrintableTicket> FreeTicketlist = new List<PrintableTicket>();

            vmST.GetTicketLastSerial();
            int ticketNumber = vmST.TicketsStartNumber;

            for (int i = 0; i < vmST.SrcTickets[0].Interi; i++)
                FullPriceTicketlist.Add(new PrintableTicket(TicketType.FullPrice, ticketNumber++, vmST.Protocollo));

            for (int i = 0; i < vmST.SrcTickets[0].Ridotti; i++)
                ReducedTicketlist.Add(new PrintableTicket(TicketType.Reduced, ticketNumber++, vmST.Protocollo));

            for (int i = 0; i < vmST.SrcTickets[0].Scontati; i++)
                DiscountTicketlist.Add(new PrintableTicket(TicketType.Discount, ticketNumber++, vmST.Protocollo));

            //for (int i = 0; i < vmST.SrcTickets[0].Cumulativi; i++)
            //    CumulativeTicketlist.Add(new PrintableTicket(TicketType.Cumulative, ticketNumber++, vmST.Protocollo));

            if (vmST.SrcTickets[0].Cumulativi > 0)
            {
                CumulativeTicketlist.Add(new PrintableTicket(TicketType.Cumulative, ticketNumber++, vmST.Protocollo, vmST.SrcTickets[0].Cumulativi));
            }

            for (int i = 0; i < vmST.SrcTickets[0].Omaggio; i++)
                FreeTicketlist.Add(new PrintableTicket(TicketType.Free, ticketNumber++, vmST.Protocollo));

            bool allTicketsPrinted = true;

            if (usePrinter && printerFound)
            {

                int printableTickets = FullPriceTicketlist.Count + ReducedTicketlist.Count + DiscountTicketlist.Count + CumulativeTicketlist.Count + FreeTicketlist.Count;
                var printedTickets = _Print(FullPriceTicketlist, ReducedTicketlist, DiscountTicketlist, CumulativeTicketlist, FreeTicketlist);
                //int printedTickets = printableTickets;
                allTicketsPrinted = (printedTickets == printableTickets);

                if (!allTicketsPrinted)
                {
                    MessageBox.Show("Stampante non in linea o carta esaurita!");
                    return;
                }
                else
                {
                    
                    vmST.UpdatePrinted(ticketNumber);
                    vmST.LoadAppointments();

                    btnPrint.IsEnabled = false;

                    //CM added 2016-31-05
                    btnPrintGruoped.IsEnabled = false;
                    //CM end

                    btnCancelTicket.IsEnabled = true;

                    lblMessage.Content = "Biglietti emessi per questa visita!";
                    
                    //vmST.SrcTickets.Clear();
                    //vmST.AddTicketRow();
                    //cmbVisitTypes.IsEnabled = true;
                    //cmbPaymentTypes.IsEnabled = true;
                    //cmbLanguages.IsEnabled = true;
                }

            }


            if (!usePrinter && !printerFound)
            {
                vmST.UpdatePrinted(ticketNumber);
                vmST.LoadAppointments();

                btnPrint.IsEnabled = false;
                //CM added 2016-31-05
                btnPrintGruoped.IsEnabled = false;
                //CM end
                btnCancelTicket.IsEnabled = true;

                lblMessage.Content = "Biglietti emessi per questa visita!";
            }

        }

        #region Printer

        private void Print(string TicketString)
        {
            try
            {
                if (!_printer.isOpen)
                {
                    LogMessage("Print : _printer is close");
                    _printer.Open();
                }
                
                _printer.Print(TicketString);
            }
            catch (Exception ex)
            {
                try
                {
                    if (_printer.isOpen)
                        _printer.Close();
                }
                catch(Exception exint) 
                {
                    LogExc(GetRecursiveInnerException(exint));
                }

                LogExc(GetRecursiveInnerException(ex));
            }

        }

        private string TicketPrintString(string VisitType, string TicketDescription, string Price, string BarCode, int pax, DateTime date, string protocol, string serial, string tourDate = "", string tourTime = "")
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


        private bool PrintTicket(bool cut, int pax, int ticketTypeID, ref DateTime date, PrintableTicket ticket, out long code)
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

                switch (ticketTypeID)
                {
                    case 1:
                        ticketType = "Intero";
                        price = (vmST.SelectedNewVisitType.PrezzoIntero != null) ? (decimal)vmST.SelectedNewVisitType.PrezzoIntero : 0;
                        //sPrice = String.Format("{0:0.00}", price);
                        break;
                    case 2:
                        ticketType = "Ridotto";
                        price = (vmST.SelectedNewVisitType.PrezzoRidotto != null) ? (decimal)vmST.SelectedNewVisitType.PrezzoRidotto : 0;
                        //sPrice = String.Format("{0:0.00}", price);
                        break;
                    case 3:
                        ticketType = "Scontato";
                        price = (vmST.SelectedNewVisitType.PrezzoScontato != null) ? (decimal)vmST.SelectedNewVisitType.PrezzoScontato : 0;
                        //sPrice = String.Format("{0:0.00}", price);
                        break;
                    case 4:
                        ticketType = "Cumulativo";
                        price = (vmST.SelectedNewVisitType.PrezzoCumulativo != null) ? (decimal)vmST.SelectedNewVisitType.PrezzoCumulativo : 0; ;
                        sPrice = String.Format("{0:0.00}", price);
                        break;
                    case 5:
                        ticketType = "Omaggio";
                        break;
                    default:
                        ticketType = "Intero";
                        price = (vmST.SelectedNewVisitType.PrezzoIntero != null) ? (decimal)vmST.SelectedNewVisitType.PrezzoIntero : 0;
                        //sPrice = String.Format("{0:0.00}", price);
                        break;

                }

                //CM added
                sPrice =string.IsNullOrEmpty(sPrice)?String.Format("{0:0.00}", price*ticket.Pax):sPrice;
                //CM end

                BarCode.GenerateBarCode(pax, date, 0, ticketTypeID, out val);
                Print("<CB>");
                Print(TicketPrintString(vmST.SelectedNewVisitType.Descrizione, ticketType, sPrice, val.ToString().PadLeft(12, '0'), pax, date, ticket.TicketProtocol, ticket.TicketNumber.ToString(), vmST.CurrentDate.ToString("dd/MM/yyyy"), vmST.SrcTickets[0].Ora_Visita));
                Print((cut ? "<p>" : "<q>"));
                code = val;
                return _printer.WaitStatus(startPrint, Thera.Biglietteria.Boca.PrinterStatus.TICKET_ACK, 2000);
            }
            catch(Exception ex)
            {
                LogExc(GetRecursiveInnerException(ex));
                return false;
            }
        }

        //CM added  Stampa ragruppata 2016-31-05
        private int _PrintGrouped(List<PrintableTicket> fullPriceTickets, List<PrintableTicket> reducedTickets, List<PrintableTicket> discountTickets, List<PrintableTicket> cumulativeTickets, List<PrintableTicket> freeTickets)
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
                if (PrintTicket(true, item.Pax, 1, ref date, item, out code))
                {
                    vmST.AddTicket(code, vmST.IdVisitaProgrammata, 1, item.Pax);
                    printedTickets++;
                }
            }

            foreach (PrintableTicket item in reducedTickets)
            {
                cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, item.Pax, 2, ref date, item, out code))
                {
                    vmST.AddTicket(code, vmST.IdVisitaProgrammata, 2, item.Pax);
                    printedTickets++;
                }
            }

            foreach (PrintableTicket item in discountTickets)
            {
                cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, item.Pax, 3, ref date, item, out code))
                {
                    vmST.AddTicket(code, vmST.IdVisitaProgrammata, 3, item.Pax);
                    printedTickets++;
                }
            }

            foreach (PrintableTicket item in cumulativeTickets)
            {
                cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, item.Pax, 4, ref date, item, out code))
                {
                    vmST.AddTicket(code, vmST.IdVisitaProgrammata, 4, item.Pax);
                    printedTickets++;
                }
            }

            foreach (PrintableTicket item in freeTickets)
            {
                cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, item.Pax, 5, ref date, item, out code))
                {
                    vmST.AddTicket(code, vmST.IdVisitaProgrammata, 5, item.Pax);
                    printedTickets++;
                }
            }

            return printedTickets;

        }
        //CM end


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
                    vmST.AddTicket(code, vmST.IdVisitaProgrammata, 1, 1);
                    printedTickets++;
                }
            }

            foreach (PrintableTicket item in reducedTickets)
            {
                cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, 1, 2, ref date, item, out code))
                {
                    vmST.AddTicket(code, vmST.IdVisitaProgrammata, 2, 1);
                    printedTickets++;
                }
            }

            foreach (PrintableTicket item in discountTickets)
            {
                cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, 1, 3, ref date, item, out code))
                {
                    vmST.AddTicket(code, vmST.IdVisitaProgrammata, 3, 1);
                    printedTickets++;
                }
            }

            //foreach (PrintableTicket item in cumulativeTickets)
            //{
            //    cut = (printedTickets + 1) == totalPrintableTickets;
            //    if (PrintTicket(true, 1, 4, ref date, item, out code))
            //    {
            //        vmST.AddTicket(code, vmST.IdVisitaProgrammata, 4, 1);
            //        printedTickets++;
            //    }
            //}

            foreach (PrintableTicket item in cumulativeTickets)
            {
                cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true,item.Pax, 4, ref date, item, out code))
                {
                    vmST.AddTicket(code, vmST.IdVisitaProgrammata, 4, item.Pax);
                    printedTickets++;
                }
            }

            foreach (PrintableTicket item in freeTickets)
            {
                cut = (printedTickets + 1) == totalPrintableTickets;
                if (PrintTicket(true, item.Pax, 5, ref date, item, out code))
                {
                    vmST.AddTicket(code, vmST.IdVisitaProgrammata, 5, 1);
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
              
                LogMessage(port);

                if (port != "")
                {
                    if (_printer != null)
                    {
                        LogMessage("_FindPrinter : _printer != null");

                        if (_printer.isOpen)
                            LogMessage("_FindPrinter : isOpen before new");

                        _printer.Close();
                    }

                    if (bUseDummyPrinter)
                        _printer = new DummyPrinter(port);
                    else
                        _printer = new BocaPrinter(port);

                    _printer.Open();
                }
                else
                {
                    LogMessage("_FindPrinter : port == ''");
                    return false;
                }
            }
            else
            {
                LogMessage("_FindPrinter : ports.Length == 0");
                return false;
            }


            return true;
        }

        #endregion

        private void btnNewVisit_Click(object sender, RoutedEventArgs e)
        {
            lblMessage.Content = "";
            this.btnPrint.IsEnabled = true;
            //CM added 2016-31-05
            this.btnPrintGruoped.IsEnabled = true;
            //CM end
            this.cmbVisitTypes.IsEnabled = true;
            cmbVisitTypes.SelectedItem = cmbVisitTypes.Items[0];
            cmbLanguages.SelectedItem = cmbLanguages.Items[0];
            cmbPaymentTypes.SelectedItem = cmbPaymentTypes.Items[0];
            vmST.ClearSelection();
            vmST.SrcTickets.Clear();
            vmST.AddTicketRow();
            btnCancelTicket.IsEnabled = false;
        }

        private void swAppointments_AppointmentDeleted(object sender, AppointmentDeletedEventArgs e)
        {
            Appointment selectedAppointment = (Appointment)e.Appointment;
            var appInfo = selectedAppointment.Body.Split(',');
            vmST.DeletePrenotation(Convert.ToInt32(appInfo[0]), Convert.ToInt32(appInfo[1]));
            if (vmST.EnableAll)
            {
                btnNewVisit_Click(null, null);
            }
        }


        #region Error Logging


        private Exception GetRecursiveInnerException(Exception exc)
        {
            if (exc.InnerException != null)
                return GetRecursiveInnerException(exc.InnerException);
            else
                return exc;
        }

        static object c = new object();
        private void LogExc(Exception ex, string message = "")
        {
            lock (c)
            {
                var logFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\SCV_Log.txt";
                var oldLogFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\SCV_Log_old.txt";

                try
                {
                    long len = 0;
                    using (FileStream fs = new FileStream(logFile, FileMode.OpenOrCreate, FileAccess.Read))
                        len = fs.Length;
                    if (File.Exists(logFile) && new FileInfo(logFile).Length > 1048576)
                    {
                        File.Copy(logFile, oldLogFile, true);
                        File.Delete(logFile);
                    }
                }
                catch { }
                try
                {
                    using (FileStream fs = new FileStream(logFile, FileMode.Append, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(string.Format("{0}{1}{2}{3}Message:{4}{5}source:{6}{7}Stack Trace:{8}{9}TargetSite:{10}{11}"
                                                                                 , "---------------------------------------------------------"
                                                                                 , System.Environment.NewLine
                                                                                 , DateTime.Now.ToString()
                                                                                 , System.Environment.NewLine
                                                                                 , ex.Message
                                                                                 , System.Environment.NewLine
                                                                                 , ex.Source
                                                                                 , System.Environment.NewLine
                                                                                 , ex.StackTrace
                                                                                 , System.Environment.NewLine
                                                                                 , ex.StackTrace
                                                                                 , System.Environment.NewLine));


                    }
                }
                catch { }
            }
        }

        private void LogMessage(string message = "")
        {
            lock (c)
            {
                var logFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\SCV_Log.txt";
                var oldLogFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\SCV_Log_old.txt";

                try
                {
                    long len = 0;
                    using (FileStream fs = new FileStream(logFile, FileMode.OpenOrCreate, FileAccess.Read))
                        len = fs.Length;
                    if (File.Exists(logFile) && new FileInfo(logFile).Length > 1048576)
                    {
                        File.Copy(logFile, oldLogFile, true);
                        File.Delete(logFile);
                    }
                }
                catch { }
                try
                {
                    using (FileStream fs = new FileStream(logFile, FileMode.Append, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(string.Format("{0}{1}{2}{3}Message:{4}{5}"
                                                                                 , "---------------------------------------------------------"
                                                                                 , System.Environment.NewLine
                                                                                 , DateTime.Now.ToString()
                                                                                 , System.Environment.NewLine
                                                                                 , message
                                                                                 , System.Environment.NewLine));


                    }
                }
                catch { }
            }
        }
        #endregion



    }
}
