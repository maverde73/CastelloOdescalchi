using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;
using Scv_Dal;
using Scv_Entities;
using Scv_Model;
using Presentation.Classes;

namespace Presentation
{
    public class ScheduledToursViewModel : ViewModelBase
    {
        #region Variables
        public event PropertyChangedEventHandler PropertyChanged;
        private VisitaProgrammata_Dal dalVisitaProgrammata = new VisitaProgrammata_Dal();
        private EasyTicketing_DAL easyTicketing_DAL = new EasyTicketing_DAL();
        private ObservableCollection<Hour> availableHours = null;
        private ObservableCollection<LK_TipoVisita> availableVisitTypes = null;
        private LK_TipoVisita selectedNewVisitType = null;
        private LK_Lingua_Dal dalLanguage = new LK_Lingua_Dal();
        public Richiedente_Dal dalPetitioner = new Richiedente_Dal();
        private Pagamento_Dal dalPagamento = new Pagamento_Dal();
        LK_Progressivi_Dal dalProgressives = new LK_Progressivi_Dal();
        private Biglietto_Dal dalBiglietto = new Biglietto_Dal();

        #endregion

        #region Properties

        private DateTime currentDate = DateTime.Today;
        public DateTime CurrentDate
        {
            get
            {
                return this.currentDate;
            }
            set
            {
                this.currentDate = value;
                OnPropertyChanged(this, "CurrentDate");
            }
        }


        private ObservableCollection<Appointment> appointments;
        public ObservableCollection<Appointment> Appointments
        {
            get { return this.appointments; }
            set
            {
                if (this.appointments != value)
                {
                    this.appointments = value;
                    this.OnPropertyChanged(() => this.Appointments);
                }
            }
        }


        private ObservableCollection<V_EvidenzeGiornaliere> visits;
        public ObservableCollection<V_EvidenzeGiornaliere> Visits
        {
            get { return this.visits; }
            set
            {
                if (this.visits != value)
                {
                    this.visits = value;
                    this.OnPropertyChanged(() => this.Visits);
                }
            }
        }

        private V_EvidenzeGiornaliere selectedVisit;
        public V_EvidenzeGiornaliere SelectedVisit
        {
            get { return this.selectedVisit; }
            set
            {
                if (this.selectedVisit != value)
                {
                    this.selectedVisit = value;
                    this.OnPropertyChanged(() => this.SelectedVisit);
                }
            }
        }

        private bool loading = false;
        public bool Loading
        {
            get { return loading; }
            set { loading = value; }
        }

        private int idVP;
        public int IdVP
        {
            get { return this.idVP; }
            set
            {
                this.idVP = value;
                OnPropertyChanged(this, "IdVP");
            }
        }


        private bool enableAll = false;
        public bool EnableAll
        {
            get { return enableAll; }
            set { enableAll = value; }
        }



        private Richiedente selectedRichiedente = null;
        public Richiedente SelectedRichiedente
        {
            get
            {
                return selectedRichiedente;
            }
            set
            {
                selectedRichiedente = value;
                OnPropertyChanged(this, "SelectedRichiedente");
            }
        }

        private ObservableCollection<V_EvidenzeGiornaliere> srcEvidenzeGiornaliere = null;
        public ObservableCollection<V_EvidenzeGiornaliere> SrcEvidenzeGiornaliere
        {
            get
            {
                if (srcEvidenzeGiornaliere == null)
                    srcEvidenzeGiornaliere = new ObservableCollection<V_EvidenzeGiornaliere>();
                return srcEvidenzeGiornaliere;
            }
            set
            {
                srcEvidenzeGiornaliere = value;
                OnPropertyChanged(this, "SrcEvidenzeGiornaliere");
            }
        }

        private ObservableCollection<Ticket> srcTickets = null;
        public ObservableCollection<Ticket> SrcTickets
        {
            get
            {
                if (srcTickets == null)
                    srcTickets = new ObservableCollection<Ticket>();
                return srcTickets;
            }
            set
            {
                srcTickets = value;
                OnPropertyChanged(this, "SrcTickets");
            }
        }

