using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Scv_Model;
using Scv_Dal;
using Scv_Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using System.Net.Mail;
using VisitsSummaryManager;
using Presentation.CustomControls.PaymentLib;
using System.Threading;
using System.Globalization;
using System.Configuration;



namespace Presentation
{
    /// <summary>
    /// Interaction logic for wndScheduleTours.xaml
    /// </summary>
    public partial class wndScheduleTours : BaseDetailPage
    {
        DateTime start = DateTime.Now;

        #region Private Fields

        private ScheduleToursViewModel vM = null;
        private int previousPrenotationStatusID = 0;
        private bool previousConfirmedChecked = false;
        private GridViewRowColor gvColors = null;
        private ObservableCollection<ValidationError> validationErrors = null;
        List<VisitSummaryElement> list = new List<VisitSummaryElement>();
        private int _errors = 0;
        bool enableOnlinePaymentEmailSending = false;
        bool enableOnlinePayment = false;
        int prenotationID = 0;
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

        #endregion// Public Properties


        #region Constructors

        public wndScheduleTours(int detailID, LK_User user)
            : base(detailID)
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(wndScheduleTours_Loaded);
            this.Closing += new CancelEventHandler(wndScheduleTours_Closing);
            User = user;

            vM = new ScheduleToursViewModel(detailID);

            prenotationID = detailID;

            this.DataContext = vM;



            vM.ObjPrenotation.Id_User = User.Id_User;

            if (ConfigurationManager.AppSettings["enableonlinepaymentemailsending"] != null)
                enableOnlinePaymentEmailSending = Convert.ToBoolean(ConfigurationManager.AppSettings["enableonlinepaymentemailsending"]);

            if (ConfigurationManager.AppSettings["enableonlinepayment"] != null)
                enableOnlinePayment = Convert.ToBoolean(ConfigurationManager.AppSettings["enableonlinepayment"]);

            VisitSummaryElement element = null;
            foreach (Hour h in vM.AvailablesHours)
            {
                element = new VisitSummaryElement();
                element.Hour = h.Time;
                list.Add(element);
            }

            UpdateVisitSummary(detailID);

            //Si ricava lo status della prenotazione prima del salvataggio e dell'eventuale
            //cambio di status onde poter decidere se inviare o meno le email di
            //notifica ai richiedenti al momento del salvataggio. Impostarlo in questa
            //fase rende possibile il confronto anche con eventuale cambio di stato
            //eseguito manualmente.
            previousPrenotationStatusID = vM.ObjPrenotation != null && vM.ObjPrenotation.Id_TipoRisposta != null ? (int)vM.ObjPrenotation.Id_TipoRisposta : 0;

            btnGuidesMail.Click += new RoutedEventHandler(btnGuidesMail_Click);
            btnPublicMail.Click += new RoutedEventHandler(btnPublicMail_Click);

            var dtInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            dtInfo.DateTimeFormat.LongDatePattern = "dddd dd/MM/yyyy";
            dtpSelectDate.Culture.DateTimeFormat = dtInfo.DateTimeFormat;

            if (vM.ObjPrenotation.Id_TipoRisposta == 2)
            {
                btnPublicMail.IsEnabled = true;
            }
            else
            {
                btnPublicMail.IsEnabled = false;
            }

            if (vM.ObjClosingDateResult.Message.Length > 0)
                MessageBox.Show(vM.ObjClosingDateResult.Message, "Chiusura Necropoli");

        }

        #endregion// Constructors



        #region Events Handlers

        //Visite prenotate
        private void grdPrenotationVisits_EditEnded(object sender, GridViewRowEditEndedEventArgs e)
        {
            if (e.EditAction == Telerik.Windows.Controls.GridView.GridViewEditAction.Commit)
            {
                //questo evento triggera solo il ricalcolo dei visitatori rimanenti. Non viene
                //utilizzata la propetà "Nr_Visitatori" della VisitaProgrammata in quanto
                //triggera l'evento ad ogni pressione della tastiera, producendo un evento per
                //ogni numero premuto. Questo renderebbe impossibile un unico ricalcolo per numeri 
                //a due o più cifre. In questo modo, invece, si triggerà l'evento solo dopo
                //che l'utente ha terminato l'intera modifica del numero

                PropertyChangedEventArgs args = null;

                //E' importante rispettare questa sequenza di choamate (prima Id_Lingua e poi Nr_Visitatori)
                //in quanto nel caso che al termine dell'editing vengano cambiati entrambi, se si invia prima il
                //numero visitatori, se si tratta di una visita disabilitata perchè esiste già una lingua simile,
                //il programma reintegra il nuovo valore in una visita con la stessa lingua, allo scopo di lasciare
                //disattivata sempre e solo una sola visita. Come risultato si avrà che cambiando il numero di
                //visitatori da una visita disattivata non si ottiene alcun cambiamento.
                //Invece in questo modo, verrebbe cambiata prima la lingua, con conseguente attivazione della visita,
                //e quindi il numero dei suoi visitatori.
                int oldLingua = 0;
                int.TryParse(e.OldValues["Id_Lingua"].ToString(), out oldLingua);
                if (((V_VisitePrenotate)e.NewData).Id_Lingua != oldLingua)
                {
                    int idVisitaPrenotata = ((V_VisitePrenotate)e.NewData).Id_VisitaPrenotata;
                    int idLingua = ((V_VisitePrenotate)e.NewData).Id_Lingua;
                    args = new PropertyChangedEventArgs("Id_Lingua");
                    vM.OnPrenotationVisitNestedPropertyChanged(((V_VisitePrenotate)e.Row.Item), args);
                    foreach (V_VisiteProgrammate vp in vM.SrcVisiteProgrammateAll.Where(vpx => vpx.Id_VisitaPrenotata == idVisitaPrenotata))
                    {
                        vp.Id_Lingua = idLingua;
                    }
                }

                int oldVisitatori = 0;
                int.TryParse(e.OldValues["Nr_Visitatori"].ToString(), out oldVisitatori);
                if (((V_VisitePrenotate)e.NewData).Nr_Visitatori != oldVisitatori)
                {
                    args = new PropertyChangedEventArgs("Nr_Visitatori");
                    vM.OnPrenotationVisitNestedPropertyChanged(((V_VisitePrenotate)e.Row.Item), args);
                }

                ((V_VisitePrenotate)e.Row.Item).Editing = false;
            }
        }

