using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Scv_Dal;
using Scv_Entities;
using System.Globalization;
using System.Threading;
using Scv_Model;
using System.Windows;
using System.Windows.Media;

namespace Presentation
{
	public class ScheduleToursViewModel : INotifyPropertyChanged
	{
		#region DAL

		private Prenotazione_Dal dalPrenotation = new Prenotazione_Dal();
		private Richiedente_Dal dalPetitioner = new Richiedente_Dal();
		private VisitaPrenotata_Dal dalPrenotationVisit = new VisitaPrenotata_Dal();
		private VisitaProgrammata_Dal dalVisitaProgrammata = new VisitaProgrammata_Dal();
		private LK_Lingua_Dal dalLanguage = new LK_Lingua_Dal();
		private Pagamento_Dal dalPagamento = new Pagamento_Dal();
		private LK_TipoVisita_Dal dalTipoVisita = new LK_TipoVisita_Dal();
		private LK_TipoPagamento_Dal dalTipoPagamento = new LK_TipoPagamento_Dal();
		private Guida_Dal dalGuida = new Guida_Dal();
		private GuidaDisponibile_Dal dalGuideDisponibili = new GuidaDisponibile_Dal();
		private LK_TipoRisposta_Dal dalTipoRisposta = new LK_TipoRisposta_Dal();
		private Parametri_Dal dalParametri = new Parametri_Dal();
		private LK_Chiusura_Dal dalChiusura = new LK_Chiusura_Dal();
        VisitaProgrammata_Dal dalVisits = new VisitaProgrammata_Dal();
        Parametri_Dal dalParameters = new Parametri_Dal();

		#endregion// DAL

		DateTime startCtor = DateTime.Now;
		DateTime start = DateTime.Now;
		int i1 = 0;
		int i2 = 0;
		int i3 = 0;
		int i0 = 0;


		#region Private Fields

		private int lastSuitableResponseTypeID = 0;
		private bool forwardToSenderGuide = false;
		private bool forwardToSenderVisitor = false;
		private bool blockAllVisits = false;
		private bool loading = false;
        string ora_inizio_visite_pm = "";

		#endregion// Private Fields




		#region Properties

		public bool Loading
		{
			get { return loading; }
			set { loading = value; }
		}

		public int maxVisitorsPerGroup = 0;
		public int maxVisitorsPerDay = 0;

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

		private string dateMessage = string.Empty;
		public string DateMessage
		{
			get { return dateMessage; }
			set { dateMessage = value; OnPropertyChanged(this, "DateMessage"); }
		}

		private Visibility dateMessageVisibility = Visibility.Collapsed;
		public Visibility DateMessageVisibility
		{
			get { return dateMessageVisibility; }
			set { dateMessageVisibility = value; OnPropertyChanged(this, "DateMessageVisibility"); }
		}

		private DateTime? scheduleDate = (DateTime?)null;
		public DateTime? ScheduleDate
		{
			get { return scheduleDate; }
			set { scheduleDate = value; OnPropertyChanged(this, "ScheduleDate"); }
		}

		private ObservableCollection<V_VisitePrenotate> srcPrenotationVisits = null;
		public ObservableCollection<V_VisitePrenotate> SrcPrenotationVisits
		{
			get
			{
				if (srcPrenotationVisits == null)
					srcPrenotationVisits = new ObservableCollection<V_VisitePrenotate>();
				return srcPrenotationVisits;
			}
			set { srcPrenotationVisits = value; OnPropertyChanged(this, "SrcPrenotationVisits"); }
		}

		private ObservableCollection<V_VisiteProgrammate> srcVisiteProgrammate = null;
		public ObservableCollection<V_VisiteProgrammate> SrcVisiteProgrammate
		{
			get
			{
				//if (srcVisiteProgrammate == null)
				//    srcVisiteProgrammate = new ObservableCollection<V_VisiteProgrammate>();
				return srcVisiteProgrammate;
			}
			set
			{
				srcVisiteProgrammate = value;
				OnPropertyChanged(this, "SrcVisiteProgrammate");
			}
		}