        public ObservableCollection<Hour> AvailableHours
        {
            get
            {
                if (availableHours == null)
                    availableHours = new ObservableCollection<Hour>();
                return availableHours;
            }
            set
            {
                availableHours = value;
                OnPropertyChanged(this, "AvailableHours");
            }
        }

        public ObservableCollection<LK_TipoVisita> AvailableVisitTypes
        {
            get
            {
                if (availableVisitTypes == null)
                    availableVisitTypes = new ObservableCollection<LK_TipoVisita>();
                return availableVisitTypes;
            }
            set
            {
                availableVisitTypes = value;
                OnPropertyChanged(this, "AvailableVisitTypes");
            }
        }

        private ObservableCollection<LK_Lingua> availableLanguages = null;
        public ObservableCollection<LK_Lingua> AvailableLanguages
        {
            get
            {
                if (availableLanguages == null)
                    availableLanguages = new ObservableCollection<LK_Lingua>();
                return availableLanguages;
            }
            set { availableLanguages = value; }
        }



        public LK_TipoVisita SelectedNewVisitType
        {
            get
            {
                return selectedNewVisitType;
            }
            set
            {
                selectedNewVisitType = value;
                OnPropertyChanged(this, "SelectedNewVisitType");
            }
        }

        decimal prezzoTotale;
        public decimal PrezzoTotale
        {
            get
            {
                prezzoTotale = 0;
                foreach (Ticket ticket in SrcTickets)
                {
                    prezzoTotale += ticket.Prezzo;
                }
                return prezzoTotale;
            }
            set { prezzoTotale = value; OnPropertyChanged(this, "PrezzoTotale"); }

        }

        private ObservableCollection<Richiedente> availablePetitioners = null;
        public ObservableCollection<Richiedente> AvailablePetitioners
        {
            get
            {
                if (availablePetitioners == null)
                    availablePetitioners = new ObservableCollection<Richiedente>();
                return availablePetitioners;
            }
            set { availablePetitioners = value; }
        }

        private List<LK_TipoPagamento> availablePaymentTypes = null;
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

        int id_TipoPagamento = 0;
        public int Id_TipoPagamento
        {
            get
            {
                return id_TipoPagamento;
            }
            set { id_TipoPagamento = value; OnPropertyChanged(this, "Id_TipoPagamento"); }
        }

        int selectedId_Lingua = 0;
        public int SelectedId_Lingua
        {
            get
            {
                return selectedId_Lingua;
            }
            set { selectedId_Lingua = value; OnPropertyChanged(this, "SelectedId_Lingua"); }
        }

        private int ticketsStartNumber = 0;
        public int TicketsStartNumber
        {
            get { return ticketsStartNumber; }
            set { ticketsStartNumber = value; OnPropertyChanged(this, "TicketsStartNumber"); }
        }

        private string protocollo = "";
        public string Protocollo
        {
            get { return protocollo; }
            set { protocollo = value; OnPropertyChanged(this, "Protocollo"); }
        }

        private int idVisitaProgrammata = 0;
        public int IdVisitaProgrammata
        {
            get { return idVisitaProgrammata; }
            set { idVisitaProgrammata = value; OnPropertyChanged(this, "IdVisitaProgrammata"); }
        }

        bool paid = false;
        public bool Paid
        {
            get
            {
                return paid;
            }
            set { paid = value; OnPropertyChanged(this, "Paid"); }
        }

        private Pagamento payment = null;
        public Pagamento Payment
        {
            get
            {
                return payment;
            }
            set { payment = value; OnPropertyChanged(this, "Payment"); }
        }


        bool canprint = true;
        public bool Canprint
        {
            get
            {
                return canprint;
            }
            set { canprint = value; OnPropertyChanged(this, "Canprint"); }
        }



        #endregion

        #region Constructors
        public ScheduledToursViewModel()
		{
            LoadAppointments();
            Loading = true;
            LoadAvailablePetitioners();
            LoadAvailableHours();
            LoadAvailableLanguages();
            LoadAvailableVisitTypes();
            LoadAvailablePaymentTypes();
		}
        #endregion

