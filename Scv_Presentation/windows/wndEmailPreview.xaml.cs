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
using Scv_Dal;
using Scv_Entities;
using ShiftManager;
using System.Data.Objects.DataClasses;
using Scv_Model;
using Microsoft.Windows.Controls;
using System.Windows.Threading;
using Presentation.CustomControls.PaymentLib;
using System.Configuration;

namespace Presentation
{
    /// <summary>
    /// Interaction logic for wndEmailPreview.xaml
    /// </summary>
    public partial class wndEmailPreview : BaseDetailPage, INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private Fields

        EmailPreviewViewModel vm = null;
        PaymentViewModel vmP = null;
        int IdTipoRisposta = 0;

        //ONLINEPAYMENT
        Pagamento_Dal dalPayment = new Pagamento_Dal();
       
        OnlinePayment onlinePaymentObj = new OnlinePayment();

        //ONLINEPAYMENT
        bool onlinePaymentEnabled = false;

        #endregion// Private fields

        #region Public Properties


        #endregion // Properties

        #region Constructors

        public wndEmailPreview(EmailPreviewItem item, int idTipoRisposta = 0)
            : base()
        {
            vm = new EmailPreviewViewModel(item);

            InitializeComponent();

            //ONLINEPAYMENT
            if (ConfigurationManager.AppSettings["enableonlinepayment"] != null)
                onlinePaymentEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["enableonlinepayment"]);

            if (idTipoRisposta != 0)
                IdTipoRisposta = idTipoRisposta;

            if (!vm.ObjEmail.UserHasEmail)
            {
                pnlNoEmailMessage.Visibility = System.Windows.Visibility.Visible;
                btnSendEmail.IsEnabled = false;
            }

            btnSendPaymentOrder.IsEnabled = false;

            if (vm.PrenotationID > 0)
            {
                if (onlinePaymentEnabled)
                {
                    vmP = new PaymentViewModel(vm.PrenotationID);
                    btnSendPaymentOrder.IsEnabled = (vmP.ObjPayment != null);
                }

            }

            this.DataContext = vm;
            this.BoundHtmlRichTextBox.DataContext = this.vm;