		private ObservableCollection<V_VisiteProgrammate> srcVisiteProgrammateAll = null;
		public ObservableCollection<V_VisiteProgrammate> SrcVisiteProgrammateAll
		{
			get
			{
				if (srcVisiteProgrammateAll == null)
					srcVisiteProgrammateAll = new ObservableCollection<V_VisiteProgrammate>();
				return srcVisiteProgrammateAll;
			}
			set
			{
				srcVisiteProgrammateAll = value;
				OnPropertyChanged(this, "SrcVisiteProgrammateAll");
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

		private V_EvidenzeGiornaliere selectedEvidenzeGiornaliere = null;
		public V_EvidenzeGiornaliere SelectedEvidenzeGiornaliere
		{
			get { return selectedEvidenzeGiornaliere; }
			set { selectedEvidenzeGiornaliere = value; OnPropertyChanged(this, "SelectedEvidenzeGiornaliere"); }
		}

		private ObservableCollection<V_EvidenzeGiornaliere> srcOldEvidenzeGiornaliere = null;
		public ObservableCollection<V_EvidenzeGiornaliere> SrcOldEvidenzeGiornaliere
		{
			get
			{
				if (srcOldEvidenzeGiornaliere == null)
					srcOldEvidenzeGiornaliere = new ObservableCollection<V_EvidenzeGiornaliere>();
				return srcOldEvidenzeGiornaliere;
			}
			set
			{
				srcOldEvidenzeGiornaliere = value;
				OnPropertyChanged(this, "SrcOldEvidenzeGiornaliere");
			}
		}

		private ObservableCollection<Hour> availablesHours = null;
		public ObservableCollection<Hour> AvailablesHours
		{
			get
			{
				if (availablesHours == null)
					availablesHours = new ObservableCollection<Hour>();
				return availablesHours;
			}
			set
			{

				availablesHours = value;
                OnPropertyChanged(this, "AvailablesHours");
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

		private ObservableCollection<LK_TipoVisita> availableVisitTypes = null;
		public ObservableCollection<LK_TipoVisita> AvailableVisitTypes
		{
			get
			{
				if (availableVisitTypes == null)
					availableVisitTypes = new ObservableCollection<LK_TipoVisita>();
				return availableVisitTypes;
			}
			set { availableVisitTypes = value; }
		}

		private ObservableCollection<LK_TipoRisposta> availableResponseTypes = null;
		public ObservableCollection<LK_TipoRisposta> AvailableResponseTypes
		{
			get
			{
				if (availableResponseTypes == null)
					availableResponseTypes = new ObservableCollection<LK_TipoRisposta>();
				return availableResponseTypes;
			}
			set { availableResponseTypes = value; OnPropertyChanged(this, "AvailableResponseTypes"); }
		}

		private ObservableCollection<V_Guide> availableGuides = null;
		public ObservableCollection<V_Guide> AvailableGuides
		{
			get
			{
				if (availableGuides == null)
					availableGuides = new ObservableCollection<V_Guide>();
				return availableGuides;
			}
			set { availableGuides = value; }
		}

		private Prenotazione objPrenotation = null;
		public Prenotazione ObjPrenotation
		{
			get
			{
				if (objPrenotation == null)
					objPrenotation = new Prenotazione();
				return objPrenotation;
			}
			set { objPrenotation = value; OnPropertyChanged(this, "ObjPrenotation"); }
		}

		private V_VisitePrenotate objSelectedPrenotationVisit = null;
		public V_VisitePrenotate ObjSelectedPrenotationVisit
		{
			get { return objSelectedPrenotationVisit; }
			set
			{
				objSelectedPrenotationVisit = value;
				OnPropertyChanged(this, "ObjSelectedPrenotationVisit");

				if (value != null && !value.IsEmpty /*&& value.IsErasable*/)
				{
					LoadScheduledVisits(value);
					IsPrenotationVisitSelected = true;
					ScheduledVisitsLabel = "Programmazione visita in " + ObjSelectedPrenotationVisit.LinguaVisita;
				}
				else
					IsPrenotationVisitSelected = false;
			}
		}

		private ClosingDateResult objClosingDateResult = null;
		public ClosingDateResult ObjClosingDateResult
		{
			get
			{
				if (objClosingDateResult == null)
					objClosingDateResult = new ClosingDateResult();
				return objClosingDateResult;
			}
			set { objClosingDateResult = value; OnPropertyChanged(this, "ObjClosingDateResult"); }
		}

		private string scheduledVisitsSummaryLabel = string.Empty;
		public string ScheduledVisitsSummaryLabel
		{
			get { return scheduledVisitsSummaryLabel; }
			set { scheduledVisitsSummaryLabel = value; OnPropertyChanged(this, "ScheduledVisitsSummaryLabel"); }
		}

		private bool isPrenotationVisitSelected = false;
		public bool IsPrenotationVisitSelected
		{
			get { return isPrenotationVisitSelected; }
			set { isPrenotationVisitSelected = value; OnPropertyChanged(this, "IsPrenotationVisitSelected"); }
		}
		
		private bool canChangeTotalVisitors = true;
		public bool CanChangeTotalVisitors
		{
			get { return canChangeTotalVisitors; }
			set { canChangeTotalVisitors = value; OnPropertyChanged(this, "CanChangeTotalVisitors"); }
		}

		private bool canScheduleVisit = false;
		public bool CanScheduleVisit
		{
			get { return canScheduleVisit; }
			set { canScheduleVisit = value; OnPropertyChanged(this, "CanScheduleVisit"); }
		}

		private bool canDeletePrenotationVisit = false;
		public bool CanDeletePrenotationVisit
		{
			get { return canDeletePrenotationVisit; }
			set { canDeletePrenotationVisit = value; OnPropertyChanged(this, "CanDeletePrenotationVisit"); }
		}

		private bool canSendPetitionerEmail = false;
		public bool CanSendPetitionerEmail
		{
			get { return canSendPetitionerEmail; }
			set { canSendPetitionerEmail = value; OnPropertyChanged(this, "CanSendPetitionerEmail"); }
		}

		private bool canSave = false;
		public bool CanSave
		{
			get { return canSave; }
			set { canSave = value; OnPropertyChanged(this, "CanSave"); }
		}

		private bool canPayFull = false;
		public bool CanPayFull
		{
			get { return canPayFull; }
			set { canPayFull = value; OnPropertyChanged(this, "CanPayFull"); }
		}

		private bool canPaySingle = false;
		public bool CanPaySingle
		{
			get { return canPaySingle; }
			set { canPaySingle = value; OnPropertyChanged(this, "CanPaySingle"); }
		}

		private ConflictResult conflictResult = null;
		public ConflictResult ConflictResult
		{
			get
			{
				if (conflictResult == null)
					conflictResult = new ConflictResult();
				return conflictResult;
			}
			set { conflictResult = value; OnPropertyChanged(this, "ConflictResult"); }
		}

		private bool isLoked = false;
		public bool IsLoked
		{
			get { return isLoked; }
			set { isLoked = value; OnPropertyChanged(this, "IsLoked"); }
		}

		private string scheduledVisitsLabel = string.Empty;
		public string ScheduledVisitsLabel
		{
			get { return scheduledVisitsLabel; }
			set { scheduledVisitsLabel = value; OnPropertyChanged(this, "ScheduledVisitsLabel"); }
		}

		private string petitioner = "";
		public string Petitioner
		{
			get
			{

				return petitioner;
			}
			set
			{
				petitioner = value;
				OnPropertyChanged(this, "Petitioner");
			}
		}

        public string ora_inizio_visite_am = "";
        public string ora_ultima_visita = "";

		public int LastSuitableResponseTypeID
		{
			get { return lastSuitableResponseTypeID; }
			set { lastSuitableResponseTypeID = value; OnPropertyChanged(this, "LastSuitableResponseTypeID"); }
		}

		public bool ForwardToSenderGuide
		{
			get { return forwardToSenderGuide; }
		}

		public bool ForwardToSenderVisitor
		{
			get { return forwardToSenderVisitor; }
		}

		#endregion




		#region Constructors

		public ScheduleToursViewModel(int detailID)
		{
			Loading = true;
			//Console.WriteLine("iniziato costruttore viewmodel");

			startCtor = DateTime.Now;

			if (detailID > 0)
			{
                ora_inizio_visite_am = dalParametri.GetItem("ora_inizio_visite_am").Valore;
                ora_ultima_visita = dalParametri.GetItem("ora_ultima_visita").Valore;

				bool.TryParse(dalParametri.GetItem("forwardToSenderGuide").Valore, out forwardToSenderGuide);
				bool.TryParse(dalParametri.GetItem("forwardToSenderVisitor").Valore, out forwardToSenderVisitor);

				maxVisitorsPerGroup = Convert.ToInt32(dalParametri.GetItem("visitatori_gruppo").Valore);
				maxVisitorsPerDay = Convert.ToInt32(dalParametri.GetItem("visitatori_giorno").Valore);

				LoadAvailableLanguages();
				LoadAvailableHours();
				LoadAvailableVisitTypes();

				LoadMaster(detailID);

				LoadAvailableResponseTypes();

				//Se vi sono biglietti emessi per qualsiasi delle visite la prenotazione è bloccata
				if (dalVisitaProgrammata.GetBigliettiEmessiByIdPrenotazione(ObjPrenotation.Id_Prenotazione) > 0)
					IsLoked = true;

				LastSuitableResponseTypeID = (int)ObjPrenotation.Id_TipoRisposta;


				if (srcPrenotationVisits.Count > 0)
					ObjSelectedPrenotationVisit = srcPrenotationVisits[0];

				ObjPrenotation.NestedPropertyChanged += new PropertyChangedEventHandler(OnPrenotationNestedPropertyChanged);

				if (
					SrcVisiteProgrammateAll.Count > 0 
					&&
                    //SrcVisiteProgrammateAll[0].Dt_Visita >= DateTime.Now.Date
                    //&& 
					SrcVisiteProgrammateAll.Count == SrcEvidenzeGiornaliere.Where(x => x.VP_IdPrenotazione == ObjPrenotation.Id_Prenotazione).Count())
					CanPayFull = true;

				CanSave = !IsLoked;
			}

            ora_inizio_visite_pm = dalParametri.GetItem("ora_inizio_visite_pm").Valore;
		}

		#endregion




		#region Lookup And DB Methods

		public void LoadMaster(int detailID)
		{
			BaseFilter filter = new BaseFilter();
			filter.SortDirection = SortDirection.ASC;
			filter.AddFilter("Id_Prenotazione", Scv_Model.Common.Utilities.ValueType.Int, detailID);
			int count = 0;

			ObjPrenotation = dalPrenotation.GetItem(detailID);

            string nome = string.IsNullOrEmpty(dalPetitioner.GetItem(ObjPrenotation.Id_Richiedente).Nome) ? "" : dalPetitioner.GetItem(ObjPrenotation.Id_Richiedente).Nome;

            Petitioner = nome + " " + dalPetitioner.GetItem(ObjPrenotation.Id_Richiedente).Cognome;

			SrcPrenotationVisits = new ObservableCollection<V_VisitePrenotate>(dalPrenotationVisit.GetVFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count).ToList());
			SrcVisiteProgrammateAll = dalVisitaProgrammata.GetVListByIdPrenotazione(detailID);

			//Se ci sono visite programmate con questo protocollo Currentdate viene
			//impostata con la data della prima delle suddette visite, in modo da
			//predisporre la maschera sempre sulla data di eventuali visite già
			//programmate. Se invece non ci sono visite già programmate per questo
			//protocollo, CurrentDate si lascia sulla prima delle due date indicate
			//come "range" nella prenotazione.
			if (SrcVisiteProgrammateAll != null && SrcVisiteProgrammateAll.Count > 0)
			{
				ScheduleDate = SrcVisiteProgrammateAll[0].Dt_Visita;
				CurrentDate = ScheduleDate.Value.Date;
			}
			else
			{
				DateTime? progDate = dalVisitaProgrammata.GetDateByIdPrenotazione(ObjPrenotation.Id_Prenotazione);
				CurrentDate = progDate != null ? progDate.Value.Date : ObjPrenotation.Dt_VisiteDA;
			}
			
			//Si caricano le evidenze giornaliere che popolano la griglia, il sommario visite
			//e l'etichetta con il numero di visitatori
			LoadEvidenzeGiornaliere();

			SrcOldEvidenzeGiornaliere = SrcEvidenzeGiornaliere;

			//Se esiste una visita pagata singolarmente si bloccano tutte le visite pagate singolarmente
			//Se invece esiste un pagamento totale si bloccano TUTTE le visite
			Pagamento p = dalPagamento.GetItemByIdPrenotazione(ObjPrenotation.Id_Prenotazione);
            //ONLINEPAYMENT
            //Nel caso sia stato già effettuato un pagamento totale 
            //le modifiche alle visite sono consentite solo nel caso
            //in cui non siano già stati emessi biglietti


            if ((p != null && p.Dt_Pagamento != null) && !p.FL_PagamentoParziale)
            {
                blockAllVisits = true;
                SrcPrenotationVisits.ToList().ForEach(x => x.IsErasable = false);
                SrcPrenotationVisits.ToList().ForEach(x => x.IsLanguageEditable = false);
                SrcPrenotationVisits.ToList().ForEach(x => x.IsReadonlyLanguage = true);
                SrcPrenotationVisits.ToList().ForEach(x => x.IsNumericEditable = false);
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.IsErasable = false);
                //SrcVisiteProgrammateAll.ToList().ForEach(x => x.IsNumericEditable = false);
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.Interi = false);
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.Ridotti = false);
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.Scontati = false);
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.Cumulativo = false);
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.Omaggio = false);
                CanScheduleVisit = false;
                CanChangeTotalVisitors = false;
                