        #region Methods
        public void LoadAppointments()
        {
           List<Appointment> _appointments = new List<Appointment>();
           Appointment _appointment = null;
           Visits = dalVisitaProgrammata.GetVisiteProgrammate();
           V_EvidenzeGiornaliere vp = null;
           for (int i = 0; i < Visits.Count; i++)
           {
               vp = Visits[i];
               _appointment = new Appointment();
               _appointment.Subject = string.Format("{0} - {1} - {2} - ({3}) - {4}", vp.TipoVisita, vp.Short, vp.Responsabile, GetTicketTypesInfo(vp), vp.NProtocollo);
               _appointment.Body = vp.Id_Prenotazione.ToString() + "," + vp.Id_VisitaProgrammata;
               _appointment.Start = Convert.ToDateTime(vp.Dt_Visita.ToString("dd/MM/yyyy") + " " + vp.Ora_Visita);
               _appointment.End = _appointment.Start.AddMinutes(15);
               _appointments.Add(_appointment);
           }

          Appointments = new ObservableCollection<Appointment>(_appointments);
        }

        private string GetTicketTypesInfo(V_EvidenzeGiornaliere vp)
        {
            string ticketTypesInfo = "";
            if (vp.Nr_Interi != null)
               ticketTypesInfo = Convert.ToInt16(vp.Nr_Interi) > 0 ? "I:" + Convert.ToString(vp.Nr_Interi) : "";

            if (vp.Nr_Ridotti != null)
                ticketTypesInfo += Convert.ToInt16(vp.Nr_Ridotti) > 0 ? ((ticketTypesInfo != "") ? "-" : "") + "R:" + Convert.ToString(vp.Nr_Ridotti) : "";

            if (vp.Nr_Scontati != null)
                ticketTypesInfo += Convert.ToInt16(vp.Nr_Scontati) > 0 ? ((ticketTypesInfo != "") ? "-" : "") + "S:" + Convert.ToString(vp.Nr_Scontati) : "";

            if (vp.Nr_Cumulativi != null)
                ticketTypesInfo += Convert.ToInt16(vp.Nr_Cumulativi) > 0 ? ((ticketTypesInfo != "") ? "-" : "") + "C:" + Convert.ToString(vp.Nr_Cumulativi) : "";
            
            return ticketTypesInfo;
        }

        public string GetOraIniziale()
        {
            Parametri_Dal dalParameters = new Parametri_Dal();
            return dalParameters.GetItem("ora_iniziale_visite_default").Valore;
        }

        public string GetOraFinale()
        {
            Parametri_Dal dalParameters = new Parametri_Dal();
            return dalParameters.GetItem("ora_ultima_visita").Valore;
        }


        public List<V_VisiteProgrammate> GetV_EvidenzeGiornaliereVisitsSummary()
        {
            return dalVisitaProgrammata.GetVListByDate(CurrentDate.Date);
        }

        private void LoadAvailableHours()
        {
            AvailableHours = new ObservableCollection<Hour>(Helper_Dal.GetHours("09:00", "18:00", 15));
        }

        private void LoadAvailableVisitTypes()
        {
            AvailableVisitTypes = new ObservableCollection<LK_TipoVisita>(easyTicketing_DAL.GetVisitTypes());
            if (AvailableVisitTypes.Count > 0)
                SelectedNewVisitType = AvailableVisitTypes[0];
        }

        public void AddTicketRow()
        {
            Ticket ticket = new Ticket();

            ticket.Id_TipoVisita = SelectedNewVisitType.Id_TipoVisita;
            ticket.ObjTipoVisita = SelectedNewVisitType;
            ticket.TipoVisita = SelectedNewVisitType.Descrizione;
            ticket.AvailableHours = AvailableHours;
            if (SrcTickets.Count == 0)
                ticket.Id_Riga = 1;
            else
                ticket.Id_Riga = SrcTickets.Max(tx => tx.Id_Riga) + 1;

            //Convert.ToDateTime("01/01/2001 " + vmST.GetOraIniziale()).TimeOfDay;
            TimeSpan currentTime = new TimeSpan();
            
            string sTimeNow = DateTime.Now.ToString("dd/MM/yyyy HH:mm").Substring(11, 5);
            TimeSpan timeNow = Convert.ToDateTime("01/01/2001 " + sTimeNow).TimeOfDay;
            for (int i = 0; i < ticket.AvailableHours.Count; i++)
            {
                currentTime = Convert.ToDateTime("01/01/2001 " + ticket.AvailableHours[i].Time).TimeOfDay;
                //currentTime = Convert.ToDateTime("01/01/2001 " + ticket.AvailableHours[i].Time).AddMinutes(15).TimeOfDay;
                if (currentTime >= timeNow)
                {
                    ticket.Ora_Visita = ticket.AvailableHours[i].Time;
                    break;
                }
            }


            SrcTickets.Add(ticket);
        }

