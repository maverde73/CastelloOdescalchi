using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Scv_Dal;
using Scv_Entities;
using System.Collections.ObjectModel;
using Scv_Model;
using Presentation.Classes;

namespace Presentation
{
    public class EasyTicketingViewModel : INotifyPropertyChanged
    {
        #region Public Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Variables
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

        private ObservableCollection<V_EvidenzeGiornaliere> visits;
        public ObservableCollection<V_EvidenzeGiornaliere> Visits
        {
            get { return this.visits; }
            set
            {
                if (this.visits != value)
                {
                    this.visits = value;
                    OnPropertyChanged(this, "Visits");
                }
            }
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

        private bool loading = false;
        public bool Loading
        {
            get { return loading; }
            set { loading = value; }
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

        public EasyTicketingViewModel()
        {
            Loading = true;
            LoadAvailablePetitioners();
            LoadAvailableHours();
            LoadAvailableLanguages();
            LoadAvailableVisitTypes();
            LoadAvailablePaymentTypes();
        }

        #endregion

        #region Events Handling

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null && (!Loading))
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

            switch (propertyName)
            {
                case "CurrentDate":
                case "SelectedNewVisitType":
                    SrcEvidenzeGiornaliere.Clear();
                    SrcTickets.Clear();
                    AddTicketRow();
                    break;
            }
        }

        #endregion

        #region Methods

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
            if(AvailableVisitTypes.Count > 0)
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
            
            SrcTickets.Add(ticket);
        }

        public void OnSelectScheduledVisit(int idPrenotazione, int idVisitaProg)
        {
            Payment = dalPagamento.GetItemByIdPrenotazione(idPrenotazione);
            if(Payment == null)
                Payment = dalPagamento.GetItemByIdVisitaProgrammata(idVisitaProg);

            Paid = (Payment != null);
            var selectedVisit = Visits.FirstOrDefault(vx => vx.Id_VisitaProgrammata == idVisitaProg);
            SelectedNewVisitType = AvailableVisitTypes.FirstOrDefault(vx => vx.Id_TipoVisita == selectedVisit.Id_TipoVisita);
            SelectedId_Lingua = selectedVisit.Id_Lingua;

            if (selectedVisit.Consegnati != null)
                Canprint = !(Convert.ToInt32(selectedVisit.Consegnati) > 0);

            SrcTickets.Clear();
            Ticket ticket = new Ticket();
            ticket.Pagamento = Payment;
            ticket.Id_TipoVisita = SelectedNewVisitType.Id_TipoVisita;
            ticket.ObjTipoVisita = SelectedNewVisitType;
            ticket.TipoVisita = SelectedNewVisitType.Descrizione;
            ticket.AvailableHours = AvailableHours;
            ticket.Ora_Visita = selectedVisit.Ora_Visita;
            int interi = selectedVisit.Nr_Interi == null ? 0 : (int)selectedVisit.Nr_Interi;
            int ridotti = selectedVisit.Nr_Ridotti == null ? 0 : (int)selectedVisit.Nr_Ridotti;
            int scontati = selectedVisit.Nr_Scontati == null ? 0 : (int)selectedVisit.Nr_Scontati;
            int cumulativi = selectedVisit.Nr_Cumulativi == null ? 0 : (int)selectedVisit.Nr_Cumulativi;
            int omaggio = selectedVisit.Nr_Omaggio == null ? 0 : (int)selectedVisit.Nr_Omaggio;


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

        public void AddNewVisit(int idLingua,
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
            visitaProgrammata.Nr_CumulativiConsegnati = (short)ticket.Cumulativi;
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

        #endregion
    }
}