            btnSendEmail.Click += new RoutedEventHandler(btnSendEmail_Click);
            btnPrint.Click += new RoutedEventHandler(btnPrint_Click);
            btnCopy.Click += new RoutedEventHandler(btnCopy_Click);
        }

        #endregion // Constructors

        #region Event Handling

        void btnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.EmailDestination = Scv_Model.EmailDestination.Send;
            //ONLINEPAYMENT
            DateTime checkDate = new DateTime();
            if (onlinePaymentEnabled)
            {
                if (IdTipoRisposta == 2)
                {
                    //ORDINE DI PAGAMENTO SOLO X TIPO RISPOSTA  "VISITA PROGRAMMATA"
                    vmP = new PaymentViewModel(vm.PrenotationID);
                    if (vmP.ObjPayment != null && vmP.CheckIfOnlinePaymentIsAllowed(out checkDate))
                    {
                        if (DoPay())
                            this.Close();
                        else
                            MessageBox.Show("Problemi nella creazione dell'ordine di pagamento online.");
                    }
                }
                else if (IdTipoRisposta == 5 || IdTipoRisposta == 6)
                {
                    //TIPI RISPOSTA ANNULLAMENTO
                    vmP = new PaymentViewModel(vm.PrenotationID,false);
                    if (vmP.ObjPayment != null && vmP.ObjPayment.Dt_Pagamento == null)
                    {
                        //ANNULLAMENTO ORDINE DI PAGAMENTO
                        if (DoCancelOnlinePaymentOrder())
                            this.Close();
                        else
                            MessageBox.Show("Problemi nell'annullamento dell'ordine di pagamento online.");
                    }
                }
                else
                    this.Close();
            }
            else
                this.Close();
        }

        void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
            //this.EmailDestination = Scv_Model.EmailDestination.Print;
            DoPrint();
        }

        void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
            //this.EmailDestination = Scv_Model.EmailDestination.Copy;
            DoCopy();
        }

        void frm_Closed(object sender, EventArgs e)
        {
        }

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        private void btnSendPaymentOrder_Click(object sender, RoutedEventArgs e)
        {
            //ONLINEPAYMENT
            DateTime checkDate = new DateTime();
            if (!vmP.CheckIfOnlinePaymentIsAllowed(out checkDate))
            {
                MessageBox.Show(string.Format("La creazione dell'ordine di pagamento online è consentita solo per visite con data successiva al {0}.", checkDate.ToString("dd/MM/yyyy"), "Attenzione"));
                return;
            }

            vmP = new PaymentViewModel(vm.PrenotationID);
            if (vmP.ObjPayment != null)
            {
                if (DoPay())
                    this.Close();
            }
        }

        #endregion // Event Handling

        #region Public Methods
        //ONLINEPAYMENT
        private bool SendOnLinePaymentRequest(out string message, out OnLinePaymentLog onLinePaymentLog, out string numeroOrdine)
        {
            message = "";

            onLinePaymentLog = null;

            numeroOrdine = "";

            return onlinePaymentObj.SendOnLinePaymentRequest(vmP.ObjPayment, User.Id_User, out message, out onLinePaymentLog, out numeroOrdine);
        }

        //ONLINEPAYMENT
        private bool DoPay()
        {
            //Eventuale messaggio errore

            string message = string.Empty;
            bool setRicevuta = false;
            OnLinePaymentLog onLinePaymentLog = null;

            vmP.ObjPayment.Id_User = User.Id_User;

            //ONLINEPAYMENT
            if (vmP.ObjPayment.Id_TipoPagamento == 1)
            {
                string retMessage = "";
                string numeroOrdine = "";

                if (!SendOnLinePaymentRequest(out retMessage, out onLinePaymentLog, out numeroOrdine))
                {
                    MessageBox.Show(retMessage, "Attenzione");
                    return false;
                }

                vmP.ObjPayment.NumeroOrdine = numeroOrdine;
                vmP.ObjPayment.StatusOrdine = "CREATED";
                vmP.ObjPayment.Dt_Pagamento = null;
            }

            dalPayment.InsertOrUpdate(new List<Pagamento>() { vmP.ObjPayment }, onLinePaymentLog, setRicevuta);

            return true;
        }

        private bool DoCancelOnlinePaymentOrder()
        {
            //Eventuale messaggio errore
            OnLinePaymentLog onLinePaymentLog = null;
            string message = string.Empty;

            //ONLINEPAYMENT
            //NEL CASO DEL PAGAMENTO ONLINE SE NON C'E' STATA ANCORA UNA NOTIFICA
            //DI PAGAMENTO, OVVERO NEL CASO IN CUI LA DATA DI PAGAMENTO == NULL
            //VIENE RICHIAMATA LA PROCEDURA DI ANNULLAMENTO DELL'ORDINE DI PAGAMENTO
            if (vmP.ObjPayment.Id_TipoPagamento == 1)
            {
                string retMessage = "";
                if (!CancelOnLinePaymentRequest(out retMessage, out onLinePaymentLog))
                {
                    MessageBox.Show(retMessage, "Attenzione");
                    return false;
                }
            }

            dalPayment.CancelPayment(vmP.ObjPayment, onLinePaymentLog, true);

            return true;
        }

        private bool CancelOnLinePaymentRequest(out string message, out OnLinePaymentLog onLinePaymentLog)
        {
            message = "";

            onLinePaymentLog = null;

            return onlinePaymentObj.CancelOnLinePaymentRequest(vmP.ObjPayment, User.Id_User, out message, out onLinePaymentLog);

        }
        
        #endregion// Public Methods

        #region Private Methods

        private void DoPrint()
        {
            bool open = true;
            string windowName = "Email";
            BasePrintPage frm = null;
            foreach (Window wnd in Application.Current.Windows)
            {
                if (wnd.Name == windowName && wnd.IsVisible)
                {

                    frm = (BasePrintPage)wnd;
                    frm.Focus();
                    open = false;
                }
            }
            if (open)
            {
                frm = new wndPrintEmailPreview();
                frm.Name = windowName;
                frm.DsEmailPrint = new List<EmailPreviewItem>() { vm.ObjEmail };
                frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                frm.Closed += new EventHandler(frm_Closed);
                frm.ShowDialog();
            }
        }

        private void DoCopy()
        {
            EmailPreviewItem item = vm.ObjEmail;
            string cl = item.RecipientsLabel + " " + item.Recipients;
            cl += "<br />" + item.SubjectLabel + " " + item.Subject;
            cl += "<br />" + item.Body;

            //cl = cl
            //    .Replace("<br />", Environment.NewLine)
            //    .Replace("<br/>", Environment.NewLine)
            //    .Replace("</b>", "")
            //    .Replace("<b>", "");


            System.Windows.Forms.WebBrowser web = new System.Windows.Forms.WebBrowser();
            web.CreateControl();
            web.DocumentText = cl;
            while (web.DocumentText != cl)
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            web.Document.ExecCommand("SelectAll", false, null);
            web.Document.ExecCommand("Copy", false, null);
            rtb.Paste();

            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);

            try
            {
                Clipboard.SetData(DataFormats.UnicodeText, textRange.Text);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                System.Threading.Thread.Sleep(0);
                try
                {
                    Clipboard.SetData(DataFormats.Text, cl);
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    MessageBox.Show("Can't Access Clipboard");
                }
            }
        }

        #endregion// Private Methods




        #region Overrides

        protected override void SetLayout()
        {

        }

        #endregion// Overrides


    }
}