        public void OnSelectScheduledVisit(int idPrenotazione, int idVisitaProg)
        {
            Payment = dalPagamento.GetItemByIdPrenotazione(idPrenotazione);
            if (Payment == null)
                Payment = dalPagamento.GetItemByIdVisitaProgrammata(idVisitaProg);

            if (Payment != null)
                Id_TipoPagamento = (int)Payment.Id_TipoPagamento;

            Paid = (Payment != null);
            SelectedVisit = Visits.FirstOrDefault(vx => vx.Id_VisitaProgrammata == idVisitaProg);

            IdVisitaProgrammata = SelectedVisit.Id_VisitaProgrammata;
            Protocollo = SelectedVisit.NProtocollo.ToString();

            SelectedNewVisitType = AvailableVisitTypes.FirstOrDefault(vx => vx.Id_TipoVisita == SelectedVisit.Id_TipoVisita);
            SelectedId_Lingua = SelectedVisit.Id_Lingua;

            if (SelectedVisit.Consegnati != null)
                Canprint = !(Convert.ToInt32(SelectedVisit.Consegnati) > 0);

            SrcTickets.Clear();
            Ticket ticket = new Ticket();
            ticket.Pagamento = Payment;
            ticket.Id_TipoVisita = SelectedNewVisitType.Id_TipoVisita;
            ticket.ObjTipoVisita = SelectedNewVisitType;
            ticket.TipoVisita = SelectedNewVisitType.Descrizione;
            ticket.AvailableHours = AvailableHours;
            ticket.Ora_Visita = SelectedVisit.Ora_Visita;
            int interi = SelectedVisit.Nr_Interi == null ? 0 : (int)SelectedVisit.Nr_Interi;
            int ridotti = SelectedVisit.Nr_Ridotti == null ? 0 : (int)SelectedVisit.Nr_Ridotti;
            int scontati = SelectedVisit.Nr_Scontati == null ? 0 : (int)SelectedVisit.Nr_Scontati;
            int cumulativi = SelectedVisit.Nr_Cumulativi == null ? 0 : (int)SelectedVisit.Nr_Cumulativi;
            int omaggio = SelectedVisit.Nr_Omaggio == null ? 0 : (int)SelectedVisit.Nr_Omaggio;


            ticket.Interi = interi;
            ticket.Ridotti = ridotti;
            ticket.Scontati = scontati;
            ticket.Cumulativi = cumulativi;
            ticket.Omaggio = omaggio;

            if (SrcTickets.Count == 0)
                ticket.Id_Riga = 1;
            else
                ticket.Id_Riga = SrcTickets.Max(tx => tx.Id_Riga) + 1;

            SrcTickets.Add(ticket);
        }

        public void ClearSelection()
        {
            Payment = null;
            Paid = false;
            SelectedVisit = null;
            IdVisitaProgrammata = 0;
            Id_TipoPagamento = 0;
            Protocollo = "";
            SelectedId_Lingua = 0;
            Canprint = true;
        }
        public void RemoveTicketRow(int id_riga)
        {
            SrcTickets.Remove(SrcTickets.FirstOrDefault(tx => tx.Id_Riga == id_riga));
        }

        public void ResetAll()
        {
            if (AvailableVisitTypes.Count > 0)
                SelectedNewVisitType = AvailableVisitTypes[0];

            SrcTickets.Clear();
        }

        private void LoadAvailableLanguages()
        {
            AvailableLanguages = new ObservableCollection<LK_Lingua>(dalLanguage.GetList());
            SelectedId_Lingua = AvailableLanguages[0].Id_Lingua;
        }

        private void LoadAvailablePetitioners()
        {
            BaseFilter filter = new BaseFilter();
            filter.AddSortField("Cognome");
            filter.AddSortField("Nome");
            filter.SortDirection = SortDirection.ASC;
            int count = 0;
            AvailablePetitioners = dalPetitioner.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count);
        }

