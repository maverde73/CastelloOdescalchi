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
using Telerik.Windows.Controls.GridView;
using Scv_Entities;
using Scv_Model;
using Telerik.Windows.Controls;
using Scv_Dal;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using Scv_Model.Common;
using SCV_FSP_PaymentServiceBO;
namespace Presentation.CustomControls.PaymentLib
{
    /// <summary>
    /// Interaction logic for wndPayment.xaml
    /// </summary>
    public partial class wndPayment : BasePaymentManager, INotifyPropertyChanged
    {
        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion// Public Events

        #region Private Fields

        private ObservableCollection<ValidationError> validationErrors = null;

        private int _errors = 0;

        private PaymentViewModel vm = null;

        private Pagamento_Dal dalPayment = new Pagamento_Dal();

        Parametri_Dal dalParameters = new Parametri_Dal();

        //ONLINEPAYMENT
        OnlinePayment onlinePaymentObj = new OnlinePayment();

        #endregion// Private Fields


        #region Public Properties

        public ObservableCollection<ValidationError> ValidationErrors
        {
            get
            {
                if (validationErrors == null)
                    validationErrors = new ObservableCollection<ValidationError>();
                return validationErrors;
            }
            set { validationErrors = value; }
        }

        public List<int> VisitsIDs
        {
            get { return (List<int>)GetValue(VisitsIDsProperty); }
            set { SetValue(VisitsIDsProperty, value); }
        }

        public PaymentRange PaymentRange
        {
            get { return (PaymentRange)GetValue(PaymentRangeProperty); }
            set
            {
                SetValue(PaymentRangeProperty, value);
            }
        }

        #endregion// Public Properties


        #region Dependency Properties

        public static DependencyProperty VisitsIDsProperty =
            DependencyProperty.Register(
                "VisitsIDs",
                typeof(List<int>),
                typeof(wndPayment),
                new UIPropertyMetadata()
                );

        public static DependencyProperty PaymentRangeProperty =
            DependencyProperty.Register(
                "PaymentRange",
                typeof(PaymentRange),
                typeof(wndPayment),
                new UIPropertyMetadata()
                );

        #endregion// Dependency Properties


        #region Constructors

        public wndPayment(PaymentRange paymentRange, int prenotationID, int? visitID)
        {
            InitializeComponent();
            vm = new PaymentViewModel(paymentRange, prenotationID, visitID);
            this.DataContext = vm;

            this.Title = vm.ObjPayment.PaymentRange;
            this.Topmost = Convert.ToBoolean(ConfigurationManager.AppSettings["windTopMost"]);

            SetLayout(paymentRange);

            btnPay.Click += new RoutedEventHandler(btnPay_Click);
            btnPayDifference.Click += new RoutedEventHandler(btnPayDifference_Click);
            btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            btnPrint.Click += new RoutedEventHandler(btnPrint_Click);
            btnRefund.Click += new RoutedEventHandler(btnRefund_Click);
            btnUpdatePayment.Click += new RoutedEventHandler(btnUpdatePayment_Click);

            btnRefund.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnPrint.IsEnabled = false;

            alreadyPayd.Visibility = System.Windows.Visibility.Collapsed;

            btnDelete.Content = "Elimina pagamento";

            if (vm.ObjPayment.Id_Pagamento > 0)
            {
                if (vm.ObjPayment.Dt_Pagamento != null)
                {
                    btnRefund.IsEnabled = (vm.ObjPayment.Id_TipoPagamento != 1);
                    btnPrint.IsEnabled = true;
                    alreadyPayd.Visibility = System.Windows.Visibility.Visible;
                }

                btnUpdatePayment.IsEnabled = vm.ObjPayment.ShowUpdatePaymentButton;
                btnDelete.IsEnabled = vm.ObjPayment.ShowDeleteButton;

                if (vm.ObjPayment.Id_TipoPagamento == 1 )
                {
                    if (vm.ObjPayment.Dt_Pagamento == null)
                    {
                        btnDelete.Content = "Annullamento Ordine";
                        if (!string.IsNullOrEmpty(vm.ObjPayment.NumeroOrdine))
                        {
                            btnRegister.IsEnabled = true;
                            txtIdTransazione.IsReadOnly = false;
                            txtNoteOrdine.IsReadOnly = false;
                            dtpDtPagamento.IsReadOnly = false;
                        }
                    }
                    
                        
                }

                if (vm.ObjPayment.Dt_Pagamento != null)
                {
                    dtpDtPagamento.SelectedDate = vm.ObjPayment.Dt_Pagamento.Value;
                    dtpDtPagamento.SelectedTime = vm.ObjPayment.Dt_Pagamento.Value.TimeOfDay;
                }

                 
            }
            else
                btnCheckOrderStatus.IsEnabled = false;

            cmbPaymentType.SelectionChanged += new SelectionChangedEventHandler(cmbPaymentType_SelectionChanged);

            //TestPaymentConfirmation();
        }