        private void grdPrenotationVisits_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
        {
            ((V_VisitePrenotate)e.Row.Item).Editing = true;
        }

        private void deletePrenotationVisit(object sender, RoutedEventArgs e)
        {
            if (vM.IsLoked)
                return;

            RadButton btn = sender as RadButton;
            if (btn != null)
            {
                int visitID = 0;
                int.TryParse(btn.CommandParameter.ToString(), out visitID);
                vM.RemovePrenotationVisitRow(visitID, true);
            }
        }






        //Visite programmate
        private void grdVisiteProgrammate_EditEnded(object sender, GridViewRowEditEndedEventArgs e)
        {
            try
            {
                V_VisiteProgrammate visitaProg = ((V_VisiteProgrammate)e.Row.Item);

                visitaProg.Id_Lingua = vM.ObjSelectedPrenotationVisit.Id_Lingua;

                short oldInteri = 0;
                short oldRidotti = 0;
                short oldOmaggio = 0;
                short oldScontati = 0;
                short oldCumulativi = 0;

                short newInteri = 0;
                short newRidotti = 0;
                short newOmaggio = 0;
                short newScontati = 0;
                short newCumulativi = 0;

                string oldHour = string.Empty;

                if (e.OldValues["Nr_Interi"] != null)
                    short.TryParse(e.OldValues["Nr_Interi"].ToString(), out oldInteri);
                if (e.OldValues["Nr_Ridotti"] != null)
                    short.TryParse(e.OldValues["Nr_Ridotti"].ToString(), out oldRidotti);
                if (e.OldValues["Nr_Omaggio"] != null)
                    short.TryParse(e.OldValues["Nr_Omaggio"].ToString(), out oldOmaggio);
                if (e.OldValues["Nr_Scontati"] != null)
                    short.TryParse(e.OldValues["Nr_Scontati"].ToString(), out oldScontati);
                if (e.OldValues["Nr_Cumulativi"] != null)
                    short.TryParse(e.OldValues["Nr_Cumulativi"].ToString(), out oldScontati);

                oldHour = e.OldValues["Ora_Visita"] != null ? e.OldValues["Ora_Visita"].ToString() : string.Empty;

                newInteri = ((V_VisiteProgrammate)e.NewData).Nr_Interi != null ? (short)((V_VisiteProgrammate)e.NewData).Nr_Interi : (short)0;
                newRidotti = ((V_VisiteProgrammate)e.NewData).Nr_Ridotti != null ? (short)((V_VisiteProgrammate)e.NewData).Nr_Ridotti : (short)0;
                newOmaggio = ((V_VisiteProgrammate)e.NewData).Nr_Omaggio != null ? (short)((V_VisiteProgrammate)e.NewData).Nr_Omaggio : (short)0;
                newScontati = ((V_VisiteProgrammate)e.NewData).Nr_Scontati != null ? (short)((V_VisiteProgrammate)e.NewData).Nr_Scontati : (short)0;
                newCumulativi = ((V_VisiteProgrammate)e.NewData).Nr_Cumulativi != null ? (short)((V_VisiteProgrammate)e.NewData).Nr_Cumulativi : (short)0;

                /*AUTORIZZAZIONE PER RIDOTTI E OMAGGIO
                if (((V_VisiteProgrammate)e.Row.Item).Nr_Ridotti != null && ((V_VisiteProgrammate)e.Row.Item).Nr_Ridotti > 0 && string.IsNullOrEmpty(vM.ObjPrenotation.Autorizzazione))
                {
                    MessageBox.Show("Non è consentita l'assegnazione di biglietti ridotti senza aver compilato il campo 'Autorizzazione'.\nPremere il tasto ESC, quindi inserire l'autorizzazione");
                    ((V_VisiteProgrammate)e.Row.Item).Nr_Ridotti = (short?)null;
                    vM.CanSave = !vM.IsLoked && !vM.EmptyPrenotationVisitExists();
                    return;
                }

                if (((V_VisiteProgrammate)e.Row.Item).Nr_Omaggio != null && ((V_VisiteProgrammate)e.Row.Item).Nr_Omaggio > 0 && string.IsNullOrEmpty(vM.ObjPrenotation.Autorizzazione))
                {
                    MessageBox.Show("Non è consentita l'assegnazione di biglietti omaggio senza aver compilato il campo 'Autorizzazione'.\nPremere il tasto ESC, quindi inserire l'autorizzazione");
                    ((V_VisiteProgrammate)e.Row.Item).Nr_Omaggio = (short?)null;
                    vM.CanSave = !vM.IsLoked && !vM.EmptyPrenotationVisitExists();
                    return;
                }
                */
                bool cont = false;

                if ((vM.GetScheduledVisitsVisitorsCount(visitaProg) + vM.GetEvidenzeGiornaliereGroupHeaderVisitorsCount(visitaProg)) > vM.maxVisitorsPerGroup && (vM.GetScheduledVisitsVisitorsCount(visitaProg) + vM.GetEvidenzeGiornaliereGroupHeaderVisitorsCount(visitaProg)) < vM.maxVisitorsPerDay)
                {
                    var result = MessageBox.Show(string.Format("Per il gruppo a cui si sta per associare questi visitatori verrebbero superate le {0} unità. Continuare?", vM.maxVisitorsPerGroup.ToString()), "Attenzione", MessageBoxButton.YesNo);
                    cont = (result == MessageBoxResult.Yes);
                    if (!cont)
                    {
                        if (oldInteri != newInteri)
                            ((V_VisiteProgrammate)e.Row.Item).Nr_Interi = oldInteri > 0 ? oldInteri : (short?)null;

                        if (oldRidotti != newRidotti)
                            ((V_VisiteProgrammate)e.Row.Item).Nr_Ridotti = oldRidotti > 0 ? oldRidotti : (short?)null;

                        if (oldOmaggio != newOmaggio)
                            ((V_VisiteProgrammate)e.Row.Item).Nr_Omaggio = oldOmaggio > 0 ? oldOmaggio : (short?)null;

                        if (oldScontati != newScontati)
                            ((V_VisiteProgrammate)e.Row.Item).Nr_Scontati = oldScontati > 0 ? oldScontati : (short?)null;

                        if (oldCumulativi != newCumulativi)
                            ((V_VisiteProgrammate)e.Row.Item).Nr_Cumulativi = oldCumulativi > 0 ? oldCumulativi : (short?)null;
                    }
                }

                if ((vM.GetScheduledVisitsVisitorsCount(visitaProg) + vM.GetEvidenzeGiornaliereVisitorsCount()) > vM.maxVisitorsPerDay)
                {

                    var result = MessageBox.Show(string.Format("Per la data selezionata per la programmazione di questa visita verrebbero superate le {0} unità. Continuare?", vM.maxVisitorsPerDay.ToString()), "Attenzione", MessageBoxButton.YesNo);
                    cont = (result == MessageBoxResult.Yes);
                    if (!cont)
                    {
                        if (oldInteri != newInteri)
                            ((V_VisiteProgrammate)e.Row.Item).Nr_Interi = oldInteri > 0 ? oldInteri : (short?)null;

                        if (oldRidotti != newRidotti)
                            ((V_VisiteProgrammate)e.Row.Item).Nr_Ridotti = oldRidotti > 0 ? oldRidotti : (short?)null;

                        if (oldOmaggio != newOmaggio)
                            ((V_VisiteProgrammate)e.Row.Item).Nr_Omaggio = oldOmaggio > 0 ? oldOmaggio : (short?)null;

                        if (oldScontati != newScontati)
                            ((V_VisiteProgrammate)e.Row.Item).Nr_Scontati = oldScontati > 0 ? oldScontati : (short?)null;

                        if (oldCumulativi != newCumulativi)
                            ((V_VisiteProgrammate)e.Row.Item).Nr_Cumulativi = oldCumulativi > 0 ? oldCumulativi : (short?)null;
                    }
                }

                string retMessage = "";
                bool check = vM.CheckVisitaProgrammata(visitaProg, out retMessage);
                if (!check)
                {
                    MessageBox.Show(retMessage);
                    visitaProg.Ora_Visita = oldHour;
                    if (
                        !new VisitaProgrammata_Dal().CheckVisitaProgrammataPagamentoParziale(visitaProg.Id_VisitaProgrammata)
                        &&
                        new Pagamento_Dal().GetItemByIdPrenotazione(vM.ObjPrenotation.Id_Prenotazione) == null
                        )
                    {
                        vM.RemoveScheduledVisitRow(visitaProg.Id_VisitaProgrammata);
                        vM.CanSave = !vM.IsLoked && !vM.EmptyPrenotationVisitExists();
                    }

                    return;
                }
                else
                {
                    if (retMessage.Length > 0)
                    {
                        if (MessageBox.Show(retMessage, "", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        {
                            vM.RemoveScheduledVisitRow(visitaProg.Id_VisitaProgrammata);
                            vM.CanSave = !vM.IsLoked && !vM.EmptyPrenotationVisitExists();
                            return;
                        }
                    }
                }

                vM.SetConflicts();

                if (cont)
                {
                    if (e.EditAction == GridViewEditAction.Commit)
                    {
                        if (visitaProg.IsCancelable)
                            visitaProg.IsCancelable = false;

                        visitaProg.Editing = false;

                        visitaProg.Nr_Interi = visitaProg.Nr_Interi > 0 ? visitaProg.Nr_Interi : (short?)null;
                        visitaProg.Nr_Ridotti = visitaProg.Nr_Ridotti > 0 ? visitaProg.Nr_Ridotti : (short?)null;
                        visitaProg.Nr_Omaggio = visitaProg.Nr_Omaggio > 0 ? visitaProg.Nr_Omaggio : (short?)null;
                        visitaProg.Nr_Scontati = visitaProg.Nr_Scontati > 0 ? visitaProg.Nr_Scontati : (short?)null;
                        visitaProg.Nr_Cumulativi = visitaProg.Nr_Cumulativi > 0 ? visitaProg.Nr_Cumulativi : (short?)null;
                    }
                    else if (e.EditAction == GridViewEditAction.Cancel && visitaProg.IsCancelable)
                    {
                        vM.RemoveScheduledVisitRow(((V_VisiteProgrammate)e.Row.Item).Id_VisitaProgrammata);
                    }

                    //Si disabilita il pulsante dei pagamenti prenotazione perchè per accedere ai pagamenti dopo
                    //aver variato qualsiasi cosa nelle visite programmate è necessario salvare prima di accedere
                    //al pagamento.
                    vM.CanPayFull = false;
                }

                vM.CanSave = !vM.IsLoked && !vM.EmptyPrenotationVisitExists();
            }
            catch { }

        }

        private void grdVisiteProgrammate_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
        {
            vM.CanSave = false;
            var visitaProg = ((V_VisiteProgrammate)e.Row.Item);
            visitaProg.Editing = true;
        }

        private void btnAddVP_Click(object sender, RoutedEventArgs e)
        {
            if (!vM.AddNewScheduledVisitRow())
            {
                MessageBox.Show("Tutti i visitatori previsti per la visita prenotata selezionata sono stati già assegnati ad una o più visite programmate.");
            }
            else
            {
                grdVisiteProgrammate.CurrentItem = grdVisiteProgrammate.Items[grdVisiteProgrammate.Items.Count - 1];
                grdVisiteProgrammate.CurrentCellInfo = new GridViewCellInfo(grdVisiteProgrammate.CurrentItem, grdVisiteProgrammate.Columns["Ora_Visita"]);
                grdVisiteProgrammate.BeginEdit();
            }
        }



        //Lavorazione, posta e pagamento
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string message = string.Empty;

            if (ValidationErrors.Count > 0)
                foreach (ValidationError err in ValidationErrors)
                    message += "\n" + err.ErrorContent.ToString();

            vM.CheckConflicts();

            ConflictResult r = vM.GetConflicts();
            if (r.Conflict == ConflictType.Unacceptable)
            {
                //message += "\n- " + "Sono presenti " + r.Conflicts.Count.ToString() + " visite in conflitto ora/lingua. Risolvere prima tutti i conflitti.";
            }


            if (!vM.CheckAccountingData())
            {
                message += "\n- " + "Essendo già stato effettuato un pagamento non è consentito modificare il numero dei biglietti interi o ridotti.";
            }

            if (message.Length > 0)
            {
                message = "Impossibile continuare a causa dei seguenti errori:" + message;
                MessageBox.Show(message, "Errori", MessageBoxButton.OK);
                return;
            }

            //TODO: NEL CASO SIANO STATI EFFETTUATI PAGAMENTI PARZIALI O UN PAGAMENTO TOTALE,
            //PRIMA DI CONSENTIRE IL SALVATAGGIO, VERIFICARE CHE I DATI "CONTABILI"
            //NON SIANO STATI MODIFICATI: 
            //NEL CASO DI PAGAMENTO/I PARZIALE/I,             
            //PER LA SINGOLA VISITA NON POTRANNO ESSERE MODIFICATI IL NUMERO DI BIGLIETTI INTERI E IL NUMERO
            //DI BIGLIETTI RIDOTTI; 
            //NEL CASO DI PAGAMENTO TOTALE,
            //PER L'INTERA PRENOTAZIONE LA SOMMA DEI BIGLIETTI INTERI E LA SOMMA DEI BIGLIETTI RIDOTTI 
            //DOVRANNO COINCIDERE CON LE STESSE SOMME PRIMA DELLA MODIFICA

            CheckChangedGuides();

            VisitaProgrammata_Dal VProg_dal = new VisitaProgrammata_Dal();

            vM.ObjSelectedPrenotationVisit = (V_VisitePrenotate)grdPrenotationVisits.CurrentItem;

            //Si imposta lo stato di conferma della prenotazione
            vM.ObjPrenotation.Fl_Conferma =
                (vM.ObjPrenotation.Id_TipoRisposta == 10)
                ||
                vM.CheckPayment(vM.ObjPrenotation.Id_Prenotazione)
                ;


            //Si salva la prenotazione ricavandone il nuovo status
            bool saveOk = true;
            PrenotationStatus prenotationStatus = VProg_dal.InsertOrUpdate(vM.CurrentDate, vM.ObjPrenotation, vM.SrcPrenotationVisits, vM.SrcVisiteProgrammateAll, vM.SrcEvidenzeGiornaliere, out saveOk);

            if (!saveOk)
            {
                MessageBox.Show("Salvataggio non effettuato. Provare a rieffettuare l'operazione.", "Attenzione", MessageBoxButton.OK);
                return;
            }

            //Si crea una prenotazione temporanea alla quale si aggiorna il tipo risposta, da usare come
            //argomento per CheckSendPetitionerMail() al posto della prenotazione vera. Ci sono infatti
            //alcune situazioni in cui la sottostante chiamata a vM.LoadMaster() non carica l'attuale stato
            //della prenotazione (dopo il salvataggio), provocando un errore nella costruzione del corpo
            //email in caso di risposta negativa, in quanto verrebbe inviato il tipo risposta sbagliato.
            //Nella fattispecie, se si sceglie una risposta negativa alla prima lavorazione di una prenotazione,
            //questa non ha visite programmate. In questo caso la data corrente viene reimpostata alla prima
            //fra le due date della richiesta. Questo comporta un trigger nel viewmodel che, vedendo la prenotazione
            //senza visite, reimposta a 9 (da lavorare) il tipo di risposta della stessa. Il fatto non provoca
            //problemi in quanto la finestra si richiude subito dopo il salvataggio, ma per una prenotazione
            //senza visite l'Id_TipoRisposta della prenotazione ObjPrenotation può essere non attendibile dopo una
            //chiamata a LoadMaster, se non ci sono visite.
            Prenotazione prenotation = vM.ObjPrenotation;
            prenotation.Id_TipoRisposta = prenotationStatus.ResponsTypeID;

            //Si ricaricano i dati appena salvati
            //if (!CloseOnSave)
            vM.LoadMaster(DetailID);

            //Si aggiorna il VisitSummary
            if (!CloseOnSave)
                UpdateVisitSummary(DetailID);

            //Viene invocata la procedura di creazione email.
            //Dal momento che questa procedura è automatizzata anche sulla base delle differenze
            //tra status precedente e attuale della prenotazione, essa viene invocata PRIMA
            //di reimpostare previousPrenotationStatusID e previousConfirmedChecked che, altrimenti,
            //risulterebbero sempre uguali a quelli attuali, e la procedura automatica non partirebbe mai.
            //if(vM.SrcVisiteProgrammateAll.ToList().Count > 0)
            CheckSendPetitionerMail((TipoRisposta)previousPrenotationStatusID, (TipoRisposta)prenotationStatus.ResponsTypeID, previousConfirmedChecked, prenotationStatus.ConfirmationChecked, prenotation, vM.SrcEvidenzeGiornaliere.Where(x => vM.SrcVisiteProgrammateAll.Select(y => y.Id_VisitaProgrammata).Contains(x.Id_VisitaProgrammata)).ToList());

            //Vengono imppostati previousPrenotationStatusID e previousConfirmedChecked.
            //Notare che l'impostazione di queste due variabili è resa inutile se CloseOnSave è impostato a true,
            //in quanto appena dopo l'assegnazione, il form si chiude. Attualmente (11/07/2013), su richiesta del cliente
            //il form si chiude SEMPRE. Tuttavia viene lasciata l'assegnazione in caso di sviluppi futuri
            //o cambiamento di idea del cliente.
            previousPrenotationStatusID = vM.ObjPrenotation != null && vM.ObjPrenotation.Id_TipoRisposta != null ? (int)vM.ObjPrenotation.Id_TipoRisposta : 0;
            previousConfirmedChecked = vM.ObjPrenotation != null && vM.ObjPrenotation.Fl_Conferma != null ? (bool)vM.ObjPrenotation.Fl_Conferma : false;

            if (vM.ObjPrenotation.Id_TipoRisposta == 2)
            {
                btnPublicMail.IsEnabled = true;
            }
            else
            {
                btnPublicMail.IsEnabled = false;
            }


            if (CloseOnSave)
            {
                this.SavedObj = vM.GetV_Prenotazione();
                this.Close();
            }
        }

        private void dtpSelectDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (vM.Loading) return;

            if (vM.ObjClosingDateResult.Message.Length > 0)
                MessageBox.Show(vM.ObjClosingDateResult.Message, "Chiusura Necropoli");

            vM.ObjPrenotation.Id_TipoRisposta = vM.LastSuitableResponseTypeID;
            vM.LoadEvidenzeGiornaliere();
            UpdateVisitSummary(DetailID);

        }