        private void LoadAvailablePaymentTypes()
        {
            List<LK_TipoPagamento> list = new List<LK_TipoPagamento>();
            List<LK_TipoPagamento> tmpList = new List<LK_TipoPagamento>();
            List<int> paymentTypeIDs = new List<int>();
            paymentTypeIDs.AddRange(new List<int>() { 3, 4, 5 });

            tmpList.AddRange(dalPagamento.GetList());

            foreach (LK_TipoPagamento obj in tmpList)
                if (paymentTypeIDs.Contains(obj.Id_TipoPagamento))
                    list.Add(obj);

            AvailablePaymentTypes = list;
        }

        public int AddNewVisit(int idLingua,
                                string cognome,
                                string nome,
                                string email,
                                int idUtente,
                                int? idRichiedente)
        {
            VisitaProgrammata visitaProgrammata = new VisitaProgrammata();
            Pagamento pagamento = new Pagamento();
            Ticket ticket = SrcTickets[0];
            visitaProgrammata.Ora_Visita = ticket.Ora_Visita;
            visitaProgrammata.Dt_Visita = CurrentDate;
            visitaProgrammata.Nr_Interi = (short)ticket.Interi;
            visitaProgrammata.Nr_Ridotti = (short)ticket.Ridotti;
            visitaProgrammata.Nr_Scontati = (short)ticket.Scontati;
            visitaProgrammata.Nr_Cumulativi = (short)ticket.Cumulativi;
            visitaProgrammata.Nr_Omaggio = (short)ticket.Omaggio;
            visitaProgrammata.Nr_InteriConsegnati = (short)ticket.Interi;
            visitaProgrammata.Nr_RidottiConsegnati = (short)ticket.Ridotti;
            visitaProgrammata.Nr_ScontatiConsegnati = (short)ticket.Scontati;
            //visitaProgrammata.Nr_CumulativiConsegnati = (short)ticket.Cumulativi;
            if (((short)ticket.Cumulativi) > 0)
            {
                visitaProgrammata.Nr_CumulativiConsegnati = 1;
            }
            visitaProgrammata.Nr_OmaggioConsegnati = (short)ticket.Omaggio;
            pagamento.Id_TipoPagamento = Id_TipoPagamento;
            pagamento.Nr_InteriPagati = ticket.Interi;
            pagamento.Nr_RidottiPagati = ticket.Ridotti;
            pagamento.Nr_ScontatiPagati = ticket.Scontati;
            pagamento.Nr_CumulativiPagati = ticket.Cumulativi;
            pagamento.Importo_Interi = ticket.PrezzoInteri;
            pagamento.Importo_Ridotti = ticket.PrezzoRidotti;
            pagamento.Importo_Scontati = ticket.PrezzoScontati;
            pagamento.Importo_Cumulativi = ticket.PrezzoCumulativi;

            pagamento.PrezzoIntero = SelectedNewVisitType.PrezzoIntero;
            pagamento.PrezzoRidotto = SelectedNewVisitType.PrezzoRidotto;
            pagamento.PrezzoCumulativo = SelectedNewVisitType.PrezzoCumulativo;
            pagamento.PrezzoScontato = SelectedNewVisitType.PrezzoScontato;

            easyTicketing_DAL.AddNewVisit(CurrentDate,
                                          SelectedNewVisitType.Id_TipoVisita,
                                          visitaProgrammata,
                                          pagamento,
                                          idLingua,
                                          cognome,
                                          nome,
                                          email,
                                          idUtente,
                                          idRichiedente,
                                          out protocollo,
                                          out idVisitaProgrammata);

            return idVisitaProgrammata;

        }


        public int UpdateVisit(int idLingua,
                        string cognome,
                        string nome,
                        string email,
                        int idUtente)
                       