                if (dalVisitaProgrammata.GetBigliettiEmessiByIdPrenotazione(ObjPrenotation.Id_Prenotazione) == 0)
                {
                    SrcPrenotationVisits.ToList().ForEach(x => x.IsLanguageEditable = true);
                    SrcPrenotationVisits.ToList().ForEach(x => x.IsReadonlyLanguage = false);
                }
            }
            else
            {
                SrcPrenotationVisits.ToList().ForEach(x => x.IsErasable = !dalVisitaProgrammata.CheckVisitaPrenotataPagamentoParziale(x.Id_VisitaPrenotata));
                SrcPrenotationVisits.ToList().ForEach(x => x.IsLanguageEditable = !dalVisitaProgrammata.CheckVisitaPrenotataPagamentoParziale(x.Id_VisitaPrenotata, true));
                //SrcPrenotationVisits.ToList().ForEach(x => x.IsLanguageEditable = !dalVisitaProgrammata.CheckVisitaPrenotataPagamentoParziale(x.Id_VisitaPrenotata, true) && (dalVisitaProgrammata.GetVListByIdVisitaPrenotata(x.Id_VisitaPrenotata) == null || dalVisitaProgrammata.GetVListByIdVisitaPrenotata(x.Id_VisitaPrenotata).Count == 0));
                SrcPrenotationVisits.ToList().ForEach(x => x.IsReadonlyLanguage = !x.IsLanguageEditable);
                SrcPrenotationVisits.ToList().ForEach(x => x.IsNumericEditable = !dalVisitaProgrammata.CheckVisitaPrenotataPagamentoParziale(x.Id_VisitaPrenotata));
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.IsErasable = !dalVisitaProgrammata.CheckVisitaProgrammataPagamentoParziale(x.Id_VisitaProgrammata));
                //SrcVisiteProgrammateAll.ToList().ForEach(x => x.IsNumericEditable = !dalVisitaProgrammata.CheckVisitaProgrammataPagamentoParziale(x.Id_VisitaProgrammata));
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.Interi = (x.Interi && !dalVisitaProgrammata.CheckVisitaProgrammataPagamentoParziale(x.Id_VisitaProgrammata)));
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.Ridotti = (x.Ridotti && !dalVisitaProgrammata.CheckVisitaProgrammataPagamentoParziale(x.Id_VisitaProgrammata)));
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.Scontati = (x.Scontati && !dalVisitaProgrammata.CheckVisitaProgrammataPagamentoParziale(x.Id_VisitaProgrammata)));
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.Cumulativo = (x.Cumulativo && !dalVisitaProgrammata.CheckVisitaProgrammataPagamentoParziale(x.Id_VisitaProgrammata)));
                SrcVisiteProgrammateAll.ToList().ForEach(x => x.Omaggio = (x.Omaggio && !dalVisitaProgrammata.CheckVisitaProgrammataPagamentoParziale(x.Id_VisitaProgrammata)));
                CanScheduleVisit = IsPrenotationVisitSelected;
                CanChangeTotalVisitors = true;
            }

		}

		private void LoadAvailableLanguages()
		{
			AvailableLanguages = new ObservableCollection<LK_Lingua>(dalLanguage.GetList());
		}

		public ObservableCollection<V_GuideDisponibili> LoadAvailableGuides(V_EvidenzeGiornaliere currentItem)
		{
			return new ObservableCollection<V_GuideDisponibili>(dalGuideDisponibili.GetV_GuideDisponibili(currentItem));
		}

		private void LoadAvailableHours()
		{
			AvailablesHours = new ObservableCollection<Hour>(Helper_Dal.GetHours(ora_inizio_visite_am, ora_ultima_visita, 15));
		}

		private void LoadAvailableVisitTypes()
		{
			AvailableVisitTypes = new ObservableCollection<LK_TipoVisita>(dalTipoVisita.GetItems());
		}

		public void LoadAvailableResponseTypes()
		{			
			List<int> allowedTypes = new List<int>();
			bool paid = CheckPayment(ObjPrenotation.Id_Prenotazione);

			foreach (LK_TipoRisposta r in dalTipoRisposta.GetItems())
			{
				if (paid)
				{
					if (r.Id_TipoRisposta == 3)
						allowedTypes.Add(r.Id_TipoRisposta);
				}
				else
				{
					if (r.Id_TipoRisposta != 3)
						allowedTypes.Add(r.Id_TipoRisposta);
				}
			}

            //al caricamento di AvailableResponseTypes la combo dei tipi di risposta
            //perde la selezione
            int idTipoRispostaBefore = (int)ObjPrenotation.Id_TipoRisposta;
			AvailableResponseTypes = new ObservableCollection<LK_TipoRisposta>(dalTipoRisposta.GetItems()
                                                                               .Where(x => allowedTypes.Contains(x.Id_TipoRisposta)));

            if (!AvailableResponseTypes.Select(x => x.Id_TipoRisposta).Contains(idTipoRispostaBefore))
            {
                if (SrcVisiteProgrammateAll.Count > 0)
                    LastSuitableResponseTypeID = 2;
                else
                    LastSuitableResponseTypeID = 9;
            }
            else
            {
                LastSuitableResponseTypeID = idTipoRispostaBefore;
            }

			if (paid)
				LastSuitableResponseTypeID = 3;
            //else
            //{
            //    if (ObjPrenotation.Id_TipoRisposta == null || !AvailableResponseTypes.Select(x => x.Id_TipoRisposta).Contains((int)ObjPrenotation.Id_TipoRisposta))
            //    {
            //        if (SrcVisiteProgrammateAll.Count > 0)
            //            LastSuitableResponseTypeID = 2;
            //        else
            //            LastSuitableResponseTypeID = 9;
            //    }
            //}

			if(ObjPrenotation.Id_TipoRisposta == null || (!allowedTypes.Contains((int)ObjPrenotation.Id_TipoRisposta)))
				ObjPrenotation.Id_TipoRisposta = LastSuitableResponseTypeID;

			dalPrenotation.ChangePrenotationResponseTypeID(ObjPrenotation);
		}

		public void LoadEvidenzeGiornaliere()
		{
			SrcEvidenzeGiornaliere = dalVisitaProgrammata.GetEvidenzeGiornaliere(CurrentDate);
			SrcEvidenzeGiornaliere.ToList().ForEach(x => x.NestedPropertyChanged += new PropertyChangedEventHandler(OnScheduledVisitNestedPropertyChanged));
			SrcEvidenzeGiornaliere.ToList().ForEach(x => x.GuideForeground = GetGuideColor(x));
			SrcEvidenzeGiornaliere.ToList().ForEach(x => x.IsAvvisaEnabled = GetAvvisaGuida(x));
		}

		#endregion// Lookpup And DB Methods




		#region Prenotation Visits Methods
        public V_Prenotazione GetV_Prenotazione()
        {
            return dalPrenotation.Get_V_Item(ObjPrenotation.Id_Prenotazione);
        }

		public V_VisitePrenotate AddNewPrenotationVisitRow(int languageID, short visitors, bool canRemoveVisits = true)
		{
			//non sia ccettano visite con zero visitatori
			if (visitors == 0)
				return null;

			//Si controlla se la lingua della visita in procinto di essere inserita in lista
			//esiste già in altre lingue della lista
			bool languageExisting = VisitLanguageAlreadyExists(languageID, 0);

			//Si crea una nuova visita prenotata
			V_VisitePrenotate nv = new V_VisitePrenotate();
			nv.Id_VisitaPrenotata = CreatePrenotationVisitID();
			nv.Id_Lingua = languageID;
			nv.Nr_Visitatori = visitors;
			nv.LinguaVisita = new LK_Lingua_Dal().GetItem(nv.Id_Lingua).Descrizione;

			//La visita sarà "disabilitata" se il suo linguaggio è già presente in altra visita
			nv.IsEmpty = (nv.Nr_Visitatori == 0 || languageExisting);
			nv.IsErasable = (nv.Nr_Visitatori > 0 && !languageExisting) && !dalVisitaProgrammata.CheckVisitaPrenotataPagamentoParziale(nv.Id_VisitaPrenotata);

			if (canRemoveVisits)
			{
				//Dato che deve esserci sempre solo una visita disabilitata,
				//se nella lista c'è una visita disabilitata (e lo è in quanto la lingua
				//è incompatibile), si prelevano i suoi visitatori, si rimuove la vecchia
				//visita disabilitata e i visitatori si aggiungono a questa nuova visita
				//che, se con lingua compatibile risulterà abilitata, altrimenti sarè 
				//questa l'unica visita diabilitata.
				foreach (V_VisitePrenotate v in SrcPrenotationVisits)
				{
					if (v.IsEmpty)
					{
						nv.Nr_Visitatori += v.Nr_Visitatori;
						RemovePrenotationVisitRow(v.Id_VisitaPrenotata, false);
						break;
					}
				}
			}

			//Si aggiunge la visita alla lista visite
			SrcPrenotationVisits.Add(nv);

			//Si controlla l'univocità delle lingue, abilitando qualsiasi visita
			//la cui lingua risulti unica nella lista visite.
            //QUI (COMMENTATO CheckVisitUniqueLanguage())
            //CheckVisitUniqueLanguage();

			return nv;
		}
		
		public void RemovePrenotationVisitRow(int visitID, bool checkRemainingVisitors)
		{
			foreach (V_VisitePrenotate v in SrcPrenotationVisits)
			{
				if (v.Id_VisitaPrenotata == visitID)
				{
					//Si controlla se la visita che si sta eliminando ha un omologo disabilitato.
					//In tal caso, anzichè eliminarle la visita se ne integrano i visitatori con 
					//con quelli dell'omologo, per evitare di creare più di una visita disabilitata.
					V_VisitePrenotate vpren = SrcPrenotationVisits.FirstOrDefault(x => x.Id_Lingua == v.Id_Lingua && x.IsEmpty == true);
					if(vpren != null)
					{
						v.Nr_Visitatori += (short)vpren.Nr_Visitatori;
						SrcPrenotationVisits.Remove(vpren);
						CanSave = !IsLoked && !EmptyPrenotationVisitExists();
						return;
					}

					//Si rimuove la visita prenotata dalla lista
					SrcPrenotationVisits.Remove(v);

					//Si rimuovono tutte le visite programmate relative alla visita prenotata rimossa
					ObservableCollection<V_VisiteProgrammate> tmpScheduledVisits = new ObservableCollection<V_VisiteProgrammate>(srcVisiteProgrammateAll.Where(x => x.Id_VisitaPrenotata == v.Id_VisitaPrenotata));
					if(tmpScheduledVisits != null)
						foreach (V_VisiteProgrammate vpro in tmpScheduledVisits)
							srcVisiteProgrammateAll.Remove(vpro);
					SrcVisiteProgrammate = null;

					//Si ricava la nuova rimanenza visitatori rispetto al totale nella prenotazione
					short remainingVisitors = GetRemainingVisitors(false);

					//Se c'è una rimanenza e se è richiesto un controllo rimanenza,
					//viene aggiunta una nuova visita prenotata con lingua = lingua risposta
					//attuale nella prenotazione e vititatori = rimanenza
					if (checkRemainingVisitors && remainingVisitors > 0)
					{
						V_VisitePrenotate vp = AddNewPrenotationVisitRow(ObjPrenotation.Id_LinguaRisposta, remainingVisitors, false);

						//Se l'aggiunta va a buon fine si registra l'evento della variazione
						//del campo "lingua" della visita appena inserita.
						if (vp != null)
							vp.NestedPropertyChanged += new PropertyChangedEventHandler(OnPrenotationVisitNestedPropertyChanged);
					}
					break;
				}
			}

			//Si controlla l'univocità delle lingue, abilitando qualsiasi visita
			//la cui lingua risulti unica nella lista visite.
            //QUI (COMMENTATO CheckVisitUniqueLanguage())
            //CheckVisitUniqueLanguage();

			CanSave = !IsLoked && !EmptyPrenotationVisitExists();
		}

		public bool VisitLanguageAlreadyExists(int languageID, int exludeVisitID)
		{
			bool exists = false;

			foreach (V_VisitePrenotate v in SrcPrenotationVisits.Where(x => x.IsEmpty == false && x.Id_VisitaPrenotata != exludeVisitID))
			{
				if (v.Id_Lingua == languageID)
				{
					exists = true;
					break;
				}
			}

			return exists;
		}

		public short GetRemainingVisitors(bool excludeEmptyVisits)
		{
			return (short)(ObjPrenotation.Tot_Visitatori - GetVisitorsCount(excludeEmptyVisits));
		}

		public int GetVisitorsCount(bool excludeEmptyVisits)
		{
			int vc = 0;

			foreach (V_VisitePrenotate v in SrcPrenotationVisits)
				if (!v.IsEmpty || (v.IsEmpty && !excludeEmptyVisits))
					vc += v.Nr_Visitatori;

			return vc;
		}

		private void CheckVisitUniqueLanguage()
		{
			foreach (V_VisitePrenotate obj in SrcPrenotationVisits)
			{
				if (!VisitLanguageAlreadyExists(obj.Id_Lingua, 0 /* obj.Id_VisitaPrenotata*/))
				{
					obj.IsEmpty = false;
                    obj.IsErasable = !dalVisitaProgrammata.CheckVisitaPrenotataPagamentoParziale(obj.Id_VisitaPrenotata) && !blockAllVisits;
				}
			}
		}

		private int CreatePrenotationVisitID()
		{
			return SrcPrenotationVisits.Count > 0 ? SrcPrenotationVisits.Max(x => x.Id_VisitaPrenotata) + 1 : 1;
		}

		public bool EmptyPrenotationVisitExists()
		{
			bool exists = false;
			if (SrcPrenotationVisits != null && SrcPrenotationVisits.Count > 0 && SrcPrenotationVisits.Where(x => x.IsEmpty == true).Count() > 0)
				exists = true; ;
			return exists;
		}

		#endregion// Prenotation Visits Methods




		#region Scheduled Visits Methods

		public bool AddNewScheduledVisitRow()
		{
			Parametri_Dal dalParameters = new Parametri_Dal();
			//Viene creata una visita programmata
			V_VisiteProgrammate nv = new V_VisiteProgrammate();
			nv.Id_VisitaProgrammata = CreateScheduledVisitID();
			nv.Id_VisitaPrenotata = ObjSelectedPrenotationVisit.Id_VisitaPrenotata;
			nv.Id_Lingua = ObjSelectedPrenotationVisit.Id_Lingua;
			nv.LinguaVisita = ObjSelectedPrenotationVisit.LinguaVisita;
			nv.Id_Prenotazione = ObjPrenotation.Id_Prenotazione;
			nv.Id_TipoVisita = (int)ObjPrenotation.Id_TipoVisita;
			nv.Dt_Visita = CurrentDate.Date;

            var tV = dalTipoVisita.GetItem((int)ObjPrenotation.Id_TipoVisita);

			//L'ora della visita viene impostata come quella di una evidenza giornaliera se:
			//La visita prenotata selezionata ha un numero di visitatori inferiore o uguale 
			//al totale dei visitatori delle visite programmate
			//e se esiste almeno un'evidenza giornaliera (dalla quale prelevare l'orario)
 			//e se inoltre la visita prenotata selezionata è già nel DB e se quella visita prenotata corrisponde
			//a un'evidenza giornaliera.
			//Altrimenti si imposta la data di default
			if (
					ObjSelectedPrenotationVisit.Nr_Visitatori <= GetVisiteProgrammateVisitorsCount(null) 
					&& 
					SrcEvidenzeGiornaliere.Count > 0 
					&& 
					(
						ObjSelectedPrenotationVisit.IsLoadedFromDb
						&&
						SrcEvidenzeGiornaliere.Where(x => x.Id_VisitaPrenotata == ObjSelectedPrenotationVisit.Id_VisitaPrenotata).ToList().Count > 0
					)
				)
				nv.Ora_Visita = SrcEvidenzeGiornaliere.FirstOrDefault(x => x.Id_VisitaPrenotata == ObjSelectedPrenotationVisit.Id_VisitaPrenotata).Ora_Visita;
			else
				nv.Ora_Visita = dalParameters.GetItem("ora_iniziale_visite_default").Valore;
			nv.IsNew = true;
			nv.IsCancelable = true;
			bool retVal = false;

			if (ObjSelectedPrenotationVisit != null)
			{
                nv.Interi = tV.Interi;
                nv.Ridotti = tV.Ridotti;
                nv.Cumulativo = tV.Cumulativo;
                nv.Scontati = tV.Scontati;
                nv.Omaggio = tV.Omaggio;
     
                short visitatoriPerVP = (short)(ObjSelectedPrenotationVisit.Nr_Visitatori - GetVisiteProgrammateVisitorsCount(ObjSelectedPrenotationVisit.Id_Lingua));
                if (tV.Cumulativo)
                {
                    nv.Nr_Cumulativi = visitatoriPerVP;
                    nv.Nr_Cumulativi = nv.Nr_Cumulativi >= 0 ? nv.Nr_Cumulativi : 0;
                }
                else if (tV.Interi)
                {
                    nv.Nr_Interi = visitatoriPerVP;
                    nv.Nr_Interi = nv.Nr_Interi >= 0 ? nv.Nr_Interi : 0;
                }
                else if (tV.Ridotti)
                {
                    nv.Nr_Ridotti = visitatoriPerVP;
                    nv.Nr_Ridotti = nv.Nr_Ridotti >= 0 ? nv.Nr_Ridotti : 0;
                }
                else if (tV.Scontati)
                {
                    nv.Nr_Scontati = visitatoriPerVP;
                    nv.Nr_Scontati = nv.Nr_Scontati >= 0 ? nv.Nr_Scontati : 0;
                }
                else if (tV.Omaggio)
                {
                    nv.Nr_Omaggio = visitatoriPerVP;
                    nv.Nr_Omaggio = nv.Nr_Omaggio >= 0 ? nv.Nr_Omaggio : 0;
                }


				//Si aggiunge la visita alle visite programmate
				SrcVisiteProgrammateAll.Add(nv);

				//Si blocca la possibilità di cambiare lingua alla visita prenotata corrispondente
				SrcPrenotationVisits.FirstOrDefault(x => x.Id_VisitaPrenotata == nv.Id_VisitaPrenotata).IsLanguageEditable = false;

				ScheduleDate = nv.Dt_Visita;

				//Si aggiorna la lista delle visite programmate
				LoadScheduledVisits(ObjSelectedPrenotationVisit);

				retVal = true;
			}

			return retVal;
		}

		public void RemoveScheduledVisitRow(int visitID)
		{
			foreach (V_VisiteProgrammate v in SrcVisiteProgrammate)
			{
				if (v.Id_VisitaProgrammata == visitID)
				{
					SrcVisiteProgrammateAll.Remove(v);
					if(SrcEvidenzeGiornaliere
						.FirstOrDefault(x => x.Id_VisitaPrenotata == v.Id_VisitaPrenotata) == null 
						&&
						SrcVisiteProgrammateAll.Count == 0
					)					
						ScheduleDate = (DateTime?)null;

					SrcPrenotationVisits.FirstOrDefault(x => x.Id_VisitaPrenotata == v.Id_VisitaPrenotata).IsLanguageEditable =
                        !dalVisitaProgrammata.CheckVisitaPrenotataPagamentoParziale(v.Id_VisitaPrenotata,true)
                        //&&
                        //(dalVisitaProgrammata.GetVListByIdVisitaPrenotata(v.Id_VisitaPrenotata) == null || dalVisitaProgrammata.GetVListByIdVisitaPrenotata(v.Id_VisitaPrenotata).Count == 0)
						&&
						SrcVisiteProgrammateAll.Where(x => x.Id_VisitaPrenotata == v.Id_VisitaPrenotata).Count() == 0
						;

                    
                    
					break;
				}
			}
			LoadScheduledVisits(ObjSelectedPrenotationVisit);

			CanSendPetitionerEmail = SrcEvidenzeGiornaliere.Count > 0;

		}

		private void LoadScheduledVisits(V_VisitePrenotate obj)
		{
			if (srcVisiteProgrammateAll != null && DateMessageVisibility != Visibility.Visible)
				SrcVisiteProgrammate = new ObservableCollection<V_VisiteProgrammate>(srcVisiteProgrammateAll.Where(x => x.Id_VisitaPrenotata == obj.Id_VisitaPrenotata));
			else
				SrcVisiteProgrammate = null;
		}

		public int GetSelectedPrenotationVisitVisitorsCount()
		{
			return objSelectedPrenotationVisit.Nr_Visitatori;
		}

		public int GetScheduledVisitsVisitorsCount(V_VisiteProgrammate vPrToEdit)
		{
			int vc = 0;

			foreach (V_VisiteProgrammate v in SrcVisiteProgrammate.Where(vpx => !vpx.Id_VisitaProgrammata.Equals(vPrToEdit.Id_VisitaProgrammata)))
			{
				var i = v.Nr_Interi != null ? (short)v.Nr_Interi : 0;
				var o = v.Nr_Omaggio != null ? (short)v.Nr_Omaggio : 0;
				var r = v.Nr_Ridotti != null ? (short)v.Nr_Ridotti : 0;
                var s = v.Nr_Scontati != null ? (short)v.Nr_Scontati : 0;
                var c = v.Nr_Cumulativi != null ? (short)v.Nr_Cumulativi : 0;
				vc += (i + o + r + s + c);
			}

			var bi = vPrToEdit.Nr_Interi != null ? (short)vPrToEdit.Nr_Interi : 0;
			var bo = vPrToEdit.Nr_Omaggio != null ? (short)vPrToEdit.Nr_Omaggio : 0;
			var br = vPrToEdit.Nr_Ridotti != null ? (short)vPrToEdit.Nr_Ridotti : 0;
            var bs = vPrToEdit.Nr_Scontati != null ? (short)vPrToEdit.Nr_Scontati : 0;
            var bc = vPrToEdit.Nr_Cumulativi != null ? (short)vPrToEdit.Nr_Cumulativi : 0;
			vc += (bi + bo + br + bs + bc);

			return vc;
		}

        public int GetScheduledVisitsVisitorsCount()
        {
            int vc = 0;

            foreach (V_VisiteProgrammate v in SrcVisiteProgrammateAll)
            {
                var i = v.Nr_Interi != null ? (short)v.Nr_Interi : 0;
                var o = v.Nr_Omaggio != null ? (short)v.Nr_Omaggio : 0;
                var r = v.Nr_Ridotti != null ? (short)v.Nr_Ridotti : 0;
                var s = v.Nr_Scontati != null ? (short)v.Nr_Scontati : 0;
                var c = v.Nr_Cumulativi != null ? (short)v.Nr_Cumulativi : 0;
                vc += (i + o + r + s + c);
            }

            return vc;
        }

        public bool CheckAccountingData()
        {
            bool retVal = true;
            int interiDaProgr = 0;
            int ridottiDaProgr = 0;
            int interiProgr = 0;
            int ridottiProgr = 0;

            Pagamento p = dalPagamento.GetItemByIdPrenotazione(ObjPrenotation.Id_Prenotazione);
           
            if ((p != null && p.Dt_Pagamento != null) && !p.FL_PagamentoParziale)
            {
                //pagamento totale
                foreach (V_VisiteProgrammate v in SrcVisiteProgrammateAll)
                {
                    interiDaProgr += v.Nr_Interi != null ? (short)v.Nr_Interi : 0;
                    ridottiDaProgr += v.Nr_Ridotti != null ? (short)v.Nr_Ridotti : 0;
                }

                foreach (V_EvidenzeGiornaliere v in dalVisitaProgrammata.GetEvidenzeGiornaliere(ObjPrenotation.Id_Prenotazione).ToList())
                {
                    interiProgr += v.Nr_Interi != null ? (short)v.Nr_Interi : 0;
                    ridottiProgr += v.Nr_Ridotti != null ? (short)v.Nr_Ridotti : 0;
                }

                retVal = (interiDaProgr == interiProgr && ridottiDaProgr == ridottiProgr);
            }
            


            return retVal;
        }

		public int GetVisiteProgrammateVisitorsCount(int? languageID)
		{
			int vc = 0;

			foreach (V_VisiteProgrammate v in SrcVisiteProgrammateAll.Where(x => languageID != null ? x.Id_Lingua == languageID : true))
			{
				var i = v.Nr_Interi != null ? (short)v.Nr_Interi : 0;
				var o = v.Nr_Omaggio != null ? (short)v.Nr_Omaggio : 0;
				var r = v.Nr_Ridotti != null ? (short)v.Nr_Ridotti : 0;
				vc += (i + o + r);
			}

			return vc;
		}

		public int GetEvidenzeGiornaliereVisitorsCount()
		{
			int vc = 0;

			foreach (V_EvidenzeGiornaliere eg in SrcEvidenzeGiornaliere)
			{
				var vg = String.IsNullOrEmpty(eg.NumeroVisitatoriGruppo) ? 0 : Convert.ToInt32(eg.NumeroVisitatoriGruppo);
				vc += vg;
			}

			return vc;
		}

		public int GetEvidenzeGiornaliereGroupHeaderVisitorsCount(V_VisiteProgrammate vProg)
		{
			int vc = 0;

			var groupHeader = SrcEvidenzeGiornaliere
				.FirstOrDefault(lx => 
					lx.Ora_Visita == vProg.Ora_Visita
					&& 
					lx.Id_TipoVisita == ObjPrenotation.Id_TipoVisita
					&& 
					lx.LinguaVisita == vProg.LinguaVisita
					&&
					lx.Id_Guida.Equals(vProg.Id_Guida)
					);

			if (groupHeader != null)
			{
				if (!String.IsNullOrEmpty(groupHeader.NumeroVisitatoriGruppo))
					vc = Convert.ToInt32(groupHeader.NumeroVisitatoriGruppo);
			}

			return vc;
		}

		public bool CheckVisitaProgrammata(V_VisiteProgrammate vProg, out string message)
		{
			bool retVal = true;

			message = "";

			ConflictResult = dalVisitaProgrammata.CheckConflittoVisite(new List<V_VisiteProgrammate>() { vProg }, CurrentDate);
			switch(ConflictResult.Conflict)
			{
                //case ConflictType.Unacceptable:
                //    retVal = false;
                //    message = "L'ora indicata coincide con quella di una visita programmata in un'altra lingua.";
                //    break;

                //case ConflictType.Acceptable:
                //    retVal = true;
                //    message = "L'ora indicata coincide con quella di una visita programmata del tipo 'Speciale Scavi'. Continuare?";
                //    break;

				case ConflictType.NoConflict:
                default:
                    break;

					break;
			}

			foreach (V_VisiteProgrammate v in SrcVisiteProgrammateAll)
			{
				if (ConflictResult.Conflicts.Select(x => x.ID).Contains(v.Id_VisitaProgrammata))
					v.ConflictType = ConflictResult.Conflicts.FirstOrDefault(x => x.ID == v.Id_VisitaProgrammata).ConflictType;
			}

			foreach (V_VisiteProgrammate v in SrcVisiteProgrammate)
			{
				if (ConflictResult.Conflicts.Select(x => x.ID).Contains(v.Id_VisitaProgrammata))
					v.ConflictType = ConflictResult.Conflicts.FirstOrDefault(x => x.ID == v.Id_VisitaProgrammata).ConflictType;
			}

			return retVal;
		}

		private int CreateScheduledVisitID()
		{
			return SrcVisiteProgrammateAll.Count > 0 ? SrcVisiteProgrammateAll.Max(x => x.Id_VisitaProgrammata) + 1 : 1;
		}

		private SolidColorBrush GetGuideColor(V_EvidenzeGiornaliere obj)
		{
			return ((obj.Id_Guida != null && obj.Id_Guida > 0) && (obj.Fl_AccettaGuida == false || obj.Fl_AccettaGuida == null)) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
		}

		private bool GetAvvisaGuida(V_EvidenzeGiornaliere obj)
		{
			return (obj.Id_Guida == null || obj.Id_Guida == 0) ? false : true;
		}

        public void CheckConflicts()
        {
            ConflictResult = dalVisitaProgrammata.CheckConflittoVisite(SrcVisiteProgrammateAll.ToList(), CurrentDate);
            foreach (V_VisiteProgrammate v in SrcVisiteProgrammateAll)
            {
                if (ConflictResult.Conflicts.Select(x => x.ID).Contains(v.Id_VisitaProgrammata))
                    v.ConflictType = ConflictResult.Conflicts.FirstOrDefault(x => x.ID == v.Id_VisitaProgrammata).ConflictType;
            }
            //if (ConflictResult.Conflict == ConflictType.Unacceptable)
            //{
            //    MessageBox.Show("Una o più visite programmate sono in conflitto ora/lingua con altre visite precedentemente programmate. Controllare le evidenze giornaliere.");
            //    return false;
            //}

            //return true;
        }

		#endregion// Scheduled Visit Methods




		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion




		#region Utils

		public bool CheckDateTime(DateTime dtToCheck, string oraVisita)
		{
			return dalChiusura.CheckDateTime(dtToCheck, oraVisita);
		}

        public bool CheckPayment(int prenotationID)
        {
            //ONLINEPAYMENT

            bool paid = false;
            //Si ricavano i pagamenti relativi alla prenotazione
            List<Pagamento> p = new Pagamento_Dal().GetListByIdPrenotazione(prenotationID);
            if (p != null && p.Count > 0)
            {
                if (p[0].Dt_Pagamento != null)
                {
                    //Se è un pagamento parziale, la prenotazione è totalmente pagata
                    //se il numero di pagamenti (visite pagate) equivale al numero
                    //delle visite per questo protocollo.
                    //Se invece non è parziale, la prenotazione risulta interamente
                    //pagata con l'unico pagamento totale.
                    if (p.Count > 0 && p[0].FL_PagamentoParziale)
                    {
                        if (!(p.Count < SrcVisiteProgrammateAll.Count))
                            paid = true;
                    }
                    else
                        paid = true;
                }
            }

            return paid;
        }

		public void SetConflicts()
		{
			ConflictResult r = new ConflictResult();

			r = dalVisitaProgrammata.CheckConflittoVisite(SrcVisiteProgrammateAll.ToList(), CurrentDate);

			foreach (V_VisiteProgrammate v in SrcVisiteProgrammateAll)
			{
				foreach (V_VisiteProgrammate vpg in SrcVisiteProgrammate)
				{
					if (vpg.Id_VisitaProgrammata == v.Id_VisitaProgrammata)
					{
						v.Ora_Visita = vpg.Ora_Visita;
						v.Id_Lingua = vpg.Id_Lingua;
					}
				}

				if (ConflictResult.Conflicts.Select(x => x.ID).Contains(v.Id_VisitaProgrammata))
					v.ConflictType = ConflictResult.Conflicts.FirstOrDefault(x => x.ID == v.Id_VisitaProgrammata).ConflictType;
			}

			LoadScheduledVisits(ObjSelectedPrenotationVisit);
		}

		public ConflictResult GetConflicts()
		{
			ConflictResult r = new ConflictResult();

			foreach (V_VisiteProgrammate vp in SrcVisiteProgrammateAll)
			{
				if (vp.ConflictType == ConflictType.Unacceptable)
				{
					r.Conflict = ConflictType.Unacceptable;
					r.Conflicts.Add(new Conflict(vp.Id_VisitaProgrammata, ConflictType.Unacceptable));
				}
			}

			return r;
		}

		public int CountScheduledByPrenotated(int prenotationVisitID)
		{
			int n = 0;

			n = SrcVisiteProgrammateAll.Where(x => x.Id_VisitaPrenotata == prenotationVisitID).Count();

			return n;
		}

        //ONLINEPAYMENT
        public bool CheckIfOnlinePaymentIsAllowed(int prenotationID, out DateTime checkDate)
        {
            //LA CREAZIONE DELL'ORDINE DI PAGAMENTO E' CONSENTITA SE LA DATA DELLA VISITA
            //E' SUCCESSIVA ALLA DATA OTTENUTA AGGIUNGENDO ALLA DATA ATTUALE UN NUMERO
            //DI GIORNI INDICATI DAL PARAMETRO 'onlinePaymentDaysAfterAllowed'
            var visitDate = dalVisits.GetVListByIdPrenotazione(prenotationID)[0].Dt_Visita;
            int onlinePaymentDaysAfterAllowed = Convert.ToInt32(dalParameters.GetItem("onlinePaymentDaysAfterAllowed").Valore);

            checkDate = DateTime.Now.AddDays(onlinePaymentDaysAfterAllowed).Date;
            return (visitDate.Date > DateTime.Now.AddDays(onlinePaymentDaysAfterAllowed).Date);
        }

        public System.Windows.Media.Brush GetSummaryElementColor(string hour)
        {
            //ObjClosingDateResult
            System.Windows.Media.Brush color = System.Windows.Media.Brushes.Black;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("it-IT");

            TimeSpan tsOra_inizio_visite_pm = TimeSpan.ParseExact(ora_inizio_visite_pm, "g", culture);
            TimeSpan tsTimeoraVisita = TimeSpan.ParseExact(hour, "g", culture);

            if (ObjClosingDateResult.ClosedAM && ObjClosingDateResult.ClosedPM)
            {
                color = System.Windows.Media.Brushes.Red;
            }
            else if (ObjClosingDateResult.ClosedAM)
            {
                if (tsTimeoraVisita < tsOra_inizio_visite_pm)
                    color = System.Windows.Media.Brushes.Red;
            }
            else if (ObjClosingDateResult.ClosedPM)
            {
                if (tsTimeoraVisita >= tsOra_inizio_visite_pm)
                    color = System.Windows.Media.Brushes.Red;
            }



            return color;
        }

		#endregion




		#region Events Handling

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null && (!Loading))
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

			switch (propertyName)
			{
				case "IsPrenotationVisitSelected":
					if (ScheduleDate == null || CurrentDate == ScheduleDate)
					{
						if (ObjSelectedPrenotationVisit != null)
							ObjSelectedPrenotationVisit.AvailableLanguages = AvailableLanguages;

						CanScheduleVisit = IsPrenotationVisitSelected && !blockAllVisits;
					}
					break;

				case "CurrentDate":
					ObjClosingDateResult = dalChiusura.CheckDate(CurrentDate);

                    if (!Loading)
                    {
                        if (ScheduleDate != null && (dalVisitaProgrammata.GetPrenotazionePagamentoParziale(ObjPrenotation.Id_Prenotazione) && CurrentDate != ScheduleDate))
                        {
                            CanScheduleVisit = false;
                            DateMessage = "Visite pagate già programmate in data " + ScheduleDate.Value.ToShortDateString();
                            DateMessageVisibility = Visibility.Visible;
                            LoadScheduledVisits(ObjSelectedPrenotationVisit);
                            CanSave = false;
                            CanPayFull = false;
                        }
                        else
                        {
                            CanScheduleVisit = IsPrenotationVisitSelected && !blockAllVisits;
                            DateMessage = string.Empty;
                            DateMessageVisibility = Visibility.Collapsed;


                            if(SrcVisiteProgrammateAll.Count > 0)
                            {
                                if (SrcVisiteProgrammateAll[0].Dt_Visita != CurrentDate)
                                {

                                    //ALLA MODIFICA DELLA DATA IMPOSTIAMO PER IL PROSSIMO EVENTUALE SALVATAGGIO 
                                    //LA GUIDA GIA' EVENTUALMENTE IMPOSTATA PER IL GRUPPO DI VISITE NELLA DATA APPENA IMPOSTATA
                                    SrcVisiteProgrammateAll.Where(x => x.Dt_Visita != CurrentDate).ToList().ForEach(x => { x.Id_Guida = null; x.Fl_AccettaGuida = null; });
                                    var evidenzeGiornaliereBeforeUpd = dalVisitaProgrammata.GetEvidenzeGiornaliere(CurrentDate)
                                                                                           .Where(evx => evx.IsParentItem == true).ToList();
                                    foreach (V_EvidenzeGiornaliere eg in evidenzeGiornaliereBeforeUpd)
                                    {
                                        SrcVisiteProgrammateAll.Where(x => x.Ora_Visita == eg.Ora_Visita 
                                                                           && x.Id_Lingua == eg.Id_Lingua
                                                                           && x.Id_TipoVisita == eg.Id_TipoVisita).ToList().ForEach(x => { x.Id_Guida = eg.Id_Guida; x.Fl_AccettaGuida = eg.Fl_AccettaGuida; });
                                    }
                                }

                            }
                            

                            
                            
                            
                            
                            SrcVisiteProgrammateAll.ToList().ForEach(x => x.Dt_Visita = CurrentDate);
                            ConflictResult = dalVisitaProgrammata.CheckConflittoVisite(SrcVisiteProgrammateAll.ToList(), CurrentDate);
                            foreach (V_VisiteProgrammate v in SrcVisiteProgrammateAll)
                            {
                                if (ConflictResult.Conflicts.Select(x => x.ID).Contains(v.Id_VisitaProgrammata))
                                    v.ConflictType = ConflictResult.Conflicts.FirstOrDefault(x => x.ID == v.Id_VisitaProgrammata).ConflictType;
                            }
                            //if (ConflictResult.Conflict == ConflictType.Unacceptable)
                            //    MessageBox.Show("Una o più visite programmate sono in conflitto ora/lingua con altre visite precedentemente programmate. Controllare le evidenze giornaliere.");
                            ObjSelectedPrenotationVisit = (SrcPrenotationVisits != null && SrcPrenotationVisits.Count > 0) ? SrcPrenotationVisits[0] : null;
                            LoadScheduledVisits(ObjSelectedPrenotationVisit);
                            CanSave = !IsLoked;
                            if (
                                SrcVisiteProgrammateAll.Count > 0
                                //&&
                                //SrcVisiteProgrammateAll[0].Dt_Visita >= DateTime.Now.Date
                                && SrcVisiteProgrammateAll.Count == SrcEvidenzeGiornaliere.Where(x => x.VP_IdPrenotazione == ObjPrenotation.Id_Prenotazione).Count())
                                CanPayFull = true;
                        }


                    }

					break;

				case "SrcVisiteProgrammateAll":
					CanSendPetitionerEmail =
						(SrcVisiteProgrammateAll.Count > 0 && ObjPrenotation.Id_TipoRisposta != 9)
						||
						(
						(SrcVisiteProgrammateAll.Count == 0)
						&&
							(
							ObjPrenotation.Id_TipoRisposta == 1
							||
							ObjPrenotation.Id_TipoRisposta == 5
							||
							ObjPrenotation.Id_TipoRisposta == 6
							||
							ObjPrenotation.Id_TipoRisposta == 7
							||
							ObjPrenotation.Id_TipoRisposta == 8
							)
						);
					break;

				case "SrcEvidenzeGiornaliere" :
					ScheduledVisitsSummaryLabel = "Visite giornaliere - Totale visitatori: " + GetEvidenzeGiornaliereVisitorsCount().ToString();
					break;

				case "SelectedEvidenzeGiornaliere":
					if (!Loading && SelectedEvidenzeGiornaliere != null)
					{
						SelectedEvidenzeGiornaliere.AvailableGuides = new ObservableCollection<V_GuideDisponibili>(dalGuideDisponibili.GetV_GuideDisponibili(SelectedEvidenzeGiornaliere));
                        CanPaySingle =
                            SelectedEvidenzeGiornaliere.Id_Prenotazione == ObjPrenotation.Id_Prenotazione;
							
					}
					break;
			}
			//Console.WriteLine("OnPropertyChanged called " + (++i0).ToString() + " times");
		}
		
		public void OnPrenotationVisitNestedPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Loading) return;

			V_VisitePrenotate obj = sender as V_VisitePrenotate;
			if (obj != null)
			{
				obj.NestedPropertyChanged -= new PropertyChangedEventHandler(OnPrenotationVisitNestedPropertyChanged);
				switch (e.PropertyName)
				{
					//Questo case proviene dalla evento EditEnded della griglia, nel View.
					//Non proviene dall'effettivo cambiamento della proprietà "Nr_Visitatori"
					//perchè viene triggerato ad ogni cambiamento del numero, cosa che rende 
					//impossibile un ricalcolo unico sui numeri a due cifre (viene fatto un ricalcolo
					//ad ogni cifra digitata). In questo modo, invece, l'evento viene triggerato
					//dall'evento "EditEnded" nel view, cioè quando l'utente ha terminato l'intera modifica della cifra.
					case "Nr_Visitatori":
						short remainingVisitors = GetRemainingVisitors(false);
						if (remainingVisitors > 0)
						{
							V_VisitePrenotate v = AddNewPrenotationVisitRow(ObjPrenotation.Id_LinguaRisposta, remainingVisitors);
							if (v != null)
								v.NestedPropertyChanged += new PropertyChangedEventHandler(OnPrenotationVisitNestedPropertyChanged);
						}
						if (remainingVisitors < 0)
						{
							obj.Nr_Visitatori += remainingVisitors;
						}
						break;

					case "Id_Lingua":

                        //QUI (DECOMMENTATO TRE RIGHE SEGUENTI)
                        obj.IsEmpty = false ;
                        obj.IsErasable = !dalVisitaProgrammata.CheckVisitaPrenotataPagamentoParziale(obj.Id_VisitaPrenotata) && !blockAllVisits;
                        obj.LinguaVisita = new LK_Lingua_Dal().GetItem(obj.Id_Lingua).Descrizione;

                        //QUI (COMMENTATO IF ELSE SEGUENTE)
                        //if (!VisitLanguageAlreadyExists(obj.Id_Lingua, obj.Id_VisitaPrenotata))
                        //{
                        //    obj.IsEmpty = false ;
                        //    obj.IsErasable = !dalVisitaProgrammata.CheckVisitaPrenotataPagamentoParziale(obj.Id_VisitaPrenotata) && !blockAllVisits;
                        //    obj.LinguaVisita = new LK_Lingua_Dal().GetItem(obj.Id_Lingua).Descrizione;
                        //}
                        //else
                        //{
                        //    obj.IsEmpty = true;
                        //    obj.IsErasable = false;
                        //}

                        //QUI (COMMENTATO CheckVisitUniqueLanguage())
                        //CheckVisitUniqueLanguage();
						break;
				}

				//CanScheduleVisit = !EmptyPrenotationVisitExists();
				CanSave = !IsLoked && !EmptyPrenotationVisitExists();

				obj.NestedPropertyChanged += new PropertyChangedEventHandler(OnPrenotationVisitNestedPropertyChanged);
			}
			//Console.WriteLine("OnPrenotationVisitNestedPropertyChanged called " + (++i1).ToString() + " times");
		}

		public void OnPrenotationNestedPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Loading) return;

			Prenotazione p = ObjPrenotation;

			switch (e.PropertyName)
			{
				case "Tot_Visitatori":
					short remainingVisitors = GetRemainingVisitors(false);
					if (remainingVisitors > 0)
					{
						V_VisitePrenotate v = AddNewPrenotationVisitRow(ObjPrenotation.Id_LinguaRisposta, remainingVisitors);
						if (v != null)
							v.NestedPropertyChanged += new PropertyChangedEventHandler(OnPrenotationVisitNestedPropertyChanged);
					}

					//CanScheduleVisit = !EmptyPrenotationVisitExists();
					CanSave = !IsLoked && !EmptyPrenotationVisitExists();
					break;

				case "Dt_VisiteDA":
					if (p.Dt_VisiteA < p.Dt_VisiteDA)
						p.Dt_VisiteA = p.Dt_VisiteDA;
					if (CurrentDate < p.Dt_VisiteDA)
					{
						CurrentDate = p.Dt_VisiteDA;
						ScheduleDate = CurrentDate;
					}
					ObjPrenotation = p;
					break;

				case "Dt_VisiteA":
					if (p.Dt_VisiteDA > p.Dt_VisiteA)
						p.Dt_VisiteDA = p.Dt_VisiteA;
					if (CurrentDate > p.Dt_VisiteA)
					{
						CurrentDate = p.Dt_VisiteA;
						ScheduleDate = CurrentDate;
					}
					ObjPrenotation = p;
					break; 

				case "Id_TipoRisposta":
					CanSendPetitionerEmail =
						(SrcVisiteProgrammateAll.Count > 0 && ObjPrenotation.Id_TipoRisposta != 9)
						||
						(
						(SrcVisiteProgrammateAll.Count == 0)
						&&
							(
							ObjPrenotation.Id_TipoRisposta == 1
							||
							ObjPrenotation.Id_TipoRisposta == 5
							||
							ObjPrenotation.Id_TipoRisposta == 6
							||
							ObjPrenotation.Id_TipoRisposta == 7
							||
							ObjPrenotation.Id_TipoRisposta == 8
							)
						)
						;

					break;
			}
			//Console.WriteLine("OnPrenotationNestedPropertyChanged called " + (++i2).ToString() + " times");
		}


        public V_Prenotazione GetVPrenotazione()
        {
            return dalPrenotation.Get_V_Item(ObjPrenotation.Id_Prenotazione);
        }

		public void OnScheduledVisitNestedPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Loading) return;

			V_EvidenzeGiornaliere obj = sender as V_EvidenzeGiornaliere;
			if (obj != null)
			{
				obj.NestedPropertyChanged -= new PropertyChangedEventHandler(OnScheduledVisitNestedPropertyChanged);
				switch (e.PropertyName)
				{
					case "Id_Guida":
						//Abilitazione della casella "Avvisa" in base all'ID della guida (id==0 o null = disabilitata) 
						obj.IsAvvisaEnabled = GetAvvisaGuida(obj);

						//Se si disabilita la casella di avviso, significa che la guida è N.D. o null, quindi si svuota la casella "Accetta"
						if (!obj.IsAvvisaEnabled)
						{
							obj.Fl_AvvisaGuida = false;
							obj.Fl_AccettaGuida = false;
						}
						
						//Quando si seleziona una guida è necessario impostarne il nominativo qui, per provocare l'aggiornamento
						//delle nested properties Cognome e Nome, che vengono così mostrate sia in edit (ComboBox) che in modalità
						//readonly della griglia (TextBlock)
						if(obj.Id_Guida != null && obj.AvailableGuides.Count > 0)
						{
							V_GuideDisponibili g = obj.AvailableGuides.FirstOrDefault(x => x.Id_Guida == obj.Id_Guida);
							if (g != null)
							{
								obj.Cognome = obj.AvailableGuides.FirstOrDefault(x => x.Id_Guida == obj.Id_Guida).Cognome;
								obj.Nome = obj.AvailableGuides.FirstOrDefault(x => x.Id_Guida == obj.Id_Guida).Nome;
							}
						}
						break;

					case "Fl_AccettaGuida":
						//Si cambia il colore del nominativo guida in base al valore di AccettaGuida
						//obj.GuideForeground = GetGuideColor(obj);
						break;
				}

				obj.GuideForeground = GetGuideColor(obj);

				obj.NestedPropertyChanged += new PropertyChangedEventHandler(OnScheduledVisitNestedPropertyChanged);
				//Console.WriteLine("OnScheduledVisitNestedPropertyChanged called " + (++i3).ToString() + " times");
			}
		}

		#endregion// Event Handling
	}
}