        private void btnGuidesMail_Click(object sender, RoutedEventArgs e)
        {
            CheckChangedGuides();

            VisitaProgrammata_Dal dal = new VisitaProgrammata_Dal();
            List<V_EvidenzeGiornaliere> visits = new List<V_EvidenzeGiornaliere>();
            List<string> groups = new List<string>();
            foreach (V_EvidenzeGiornaliere eg in vM.SrcEvidenzeGiornaliere.OrderBy(x => x.Dt_Visita).ThenBy(x => x.Ora_Visita).ToList())
            {
                if (!groups.Contains(eg.Dt_Visita.ToShortDateString() + eg.Ora_Visita))
                {
                    groups.Add(eg.Dt_Visita.ToShortDateString() + eg.Ora_Visita);
                    visits.Add(eg);
                }
            }
            dal.UpdateVisitsGuides(visits);
            SendGuidesMail();
        }

        private void btnPublicMail_Click(object sender, RoutedEventArgs e)
        {
            PrenotationStatus prenotationStatus = new PrenotationStatus();
            prenotationStatus.ResponsTypeID = vM.ObjPrenotation.Id_TipoRisposta != null ? (int)vM.ObjPrenotation.Id_TipoRisposta : 0;
            prenotationStatus.ConfirmationChecked = vM.ObjPrenotation.Fl_Conferma != null ? (bool)vM.ObjPrenotation.Fl_Conferma : false; ;

            if (
                vM.SrcVisiteProgrammateAll.Count > 0
                ||
                (
                    vM.ObjPrenotation.Id_TipoRisposta == 1
                    ||
                    vM.ObjPrenotation.Id_TipoRisposta == 5
                    ||
                    vM.ObjPrenotation.Id_TipoRisposta == 6
                    ||
                    vM.ObjPrenotation.Id_TipoRisposta == 7
                    ||
                    vM.ObjPrenotation.Id_TipoRisposta == 8
                )
            )

                SendPetitionerMail(vM.ObjPrenotation, vM.SrcEvidenzeGiornaliere.Where(x => vM.SrcVisiteProgrammateAll.Select(y => y.Id_VisitaProgrammata).Contains(x.Id_VisitaProgrammata)).ToList(), true);

            //ONLINEPAYMENT
            //Si imposta lo stato di conferma della prenotazione
            vM.ObjPrenotation.Fl_Conferma =
                (vM.ObjPrenotation.Id_TipoRisposta == 10)
                ||
                vM.CheckPayment(vM.ObjPrenotation.Id_Prenotazione)
                ;
            new Prenotazione_Dal().ChangePrenotationConfirmation(vM.ObjPrenotation);

            vM.LoadMaster(DetailID);
            vM.LoadAvailableResponseTypes();

            grdVisiteGiornaliere.SelectedItems.Clear();
            vM.CanPaySingle = false;
        }