        #endregion// Constructors


        #region Event Handlers

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            //ONLINEPAYMENT
            DateTime checkDate = new DateTime();
            if (vm.ObjPayment.Id_TipoPagamento == 1 && !vm.CheckIfOnlinePaymentIsAllowed(out checkDate))
            {
                MessageBox.Show(string.Format("La creazione dell'ordine di pagamento online è consentita solo per visite con data successiva al {0}.",checkDate.ToString("dd/MM/yyyy"), "Attenzione"));
                return;
            }

            if (DoPay())
                this.Close();
        }

        private void btnUpdatePayment_Click(object sender, RoutedEventArgs e)
        {
            if (vm.ObjPayment.Dt_Pagamento != null)
            {
                if (vm.ObjPayment.Dt_Pagamento.Value.Date != DateTime.Now.Date)
                {
                    MessageBox.Show("Impossibile modificare un pagamento non eseguito oggi.", "Errore modifica pagamento");
                    return;
                }
            }

            if (DoPay())
                this.Close();
        }

        private void btnPayDifference_Click(object sender, RoutedEventArgs e)
        {
            vm.AllineaPagamento();
            btnPayDifference.Visibility = System.Windows.Visibility.Collapsed;
            btnPay.Visibility = System.Windows.Visibility.Visible;
            vm.LoadData();
            Pagamento p = vm.ObjPayment;
            vm.ObjPayment = p;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //ONLINE PAYMENT
            string message = "Eliminare il pagamento? (L'azione non è annullabile)";
            string caption = "Eliminazione pagamento";

            if (vm.ObjPayment.Id_TipoPagamento == 1 && vm.ObjPayment.Dt_Pagamento == null)
            {
                message = "Sicuro di voler annullare l'ordine di pagamento?";
                caption = "Annullamento ordine pagamento";
            }

            if (MessageBox.Show(message, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                DoDelete();
                this.Close();
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
           
            /*
            PrintReceiptArgs args = new PrintReceiptArgs();
            ObservableCollection<V_VisiteProgrammate> list = new ObservableCollection<V_VisiteProgrammate>();
            switch (vm.PaymentRange)
            {
                case PaymentLib.PaymentRange.PerPrenotation:
                    list = new ObservableCollection<V_VisiteProgrammate>(new VisitaProgrammata_Dal().GetVListByIdPrenotazione(vm.ObjPayment.Id_Prenotazione));
                    break;

                case PaymentLib.PaymentRange.PerVisit:
                    list = new ObservableCollection<V_VisiteProgrammate>() { new VisitaProgrammata_Dal().GetVListById((int)vm.ObjPayment.Id_VisitaProgrammata) };
                    break;
            }

            List<ReceiptVisitPage> ds = new Receipt_Dal().GetVisitPages(list.ToList());

            Prenotazione p = new Prenotazione_Dal().GetItem(list[0].Id_Prenotazione);

            LK_TipoPagamento tp = new LK_TipoPagamento_Dal().GetItem((int)vm.ObjPayment.Id_TipoPagamento);

            args.Data = list[0].Dt_Visita.Date;
            args.TipoPagamento = tp != null ? tp.Descrizione : string.Empty;
            args.Importo = vm.ObjPayment.Importo != null ? (decimal)vm.ObjPayment.Importo : 0;
            args.NrInteri = vm.ObjPayment.Nr_InteriPagati != null ? (int)vm.ObjPayment.Nr_InteriPagati : 0;
            args.NrRidotti = vm.ObjPayment.Nr_RidottiPagati != null ? (int)vm.ObjPayment.Nr_RidottiPagati : 0;
            args.NrOmaggio = new VisitaProgrammata_Dal().GetFreeTickets(vm.PrenotationID, vm.VisitID);
            args.TotInteri = vm.ObjPayment.Importo_Interi != null ? (int)vm.ObjPayment.Importo_Interi : 0;
            args.TotRidotti = vm.ObjPayment.Importo_Ridotti != null ? (int)vm.ObjPayment.Importo_Ridotti : 0;
            args.Protocol = p != null ? p.Protocollo : string.Empty;
            args.Ricevuta = vm.ObjPayment.Ricevuta;
            args.Promemoria = false;

            if (p != null)
            {
                Richiedente r = new Richiedente_Dal().GetItem(p.Id_Richiedente);
                if (r != null)
                {
                    LK_Titolo t = null;

                    if (r.Id_Titolo != null)
                        t = new LK_Titolo_Dal().GetItem((int)r.Id_Titolo);

                    List<TextPartFilterItem> texts = new List<TextPartFilterItem>();
                    texts.Add(new TextPartFilterItem(p.Id_LinguaRisposta, "Arc", 1, 0));
                    texts.Add(new TextPartFilterItem(p.Id_LinguaRisposta, "Brc", 0, 0));
                    texts.Add(new TextPartFilterItem(p.Id_LinguaRisposta, "Crc", 0, 0));
                    texts.Add(new TextPartFilterItem(p.Id_LinguaRisposta, "E", 0, 0));

                    args.Avvisi = (t != null ? t.Sigla + " " : string.Empty) + "<b>" + r.Cognome + " " + r.Nome + "</b>";

                    args.Avvisi += new LK_TestoStandard_Dal().AddParts(texts);
                }
            }

          

            BasePrintPage frm = new wndPrintReceipt();
            frm.PrintReceiptArgs = new List<PrintReceiptArgs>() { args };
            frm.DsPrintReceipt = ds;
            frm.ShowDialog();
            */

            BasePrintPage frm = new wndPrintFattura();
            frm.ObjPagamento = vm.ObjPayment;
            //frm.PrintReceiptArgs = new List<PrintReceiptArgs>() { args };
            //frm.DsPrintReceipt = ds;
            frm.ShowDialog();
        }

        private void btnRefund_Click(object sender, RoutedEventArgs e)
        {
            DoRefund(vm.PaymentRange, vm.PrenotationID, vm.VisitID);
        }

        private void DeleteVisit(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            if (btn != null)
            {
                int visitID = 0;
                int.TryParse(btn.CommandParameter.ToString(), out visitID);
                vm.RemovePaymentRow(visitID);
                vm.UpdateTotals();
            }
        }

        private void grdVisits_EditEnded(object sender, GridViewRowEditEndedEventArgs e)
        {
            bool visitorsAccepted = true;
            decimal remainingAmount = 0;
            LK_Lingua_Dal dalLanguage = new LK_Lingua_Dal();

            if (e.EditAction == Telerik.Windows.Controls.GridView.GridViewEditAction.Commit)
            {
                visitorsAccepted = true;
                //remainingVisitors = (int)nudVisitors.Value - pVM.GetVisitorsCount();
                //if (/*((VisitaPrenotata)e.Row.Item).Nr_Visitatori +*/ pVM.GetVisitorsCount() > nudVisitors.Value)
                //{
                //    e.Handled = true;
                //    MessageBox.Show("Il numero di visitatori impostato per questa visita supera il totale impostato nella prenotazione.");
                //    pVM.RemoveVisitRow(((VisitaPrenotata)e.Row.Item).Id_VisitaPrenotata);
                //    visitorsAccepted = false;
                //    remainingVisitors = 0;
                //    //return;
                //}


                //if (pVM.VisitLanguageAlreadyExists(((VisitaPrenotata)e.Row.Item).Id_Lingua, ((VisitaPrenotata)e.Row.Item).Id_VisitaPrenotata))
                //{

                //    e.Handled = true;
                //    MessageBox.Show("Questa prenotazione contiene già una visita in lingua" + dalLanguage.GetItem(((VisitaPrenotata)e.Row.Item).Id_Lingua).Descrizione);
                //    pVM.RemoveVisitRow(((VisitaPrenotata)e.Row.Item).Id_VisitaPrenotata);
                //    visitorsAccepted = false;
                //    remainingVisitors = 0;
                //    //return;
                //}


                if (((PaymentItem)e.Row.Item).IsEmpty)
                {
                    ((PaymentItem)e.Row.Item).IsEmpty = false;
                    ((PaymentItem)e.Row.Item).IsErasable = true;
                    vm.ObjPayments.ToList().Where(x => x.IsEmpty).ToList().ForEach(y => y.IsEmpty = false);
                    vm.ObjPayments.ToList().Where(x => !x.IsEmpty).ToList().ForEach(y => y.IsErasable = true);
                    vm.AddNewPaymentRow(remainingAmount);
                }

                ((PaymentItem)e.Row.Item).Editing = false;
                vm.UpdateTotals();

            }
            else
            {
                vm.RemovePaymentRow(((PaymentItem)e.Row.Item).ID);
                vm.AddNewPaymentRow(remainingAmount);
            }
        }

        private void grdVisits_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
        {
            ((PaymentItem)e.Row.Item).Editing = true;
        }

        private void cmbPaymentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (((LK_TipoPagamento)e.AddedItems[0]).Id_TipoPagamento != 3 && ((LK_TipoPagamento)e.AddedItems[0]).Id_TipoPagamento != 1)
                    vm.ObjPayment.ValidateAutorizzazione = true;
                else
                    vm.ObjPayment.ValidateAutorizzazione = false;

                //txtalreadyPayd.Text = "Pagamento già effettuato";
                btnPay.Content = "Effettua Pagamento";
                btnDelete.Content = "Annulla Pagamento";
                btnUpdatePayment.Content = "Modifica Pagamento";

                btnCheckOrderStatus.IsEnabled = false;

                //ONLINEPAYMENT
                if (((LK_TipoPagamento)e.AddedItems[0]).Id_TipoPagamento == 1)
                {
                    brdOnlinePayment.Visibility = System.Windows.Visibility.Visible;

                    btnPay.Content = "Crea Ordine";
                    if (vm.ObjPayment != null)
                    {
                        if (vm.ObjPayment.Dt_Pagamento == null)
                        {
                            btnDelete.Content = "Annulla Ordine";
                        }

                        btnCheckOrderStatus.IsEnabled = (vm.ObjPayment.Id_Pagamento != 0 && vm.ObjPayment.Dt_Pagamento == null);
                    }

                    if (vm.ObjPayment.Id_Pagamento > 0)
                    {
                        if (vm.ObjPayment.Dt_Pagamento == null)
                        {
                            vm.ObjPayment.ShowPaymentButton = true;
                            btnUpdatePayment.Content = "Modifica Ordine";
                            //txtalreadyPayd.Text = "Ordine pagamento già effettuato";
                        }
                    }
                }
                else
                {
                    brdOnlinePayment.Visibility = System.Windows.Visibility.Hidden;
                    if (vm.ObjPayment.Id_Pagamento > 0)
                        vm.ObjPayment.ShowPaymentButton = false;
                }

                //ONLINEPAYMENT
                if (vm.ObjPayment.Id_Pagamento == 0)
                    vm.ObjPayment.ShowPaymentButton = true;
            }
        }