        {
            Ticket ticket = SrcTickets[0];

            VisitaProgrammata visitaProgrammata = dalVisitaProgrammata.GetItem(SelectedVisit.Id_VisitaProgrammata);
            visitaProgrammata.Ora_Visita = ticket.Ora_Visita;
            visitaProgrammata.Dt_Visita = CurrentDate;
            visitaProgrammata.Nr_Interi = (short)ticket.Interi;
            visitaProgrammata.Nr_Ridotti = (short)ticket.Ridotti;
            visitaProgrammata.Nr_Scontati = (short)ticket.Scontati;
            visitaProgrammata.Nr_Cumulativi = (short)ticket.Cumulativi;
            visitaProgrammata.Nr_Omaggio = (short)ticket.Omaggio;
            visitaProgrammata.Nr_InteriConsegnati = (short)ticket.Interi;
            visitaProgrammata.Nr_RidottiConsegnati = (short)ticket.Ridotti;
            visitaProgrammata.Nr_ScontatiConsegnati = (short)ticket.Scontati;
            //visitaProgrammata.Nr_CumulativiConsegnati = (short)ticket.Cumulativi;
            if (((short)ticket.Cumulativi) > 0)
            {
                visitaProgrammata.Nr_CumulativiConsegnati = 1;
            }
            visitaProgrammata.Nr_OmaggioConsegnati = (short)ticket.Omaggio;
            visitaProgrammata.Id_User = idUtente;
            visitaProgrammata.Dt_Update = DateTime.Now;

            Pagamento pag = null;

            if (!Paid)
            {
                pag = new Pagamento();
                pag.Id_TipoPagamento = Id_TipoPagamento;
                pag.Nr_InteriPagati = ticket.Interi;
                pag.Nr_RidottiPagati = ticket.Ridotti;
                pag.Nr_ScontatiPagati = ticket.Scontati;
                pag.Nr_CumulativiPagati = ticket.Cumulativi;
                pag.Importo_Interi = ticket.PrezzoInteri;
                pag.Importo_Ridotti = ticket.PrezzoRidotti;
                pag.Importo_Scontati = ticket.PrezzoScontati;
                pag.Importo_Cumulativi = ticket.PrezzoCumulativi;
                pag.PrezzoIntero = SelectedNewVisitType.PrezzoIntero;
                pag.PrezzoRidotto = SelectedNewVisitType.PrezzoRidotto;
                pag.PrezzoCumulativo = SelectedNewVisitType.PrezzoCumulativo;
                pag.PrezzoScontato = SelectedNewVisitType.PrezzoScontato;


                if (dalVisitaProgrammata.GetVListByIdPrenotazione(SelectedVisit.Id_Prenotazione).Count > 1)
                {
                    pag.Id_VisitaProgrammata = SelectedVisit.Id_VisitaProgrammata;
                    pag.FL_PagamentoParziale = true;
                }
            }

            easyTicketing_DAL.UpdateVisit(SelectedVisit.Id_Prenotazione,
                                          CurrentDate,
                                          SelectedNewVisitType.Id_TipoVisita,
                                          visitaProgrammata,
                                          pag,
                                          idLingua,
                                          cognome,
                                          nome,
                                          email,
                                          idUtente);

            return SelectedVisit.Id_VisitaProgrammata;
        }

        public void GetTicketLastSerial()
        {
            LK_Progressivi pg = dalProgressives.GetProgressiviBySymbol("TK");
            if (pg != null)
            {
                TicketsStartNumber = pg.Progr_UltimoUscito;
            }
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

        public void UpdatePrinted(int lastProgTicket)
        {
            easyTicketing_DAL.UpdatePrinted(IdVisitaProgrammata, lastProgTicket);
        }

        public bool DeletePrenotation(int idPrenotazione,int idVProgr)
        {
            bool retVal = false;
            if (retVal = easyTicketing_DAL.DeletePrenotation(idPrenotazione))
            {
                if (idVProgr == IdVisitaProgrammata)
                {
                    ClearSelection();
                    SrcTickets.Clear();
                    AddTicketRow();
                    EnableAll = true;
                }
                else
                    EnableAll = false;
            }
            return retVal;
        }

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null && (!Loading))
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

            switch (propertyName)
            {
                case "CurrentDate":
                case "SelectedNewVisitType":
                    if (SelectedVisit == null)
                    {
                        SrcEvidenzeGiornaliere.Clear();
                        SrcTickets.Clear();
                        AddTicketRow();
                    }
                    break;
            }
        }
        #endregion
    }
}