        private void btnPayPrenotation_Click(object sender, RoutedEventArgs e)
        {
            DoPay(PaymentRange.PerPrenotation, null);
        }

        private void btnPayVisit_Click(object sender, RoutedEventArgs e)
        {
            int id = 0;

            if (((RadButton)sender).CommandParameter != null)
                int.TryParse(((RadButton)sender).CommandParameter.ToString(), out id);
            else
                if (grdVisiteGiornaliere.SelectedItems.Count > 0)
                    id = ((V_EvidenzeGiornaliere)grdVisiteGiornaliere.SelectedItems[0]).Id_VisitaProgrammata;

            if (id > 0)
                DoPay(PaymentRange.PerVisit, id);
        }

        private void btnPrintRemainder_Click(object sender, RoutedEventArgs e)
        {
            PrintReceiptArgs args = new PrintReceiptArgs();
            ObservableCollection<V_VisiteProgrammate> list = new ObservableCollection<V_VisiteProgrammate>(new VisitaProgrammata_Dal().GetVListByIdPrenotazione(vM.ObjPrenotation.Id_Prenotazione));
            List<ReceiptVisitPage> ds = new Receipt_Dal().GetVisitPages(list.ToList());

            args.Data = list.Count > 0 ? list[0].Dt_Visita : DateTime.MinValue;
            args.Protocol = vM.ObjPrenotation.Protocollo;
            args.Promemoria = true;

            Richiedente r = new Richiedente_Dal().GetItem(vM.ObjPrenotation.Id_Richiedente);
            if (r != null)
            {
                LK_Titolo t = null;

                if (r.Id_Titolo != null)
                    t = new LK_Titolo_Dal().GetItem((int)r.Id_Titolo);

                List<TextPartFilterItem> texts = new List<TextPartFilterItem>();
                texts.Add(new TextPartFilterItem(vM.ObjPrenotation.Id_LinguaRisposta, "Arc", 1, 0));
                texts.Add(new TextPartFilterItem(vM.ObjPrenotation.Id_LinguaRisposta, "Brc", 0, 0));
                texts.Add(new TextPartFilterItem(vM.ObjPrenotation.Id_LinguaRisposta, "Crc", 0, 0));
                texts.Add(new TextPartFilterItem(vM.ObjPrenotation.Id_LinguaRisposta, "E", 0, 0));

                string nome = string.IsNullOrEmpty(r.Nome) ? "" : r.Nome;

                args.Avvisi = (t != null ? t.Sigla + " " : string.Empty) + "<b>" + r.Cognome + " " + nome + "</b>";

                args.Avvisi += new LK_TestoStandard_Dal().AddParts(texts);
            }

            BasePrintPage frm = new wndPrintReceipt();
            frm.PrintReceiptArgs = new List<PrintReceiptArgs>() { args };
            frm.DsPrintReceipt = ds;
            frm.ShowDialog();
        }