        private void btnCheckOrderStatus_Click(object sender, RoutedEventArgs e)
        {
            PaymentOrder paymentOrder = null;

            try
            {
                //todo: aggiungere a tabella pagamento idtransaction
                WebClient webclient = new WebClient();
                string serviceURL = string.Format("{0}/findPaymentOrder/{1}", ConfigurationManager.AppSettings["onlinepaymentwrapperserviceurl"], vm.ObjPayment.NumeroOrdine);
                byte[] data = webclient.DownloadData(serviceURL);
                Stream stream = new MemoryStream(data);
                DataContractJsonSerializer obj = new DataContractJsonSerializer(typeof(PaymentOrder));
                paymentOrder = obj.ReadObject(stream) as PaymentOrder;

                //dalPayment

                if (paymentOrder.status == "nopaymentinfo")
                {
                    MessageBox.Show("L'ordine di pagamento con numero {0} presenta lo status 'PAID' ma per esso non sono state fornite le informazioni relative al pagamento.", "Attenzione");
                    return;
                }

                string retMessage = "";
                if (paymentOrder.status == "PAID")
                {
                    dalPayment.SetPaymentData(paymentOrder.status,
                                            paymentOrder.note,
                                            paymentOrder.orderNumber,
                                            paymentOrder.paymentInfo[0].idTransaction,
                                            paymentOrder.paymentInfo[0].authorization,
                                            paymentOrder.paymentInfo[0].amount,
                                            Convert.ToDateTime(paymentOrder.paymentInfo[0].paymentDate),
                                            paymentOrder.paymentInfo[0].currency,
                                            paymentOrder.paymentInfo[0].resultCode,
                                            out retMessage);

                    //btnRefund.IsEnabled = true;
                    btnPrint.IsEnabled = true;
                }
                else
                {
                    dalPayment.SetPaymentData(paymentOrder.status,
                                              paymentOrder.note,
                                              paymentOrder.orderNumber,
                                              null,
                                              null,
                                              0,
                                              DateTime.Now,
                                              null,
                                              null,
                                              out retMessage);
                }

                if (retMessage != "OK")
                    MessageBox.Show(retMessage, "Attenzione");
                else
                {
                    vm.LoadData();
                    if(vm.ObjPayment.Dt_Pagamento != null)
                        dtpDtPagamento.SelectedTime = vm.ObjPayment.Dt_Pagamento.Value.TimeOfDay;
                }

                if (!string.IsNullOrEmpty(vm.ObjPayment.NumeroOrdine) && vm.ObjPayment.Dt_Pagamento == null)
                {
                    txtIdTransazione.IsReadOnly = false;
                    txtNoteOrdine.IsReadOnly = false;
                    dtpDtPagamento.IsReadOnly = false;
                    btnRegister.IsEnabled = true;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string retMessage = "";

            //LE REGISTRAZIONI "MANUALI" DI AVVENUTO PAGAMENTO ONLINE
            //SONO CONTRADDISTINTE DAL "CODICEESITO" XX

            if (dtpDtPagamento.SelectedDate == null || dtpDtPagamento.SelectedTime == null)
            {
                MessageBox.Show("E' necessario indicare la data/ora in cui è stato effettuato il pagamento online", "Attenzione");
                return;
            }

            if (string.IsNullOrEmpty(txtIdTransazione.Text))
            {
                MessageBox.Show("E' necessario indicare il numero di Rif.(Riferimento) Transazione", "Attenzione");
                return;
            }


            dalPayment.SetPaymentData("PAID",
                                       txtNoteOrdine.Text,
                                       vm.ObjPayment.NumeroOrdine,
                                       txtIdTransazione.Text,
                                       "",
                                       Convert.ToInt32( (vm.ObjPayment.Importo_Interi * 100) + (vm.ObjPayment.Importo_Ridotti * 100)),
                                       Convert.ToDateTime(dtpDtPagamento.SelectedDate).Date + (TimeSpan)dtpDtPagamento.SelectedTime,
                                       "978",
                                       "XX",
                                       out retMessage);

            //btnRefund.IsEnabled = true;
            btnPrint.IsEnabled = true;

            if (retMessage != "OK")
                MessageBox.Show(retMessage, "Attenzione");
            else
            {
                vm.LoadData();
                dtpDtPagamento.SelectedTime = vm.ObjPayment.Dt_Pagamento.Value.TimeOfDay;
                this.btnRegister.IsEnabled = false;
                txtIdTransazione.IsReadOnly = true;
                dtpDtPagamento.IsReadOnly = true;
                txtNoteOrdine.IsReadOnly = true;
            }
        }

        #endregion// Event Handlers


        #region Private Methods

        private bool DoPay()
        {
            //Eventuale messaggio errore
            
            string message = string.Empty;
            bool setRicevuta = false;
            OnLinePaymentLog onLinePaymentLog = null;

            //Risultato validazione
            if (ValidationErrors.Count > 0)
                foreach (ValidationError err in ValidationErrors)
                    message += "\n" + err.ErrorContent.ToString();

            //Se il messaggio di errore non è vuoto
            //notifica all'utente e uscita dal metodo
            if (message.Length > 0)
            {
                message = "Impossibile continuare a causa dei seguenti errori:" + message;
                MessageBox.Show(message, "Errori", MessageBoxButton.OK);
                return false;
            }

            vm.ObjPayment.Id_User = User.Id_User;

            //ONLINEPAYMENT
            if (vm.ObjPayment.Id_TipoPagamento == 1)
            {
                string retMessage = "";
                string numeroOrdine = "";
                
                if (!SendOnLinePaymentRequest(out retMessage, out onLinePaymentLog, out numeroOrdine))
                {
                    MessageBox.Show(retMessage, "Attenzione");
                    return false;
                }

                vm.ObjPayment.NumeroOrdine = numeroOrdine;
                vm.ObjPayment.StatusOrdine = "CREATED";
                vm.ObjPayment.Dt_Pagamento = null;
            }

            if (vm.ObjPayment.Id_Pagamento != 0)
            {
                var dbPayment = vm.GetPayment(vm.ObjPayment.Id_Pagamento);

                if (dbPayment.Id_TipoPagamento == 1 && vm.ObjPayment.Id_TipoPagamento != 1)
                {
                    //ANNULLAMENTO DELL'ORDINE DI PAGAMENTO ONLINE
                    string retMessage = "";
                    OnLinePaymentLog onLinePaymentLogCancel = null;
                    if (!CancelOnLinePaymentRequest(out retMessage, out onLinePaymentLogCancel))
                    {
                        MessageBox.Show(retMessage, "Attenzione");
                        return false;
                    }

                    dalPayment.CancelPayment(dbPayment, onLinePaymentLogCancel, false);
                    //PER IL RECORD PAGAMENTO MODIFICATO DA ORDINE DI PAGAMENTO ONLINE AD ALTRO TIPO DI PAGAMENTO
                    //DOVRA' ESSERE IMPOSTATO IL CODICE DELLA RICEVUTA
                    setRicevuta = true;
                }
            }

            dalPayment.InsertOrUpdate(new List<Pagamento>() { vm.ObjPayment }, onLinePaymentLog, setRicevuta);

            return true;
        }

        private bool SendOnLinePaymentRequest(out string message, out OnLinePaymentLog onLinePaymentLog, out string numeroOrdine)
        {

            message = "";

            onLinePaymentLog = null;

            numeroOrdine = "";

            return onlinePaymentObj.SendOnLinePaymentRequest(vm.ObjPayment, User.Id_User, out message, out onLinePaymentLog, out numeroOrdine);

            //try
            //{

            //    ObservableCollection<V_VisiteProgrammate> list = new ObservableCollection<V_VisiteProgrammate>();

            //    list = new ObservableCollection<V_VisiteProgrammate>(new VisitaProgrammata_Dal().GetVListByIdPrenotazione(vm.ObjPayment.Id_Prenotazione));

            //    Pagamento pagamento = vm.ObjPayment;

            //    V_Prenotazione prenotazione = new Prenotazione_Dal().Get_V_Item(vm.ObjPayment.Id_Prenotazione);

            //    PaymentOrderData paymentOrderData = new PaymentOrderData();

            //    if (string.IsNullOrEmpty(prenotazione.Email))
            //    {
            //        message = "Non è stato fornito l'indirizzo email del richiedente,non è consentito procedere all'inoltro dell'ordine di pagamento online.";
            //        return false;
            //    }
                
            //    string emailRecipient = "";

            //    if (dalParameters.GetItem("sendOLPaymentReqToDummy").Valore == "1")
            //        emailRecipient = dalParameters.GetItem("olp_email_dummy").Valore;
            //    else
            //        emailRecipient = prenotazione.Email;


            //    paymentOrderData.email = emailRecipient;

            //    decimal? importoDec = (pagamento.Importo_Interi + pagamento.Importo_Ridotti);

            //    if (importoDec > 0)
            //    {
            //        //TUTTI GLI IMPORTI PER L'ORDINE DI PAGAMENTO ONLINE DEVONO ESSERE DEFINITI IN CENTESIMI DI EURO
            //        paymentOrderData.amount = Convert.ToInt32(importoDec * 100);

            //        int confirmDays = 0;

            //        if (DateTime.Now.Date < list[0].Dt_Visita.Date)
            //        {
            //            var dateDiff = list[0].Dt_Visita.Date.Subtract(DateTime.Now.Date);

            //            if (dateDiff.Days < 7)
            //                int.TryParse(dalParameters.GetItem("data_visita_minore_7").Valore, out confirmDays);

            //            if (dateDiff.Days > 7 && dateDiff.Days <= 30)
            //                int.TryParse(dalParameters.GetItem("data_visita_fino_30").Valore, out confirmDays);

            //            if (dateDiff.Days > 30)
            //                int.TryParse(dalParameters.GetItem("data_visita_oltre_30").Valore, out confirmDays);

            //            paymentOrderData.expirationDate = DateTime.Now.Date.AddDays(confirmDays).ToString("dd/MM/yyyy");
            //        }


            //        paymentOrderData.surname = Convert.ToString(prenotazione.Cognome);
            //        paymentOrderData.name = Convert.ToString(prenotazione.Nome);
            //        paymentOrderData.reservationNumber = prenotazione.NProtocollo.ToString();

            //        paymentOrderData.visitDate = list[0].Dt_Visita.Date.ToString("dd/MM/yyyy");

            //        if (list.Count > 0)
            //        {
            //            paymentOrderData.reservationRequestDetailList = new ReservationRequestDetailList[list.Count];

            //            for (int i = 0; i < list.Count; i++)
            //            {
            //                paymentOrderData.reservationRequestDetailList[i] = new ReservationRequestDetailList
            //                {
            //                    visitHour = list[i].Ora_Visita,
            //                    language = list[i].LinguaVisita,
            //                    fullTicketNumber = list[i].Nr_Interi != null ? Convert.ToInt32(list[i].Nr_Interi) : 0,
            //                    reducedTicketNumber = list[i].Nr_Ridotti != null ? Convert.ToInt32(list[i].Nr_Ridotti) : 0
            //                };
            //            }
            //        }

            //        string dataToPost = JsonSerializer.Serialize<PaymentOrderData>(paymentOrderData);

            //        WebClient webClient = new WebClient();
            //        webClient.Headers["Content-type"] = "application/json";
            //        webClient.Encoding = Encoding.UTF8;
            //        string numeroOrdine = webClient.UploadString(string.Format("{0}/createPaymentOrder.service", ConfigurationManager.AppSettings["onlinepaymentwrapperserviceurl"]), "POST", dataToPost);

            //        CreatePaymentOrderResultStruct createPaymentOrderResultStruct = JsonSerializer.Deserialize<CreatePaymentOrderResultStruct>(numeroOrdine);
            //        numeroOrdine = createPaymentOrderResultStruct.createPaymentOrderResult;

            //        if (!string.IsNullOrEmpty(numeroOrdine))
            //        {
            //            if (numeroOrdine.IndexOf("KO") != -1)
            //            {
            //                message = string.Format("La richiesta di inoltro dell'ordine di pagamento online non è andata a buon fine. Messaggio ws: {0}", numeroOrdine);
            //                return false;
            //            }
            //            else
            //            {
            //                //SUCCESSO
            //                vm.ObjPayment.Dt_Pagamento = null;
            //                vm.ObjPayment.NumeroOrdine = numeroOrdine;
            //                vm.ObjPayment.StatusOrdine = "CREATED";

            //                onLinePaymentLog = new OnLinePaymentLog
            //                {
            //                    OrderNumber = numeroOrdine,
            //                    Success = true,
            //                    User_ID = User.Id_User,
            //                    EntryType_Code = "RP",
            //                    EntryDate = DateTime.Now
            //                };
            //            }
            //        }
            //        else
            //        {
            //            message = "La richiesta di inoltro dell'ordine di pagamento non ha restituito il numero dell'ordine.";
            //            return false;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}

            //return true;
        }

        private bool CancelOnLinePaymentRequest(out string message, out OnLinePaymentLog onLinePaymentLog)
        {
            message = "";

            onLinePaymentLog = null;

            return onlinePaymentObj.CancelOnLinePaymentRequest(vm.ObjPayment, User.Id_User, out message, out onLinePaymentLog);

            //try
            //{
            //    WebClient webclient = new WebClient();

            //    if (string.IsNullOrEmpty(vm.ObjPayment.NumeroOrdine))
            //    {
            //        message = "Non risulta nessun ordine di pagamento online";
            //        return false;
            //    }

            //    string serviceURL = string.Format("{0}/abortPaymentOrder/{1}", ConfigurationManager.AppSettings["onlinepaymentwrapperserviceurl"], vm.ObjPayment.NumeroOrdine);

            //    string result = webclient.DownloadString(serviceURL);

            //    if (result.IndexOf("KO") != -1)
            //    {
            //        message = string.Format("La richiesta di annullamento dell'ordine di pagamento online non è andata a buon fine. Messaggio ws: {0}", result);
            //        return false;
            //    }

            //    onLinePaymentLog = new OnLinePaymentLog
            //    {
                    
            //        PaymentID = vm.ObjPayment.Id_Pagamento,
            //        OrderNumber = vm.ObjPayment.NumeroOrdine,
            //        User_ID = User.Id_User,
            //        EntryType_Code = "RA",
            //        EntryDate = DateTime.Now
            //    };
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}

            //return true;
        }

        private void TestPaymentConfirmation()
        {
            
            PaymentConfirmationData paymentConfirmationData = new PaymentConfirmationData
            {
                orderNumber = vm.ObjPayment.NumeroOrdine,
                idTransaction = "trans001",
                authorization = "auth001",
                paymentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                currency = "978",
                resultCode = "00"
             };

            var appAmount = Convert.ToInt32((Convert.ToDecimal(vm.ObjPayment.Importo_Interi) + Convert.ToDecimal(vm.ObjPayment.Importo_Ridotti)) * 100);
            paymentConfirmationData.amount = appAmount.ToString();

            string dataToPost = JsonSerializer.Serialize<PaymentConfirmationData>(paymentConfirmationData);

            WebClient webClient = new WebClient();
            webClient.Headers["Content-type"] = "application/json";
            webClient.Encoding = Encoding.UTF8;
            string numeroOrdine = webClient.UploadString(string.Format("{0}/notifyPaymentConfirmation", ConfigurationManager.AppSettings["onlinepaymentwrapperserviceurl"]), "POST", dataToPost);
            
            //SCV_FSP_PaymentServiceBO.PaymentServiceBO a = new PaymentServiceBO();
            //a.notifyPaymentConfirmation(vm.ObjPayment.NumeroOrdine
            //                            , "trans001"
            //                            , "auth001"
            //                            , ((Convert.ToDecimal(vm.ObjPayment.Importo_Interi) + Convert.ToDecimal(vm.ObjPayment.Importo_Ridotti)) * 100).ToString()
            //                            , DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            //                            , "978"
            //                            , "00");
        }

        private void DoRefund(PaymentRange paymentRange, int prenotationID, int? visitID)
        {
            Pagamento v = null;

            switch (paymentRange)
            {
                case PaymentLib.PaymentRange.PerVisit:
                    v = new Pagamento_Dal().GetItemByIdVisitaProgrammata((int)visitID);
                    break;

                case PaymentLib.PaymentRange.PerPrenotation:
                    v = dalPayment.GetItemByIdPrenotazione(prenotationID);
                    break;
            }

            if (v != null)
            {
                BaseDetailPage frm = new wndVisitRefund(v.Id_Pagamento);
                frm.User = User;
                frm.ShowDialog();
                vm.LoadData();
            }
        }

        private void DoDelete()
        {
            //Eventuale messaggio errore
            OnLinePaymentLog onLinePaymentLog = null;
            string message = string.Empty;

            //ONLINEPAYMENT
            //NEL CASO DEL PAGAMENTO ONLINE SE NON C'E' STATA ANCORA UNA NOTIFICA
            //DI PAGAMENTO, OVVERO NEL CASO IN CUI LA DATA DI PAGAMENTO == NULL
            //VIENE RICHIAMATA LA PROCEDURA DI ANNULLAMENTO DELL'ORDINE DI PAGAMENTO
            if (vm.ObjPayment.Id_TipoPagamento == 1)
            {
                string retMessage = "";
                if (!CancelOnLinePaymentRequest(out retMessage, out onLinePaymentLog))
                {
                    MessageBox.Show(retMessage, "Attenzione");
                    return;
                }
            }

            dalPayment.CancelPayment(vm.ObjPayment, onLinePaymentLog, true);
        }

        private void SetLayout(PaymentRange paymentRange)
        {
            lblFullPriceNr.Visibility = System.Windows.Visibility.Collapsed;
            nudFullPriceTickets.Visibility = System.Windows.Visibility.Collapsed;

            lblReducedNr.Visibility = System.Windows.Visibility.Collapsed;
            nudReducedTickets.Visibility = System.Windows.Visibility.Collapsed;

            lblDiscountNr.Visibility = System.Windows.Visibility.Collapsed;
            nudDiscountTickets.Visibility = System.Windows.Visibility.Collapsed;

            lblCumulativiNr.Visibility = System.Windows.Visibility.Collapsed;
            nudCumulativeTickets.Visibility = System.Windows.Visibility.Collapsed;
            
            switch (paymentRange)
            {
                case PaymentLib.PaymentRange.PerPrenotation:
                    lblFullPriceNr.Visibility = System.Windows.Visibility.Visible;
                    lblReducedNr.Visibility = System.Windows.Visibility.Visible;
                    lblDiscountNr.Visibility = System.Windows.Visibility.Visible;
                    lblCumulativiNr.Visibility = System.Windows.Visibility.Visible;
                    break;

                case PaymentLib.PaymentRange.PerVisit:
                    lblFullPriceNr.Visibility = System.Windows.Visibility.Visible;
                    lblReducedNr.Visibility = System.Windows.Visibility.Visible;
                    lblDiscountNr.Visibility = System.Windows.Visibility.Visible;
                    lblCumulativiNr.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        #endregion// Private Methods


        #region Error Handling

        private void Confirm_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _errors == 0;
            e.Handled = true;
        }

        private void Confirm_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                ValidationErrors.Add(e.Error);
                _errors++;
            }
            else
            {
                ValidationErrors.Remove(e.Error);
                _errors--;
            }
        }

        #endregion// Error Handling



        


    }
}