        private void btnRefund_Click(object sender, RoutedEventArgs e)
        {
            int id = 0;
            int.TryParse(((RadButton)sender).CommandParameter.ToString(), out id);
            DoRefund(id);
        }

        protected void wndScheduleTours_Loaded(object sender, RoutedEventArgs e)
        {
            vM.Loading = false;
        }

        protected void wndScheduleTours_Closing(object sender, CancelEventArgs e)
        {

        }


        #endregion// Events Handlers



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



        #region Methods

        private void CheckChangedGuides()
        {
            foreach (V_EvidenzeGiornaliere o in vM.SrcEvidenzeGiornaliere)
            {
                foreach (V_EvidenzeGiornaliere old in vM.SrcOldEvidenzeGiornaliere)
                    if (old.Id_VisitaProgrammata == o.Id_VisitaProgrammata)
                        if (o.Id_Guida != old.Id_Guida)
                            o.Dt_InvioAvviso = (DateTime?)null;
            }
        }

        private void SendGuidesMail()
        {
            LK_TestoStandard_Dal dalText = new LK_TestoStandard_Dal();
            Guida_Dal dalGuide = new Guida_Dal();
            GuidaLingua_Dal dalGuideLanguage = new GuidaLingua_Dal();

            MailCreationResult result = new MailCreationResult();
            Mailer mailer = new Mailer();

            string recipientsLabel = string.Empty;
            string subjectLabel = string.Empty;
            string testText = string.Empty;
            string subject = string.Empty;
            int languageID = 0;
            V_GuidaLingua gl = null;

            List<Guida> mailList = new List<Guida>();
            Guida g = null;
            List<int> guidesIDs = new List<int>();
            List<V_EvidenzeGiornaliere> guideVisits = null;

            foreach (V_EvidenzeGiornaliere evg in vM.SrcEvidenzeGiornaliere)
                if (evg.Fl_AvvisaGuida == true)
                {
                    if (!guidesIDs.Contains((int)evg.Id_Guida))
                    {
                        guidesIDs.Add((int)evg.Id_Guida);
                        g = dalGuide.GetItem((int)evg.Id_Guida);
                        if (g != null)
                            mailList.Add(g);
                    }
                }

            if (mailList.Count > 0)
                foreach (Guida gd in mailList)
                {
                    guideVisits = vM.SrcEvidenzeGiornaliere.Where(x => x.Id_Guida == gd.Id_Guida).ToList();
                    gl = dalGuideLanguage.GetItemsByGuideID(gd.Id_Guida).FirstOrDefault(x => x.Fl_Madre == true);
                    if (gl == null)
                        gl = dalGuideLanguage.GetItemsByGuideID(gd.Id_Guida).ToList()[0];

                    if (gl != null)
                    {
                        languageID = 1;//Italiano fixed
                        recipientsLabel = dalText.GetText(languageID, "FAX_TO").Testo;//1 = Italiano
                        subjectLabel = dalText.GetText(languageID, "FAX_SUBJ").Testo;//1 = Italiano
                        subject = "Assegnazione visite Castello Odescalchi";

                        result = mailer.CreateGuideVisitNotice(guideVisits, guideVisits[0].Dt_Visita.Month, guideVisits[0].Dt_Visita.Year);
                        EmailPreviewItem item = new EmailPreviewItem(recipientsLabel, gd.Email, gd.Cognome + " " + gd.Nome, subjectLabel, subject, ".jpg", result.Text);
                        BaseDetailPage frm = new wndEmailPreview(item);
                        frm.ShowDialog();
                        if (frm.DialogResult == true)
                        {
                            switch (frm.EmailDestination)
                            {
                                case Scv_Model.EmailDestination.Send:
                                    try
                                    {
                                        MailMessage message = new MailMessage("guideFrom@email.it", gd.Email, subject, result.Text);
                                        if (message != null)
                                        {
                                            DoSendMail(message, vM.SrcEvidenzeGiornaliere.Where(x => x.Fl_AvvisaGuida == true && x.Id_Guida == gd.Id_Guida).ToList(), vM.ForwardToSenderGuide, DateSaveTarget.Guide, null, gd.Cognome + " " + gd.Nome);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        string message = "Si è verificato un errore nella creazione del messaggio Email";

                                        if (e.TargetSite.ReflectedType.Name == "MailAddressParser")
                                            message += ":\nIl richiedente ha un indirizzo di email non valido.";

                                        MessageBox.Show(message);
                                    }
                                    break;

                                case Scv_Model.EmailDestination.Print:

                                    break;

                                case Scv_Model.EmailDestination.Copy:

                                    break;
                            }
                        }
                        else
                        {
                            result.success = false;
                            result.ErrorList += "\nInvio dell'avviso annullato dall'utente.";
                        }
                    }
                }
            else
            {
                result.success = false;
                result.ErrorList += "\nNon ci sono guide selezionate per l'avviso.";
            }

            if (!result.success)
                MessageBox.Show(result.ErrorList);
            else
                MessageBox.Show("Gli avvisi sono stati spediti con successo");

            vM.LoadMaster(DetailID);
        }

        private void CheckSendPetitionerMail(TipoRisposta previousStatus, TipoRisposta currentStatus, bool previousConfirmStatus, bool currentConfirmStatus, Prenotazione prenotation, List<V_EvidenzeGiornaliere> visits)
        {
            if (
                    (
                        previousStatus != currentStatus
                        ||
                        previousConfirmStatus != currentConfirmStatus
                    )
                    &&
                    (
                        vM.SrcVisiteProgrammateAll.Count > 0
                        ||
                        (
                            currentStatus == TipoRisposta.RichiestaIncompleta
                            ||
                            currentStatus == TipoRisposta.AnnullataDaUfficioScavi
                            ||
                            currentStatus == TipoRisposta.NecropoliChiusa
                            ||
                            currentStatus == TipoRisposta.VisiteComplete
                        )
                    )
                )
                SendPetitionerMail(prenotation, visits, currentConfirmStatus);
        }

        private void SendPetitionerMail(Prenotazione prenotation, List<V_EvidenzeGiornaliere> visits, bool currentConfirmStatus)
        {
            VisitaProgrammata_Dal dalVisit = new VisitaProgrammata_Dal();
            Richiedente_Dal dalPetitioner = new Richiedente_Dal();
            Parametri_Dal dalParameters = new Parametri_Dal();
            LK_TestoStandard_Dal dalText = new LK_TestoStandard_Dal();
            Mailer mailer = new Mailer();

            List<VisitaProgrammata> v = new List<VisitaProgrammata>();
            Richiedente petitioner = null;
            string recipientsLabel = string.Empty;
            string subjectLabel = string.Empty;
            string testText = string.Empty;

            recipientsLabel = dalText.GetText(prenotation.Id_LinguaRisposta, "FAX_TO").Testo;
            subjectLabel = dalText.GetText(prenotation.Id_LinguaRisposta, "FAX_SUBJ").Testo;

            foreach (V_EvidenzeGiornaliere vp in visits)
                if (vp.Id_Prenotazione == prenotation.Id_Prenotazione)
                    v.Add(dalVisit.GetItem(vp.Id_VisitaProgrammata));

            MailCreationResult result = new MailCreationResult();

            string subject = string.Empty;
            string body = string.Empty;

            /*
            result = mailer.CreatePetitionerVisitNoticeSubject(prenotation, (v != null && v.Count > 0) ? v[0] : null);
            if (result.success)
                subject = result.Text;

            result = mailer.CreatePetitionerVisitNoticeBody(prenotation, v, currentConfirmStatus);
            if (result.success)
                body = result.Text;
            else
            {
                result.ErrorList = "Impossibile inviare l'email a causa dei seguenti errori:" + result.ErrorList;
                MessageBox.Show(result.ErrorList);
                return;
            }
            */

            mailer.GetSubjectAndBody(vM.ObjPrenotation.Id_LinguaRisposta, (int)vM.ObjPrenotation.Id_TipoRisposta, out subject, out body);

            testText += "<b>subject:</b><br />" + subject + "<br /><br /><b>Body</b>:<br />" + body + "<br />----------------------<br />";

            petitioner = dalPetitioner.GetItem(vM.ObjPrenotation.Id_Richiedente);

            if (petitioner != null)
            {

                //{0},<br/> La prenotazione relativa a Vs {1} ingressi per {2} in data {3} è confermata.<br/> Distinti Saluti.<br/>{4}
                string egrGentSpett = "Egr. ";

                if (petitioner.Is_PF)
                {
                    if (petitioner.Sesso == "F")
                        egrGentSpett = "Gent.ma ";
                }
                else
                    egrGentSpett = "Spett.le ";

                string nominativo = "";

                if (!string.IsNullOrEmpty(petitioner.Nome))
                {
                    string nome = string.IsNullOrEmpty(petitioner.Nome) ? "" : petitioner.Nome;
                    nominativo = petitioner.Cognome.ToUpper() + " " + nome.ToUpper();
                }
                else
                    nominativo = petitioner.Cognome.ToUpper();

                string dataVisita = vM.CurrentDate.ToString("dd/MM/yyyy");

                int nrVisit = vM.GetScheduledVisitsVisitorsCount();

                string firma = dalParameters.GetItem("email_signature").Valore;

                body = string.Format(body,
                                      egrGentSpett + nominativo,
                                      nrVisit,
                                      vM.GetVPrenotazione().TipoVisita,
                                      dataVisita,
                                      vM.SrcVisiteProgrammateAll[0].Ora_Visita,
                                      firma);

                string nomex = string.IsNullOrEmpty(petitioner.Nome) ? "" : petitioner.Nome;
                EmailPreviewItem item = new EmailPreviewItem(recipientsLabel, petitioner.Email, petitioner.Cognome + " " + nomex, subjectLabel, subject, ".jpg", body);
                //ONLINEPAYMENT
                item.PrenotationID = vM.ObjPrenotation.Id_Prenotazione;
                item.CanSendOnLinePaymentOrder = vM.CanPayFull;
                BaseDetailPage frm = new wndEmailPreview(item, Convert.ToInt32(vM.ObjPrenotation.Id_TipoRisposta));
                frm.ShowDialog();
                if (frm.DialogResult == true)
                {
                    switch (frm.EmailDestination)
                    {
                        case Scv_Model.EmailDestination.Send:
                            try
                            {
                                bool sendEmail = true;
                                if (enableOnlinePayment)
                                {
                                    if (vM.ObjPrenotation.Id_TipoRisposta == 2)
                                    {
                                        DateTime resDate = new DateTime();
                                        if (vM.CheckIfOnlinePaymentIsAllowed(prenotationID, out resDate))
                                            sendEmail = enableOnlinePaymentEmailSending;
                                    }
                                }

                                if (sendEmail)
                                {
                                    MailMessage message = new MailMessage("petitionerFrom@email.it", petitioner.Email, subject, body);
                                    if (message != null)
                                        DoSendMail(message, visits, vM.ForwardToSenderVisitor, DateSaveTarget.Visitor, vM.ObjPrenotation.Id_Prenotazione);
                                }
                            }
                            catch (Exception e)
                            {
                                string message = "Si è verificato un errore nella creazione del messaggio Email";

                                if (e.TargetSite.ReflectedType.Name == "MailAddressParser")
                                    message += ":\nIl richiedente ha un indirizzo di email non valido.";

                                MessageBox.Show(message);
                            }
                            break;

                        case Scv_Model.EmailDestination.Print:

                            break;

                        case Scv_Model.EmailDestination.Copy:

                            break;
                    }
                }
            }
        }

        private void DoSendMail(MailMessage message, List<V_EvidenzeGiornaliere> ScheduledVisits, bool forwardToSender, DateSaveTarget saveDate, int? prenotationID, string guideName = "")
        {
            Email_Dal dal = new Email_Dal();
            if (dal.InsertOrUpdate(message, true, forwardToSender, saveDate, prenotationID, ScheduledVisits, guideName) > 0)
            {
                //MessageBox.Show("Il messaggio è stato correttamente inserito nella coda di invio");
                //todo: ripristinare messaggio in caso di ripristino salvataggio in pendingemails
            }
            else
                MessageBox.Show("Non è stato possibile inviare il messaggio a causa di un problema tecnico");
        }

        protected override void SetLayout()
        { }

        private void UpdateVisitSummary(int detailID)
        {
            visitSummary.ClearAllItems(false, true, true);
            visitSummary.InitializeList(list);

            //Aggiornamento sommario visite
            List<string> hoursList = new List<string>(); //raggruppamento
            List<V_EvidenzeGiornaliere> evList = new List<V_EvidenzeGiornaliere>(); //raggruppamento
            V_EvidenzeGiornaliere ev = null; //raggruppamento
            foreach (V_EvidenzeGiornaliere eg in vM.SrcEvidenzeGiornaliere.OrderBy(x => x.Ora_Visita).ThenBy(x => x.SiglaLingua))
            {
                if (!hoursList.Contains(eg.Ora_Visita + eg.SiglaLingua))
                {
                    hoursList.Add(eg.Ora_Visita + eg.SiglaLingua);
                    if (ev != null)
                        evList.Add(ev);
                    ev = new V_EvidenzeGiornaliere();
                    ev.Nr_Interi = 0;
                    ev.Nr_Ridotti = 0;
                    ev.Nr_Omaggio = 0;
                    ev.Nr_Scontati = 0;
                    ev.Nr_Cumulativi = 0;
                    ev.Ora_Visita = eg.Ora_Visita;
                    ev.SiglaLingua = eg.SiglaLingua;
                }
                if (ev != null)
                {
                    ev.Nr_Interi += (short)(eg.Nr_Interi != null ? eg.Nr_Interi : 0);
                    ev.Nr_Ridotti += (short)(eg.Nr_Ridotti != null ? eg.Nr_Ridotti : 0);
                    ev.Nr_Omaggio += (short)(eg.Nr_Omaggio != null ? eg.Nr_Omaggio : 0);
                    ev.Nr_Scontati += (short)(eg.Nr_Scontati != null ? eg.Nr_Scontati : 0);
                    ev.Nr_Cumulativi += (short)(eg.Nr_Cumulativi != null ? eg.Nr_Cumulativi : 0);
                }
            }

            if (ev != null)
                evList.Add(ev);

            foreach (V_EvidenzeGiornaliere e in evList)
                visitSummary.SetItem(e.Ora_Visita, e.SiglaLingua, ((e.Nr_Interi != null ? (short)e.Nr_Interi : 0) + (e.Nr_Omaggio != null ? (short)e.Nr_Omaggio : 0) + (e.Nr_Ridotti != null ? (short)e.Nr_Ridotti : 0) + (e.Nr_Scontati != null ? (short)e.Nr_Scontati : 0) + (e.Nr_Cumulativi != null ? (short)e.Nr_Cumulativi : 0)).ToString());

            foreach (VisitSummaryElement el in visitSummary.Elements)
            {
                visitSummary.SetColor(el.Hour, vM.GetSummaryElementColor(el.Hour));
            }


            visitSummary.BindList();
        }
        
        private void DeleteVisit(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            if (btn != null)
            {
                int visitID = 0;
                int.TryParse(btn.CommandParameter.ToString(), out visitID);
                vM.RemoveScheduledVisitRow(visitID);
                vM.CanPayFull = false;
            }
        }

        private void DoPay(PaymentRange paymentRange, int? visitID)
        {
            if (paymentRange == PaymentRange.PerPrenotation)
            {
                if (new VisitaProgrammata_Dal().CheckSinglePaidVisits(vM.ObjPrenotation.Id_Prenotazione))
                {
                    MessageBox.Show("Impossibile pagare la prenotazione: esistono visite pagate singolarmente.\nEliminare tutti i pagamenti delle singole visite, quindi pagare la prenotazione.", "Errore pagamento");
                    return;
                }
            }
            else
            {
                if (new VisitaProgrammata_Dal().CheckPaidPrenotation(vM.ObjPrenotation.Id_Prenotazione))
                {
                    MessageBox.Show("Impossibile pagare la singola visita: è già stato effettuato un pagamento per l'intera prenotazione.\nEliminare il pagamento per l'intera prenotazione, quindi pagare la singola visita.", "Errore pagamento");
                    return;
                }

                //if (vM.SrcVisiteProgrammateAll.FirstOrDefault(x => x.Id_VisitaProgrammata == visitID).Dt_Visita.Date != DateTime.Now.Date)
                //{
                //    MessageBox.Show("Impossibile pagare la singola visita in una data diversa dalla data della visita stessa.", "Errore pagamento");
                //    return;
                //}
            }

            wndPayment frm = new wndPayment(paymentRange, vM.ObjPrenotation.Id_Prenotazione, visitID);
            frm.User = this.User;
            frm.ShowDialog();

            //Si imposta lo stato di conferma della prenotazione
            vM.ObjPrenotation.Fl_Conferma =
                (vM.ObjPrenotation.Id_TipoRisposta == 10)
                ||
                vM.CheckPayment(vM.ObjPrenotation.Id_Prenotazione)
                ;
            new Prenotazione_Dal().ChangePrenotationConfirmation(vM.ObjPrenotation);

            vM.LoadMaster(DetailID);
            vM.LoadAvailableResponseTypes();
            grdVisiteGiornaliere.SelectedItems.Clear();
            vM.CanPaySingle = false;

            if (vM.ObjPrenotation.Id_TipoRisposta == 2)
            {
                btnPublicMail.IsEnabled = true;
            }
            else
            {
                btnPublicMail.IsEnabled = false;
            }

        }

        private void DoRefund(int visitID)
        {
            Pagamento v = new Pagamento_Dal().GetItemByIdVisitaProgrammata(visitID);
            if (v != null)
            {
                BaseDetailPage frm = new wndVisitRefund(v.Id_Pagamento);
                frm.User = User;
                frm.ShowDialog();
                vM.LoadMaster(this.DetailID);
            }
        }

        #endregion// Methods


    }
}
