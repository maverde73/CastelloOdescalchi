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

namespace Presentation
{
	public class PrenotationViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events




		#region DAL

		public Prenotazione_Dal dalPrenotation = new Prenotazione_Dal();
		public LK_Lingua_Dal dalLanguage = new LK_Lingua_Dal();
		public LK_TipoVisita_Dal dalVisitType = new LK_TipoVisita_Dal();
		public LK_TipoConferma_Dal dalConfirmationType = new LK_TipoConferma_Dal();
		public Richiedente_Dal dalPetitioner = new Richiedente_Dal();
		public LK_Titolo_Dal dalTitle = new LK_Titolo_Dal();
		public LK_Organizzazione_Dal dalOrganization = new LK_Organizzazione_Dal();
		public LK_Citta_Dal dalCity = new LK_Citta_Dal();
		public VisitaPrenotata_Dal dalPrenotationVisit = new VisitaPrenotata_Dal();
        public VisitaProgrammata_Dal dalVisitaProgrammata = new VisitaProgrammata_Dal();

		#endregion



		#region Private Fields

		private int prenotationID = 0;
		private bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
		public int DefaultVisitLanguageID { get; set; }

		private ObservableCollection<VisitaPrenotata> srcPrenopationVisits = null;
		private ObservableCollection<V_Prenotazione> srcPetitionerPrenotations = null;
		private Prenotazione objPrenotation = null;
		private Richiedente objPetitioner = null;
		private LK_Lingua objPetitionerLanguage = null;
		private LK_Titolo objPetitionerTitle = null;
		private LK_Organizzazione objPetitionerOrganization = null;
		private LK_Citta objPetitionerCity = null;
		
		#endregion// Private Fields




		#region Public Properties

		public ObservableCollection<VisitaPrenotata> SrcPrenotationVisits
		{
			get
			{
				if (srcPrenopationVisits == null)
					srcPrenopationVisits = new ObservableCollection<VisitaPrenotata>();
				return srcPrenopationVisits; 
			}
			set { srcPrenopationVisits = value; OnPropertyChanged(this, "SrcPrenotationVisits"); }
		}

		public ObservableCollection<V_Prenotazione> SrcPetitionerPrenotations
		{
			get	{ return srcPetitionerPrenotations;	}
			set { srcPetitionerPrenotations = value; OnPropertyChanged(this, "SrcPetitionerPrenotations"); }
		}

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

		public Richiedente ObjPetitioner
		{
			get 
			{ 
				if(objPetitioner == null)
					objPetitioner = new Richiedente();
				return objPetitioner; 
			}
			set { objPetitioner = value; OnPropertyChanged(this, "ObjPetitioner"); }
		}

		public LK_Lingua ObjPetitionerLanguage
		{
			get { if (objPetitionerLanguage == null) objPetitionerLanguage = new LK_Lingua(); return objPetitionerLanguage; }
			set { objPetitionerLanguage = value; OnPropertyChanged(this, "ObjPetitionerLanguage"); }
		}

		public LK_Titolo ObjPetitionerTitle
		{
			get { return objPetitionerTitle; }
			set { objPetitionerTitle = value; OnPropertyChanged(this, "ObjPetitionerTitle"); }
		}

		public LK_Organizzazione ObjPetitionerOrganization
		{
			get { return objPetitionerOrganization; }
			set { objPetitionerOrganization = value; OnPropertyChanged(this, "ObjPetitionerOrganization"); }
		}

		public LK_Citta ObjPetitionerCity
		{
			get { return objPetitionerCity; }
			set { objPetitionerCity = value; OnPropertyChanged(this, "ObjPetitionerCity"); }
		}

		#endregion // Properties




		#region Constructors

		public PrenotationViewModel(int detailID)
		{
			DefaultVisitLanguageID = dalLanguage.GetDefault().Id_Lingua;

            prenotationID = detailID;
			LoadAvailableTitles();
			LoadAvailableLanguages();
			LoadAvailableOrganizations();
			LoadAvailableCities();
			LoadAvailableVisitTypes();
			LoadAvailableConfirmationTypes();
			LoadAvailablePetitioners();

			if (detailID > 0)
			{
				BaseFilter filter = new BaseFilter();
				filter.SortDirection = SortDirection.ASC;
				int count = 0;
				ObjPrenotation = dalPrenotation.GetItem(detailID);
				SrcPrenotationVisits = new ObservableCollection<VisitaPrenotata>(dalPrenotationVisit.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count).ToList().Where(x => x.Id_Prenotazione == detailID));
				ObjPetitioner = dalPetitioner.GetItem(ObjPrenotation.Id_Richiedente);
				ObjPetitionerLanguage = dalLanguage.GetItem(ObjPetitioner.Id_LinguaAbituale);
				ObjPetitionerTitle = ObjPetitioner.Id_Titolo != null ? dalTitle.GetItem((int)ObjPetitioner.Id_Titolo) : null;
				ObjPetitionerOrganization = ObjPetitioner.Id_Organizzazione != null ? dalOrganization.GetItem((int)ObjPetitioner.Id_Organizzazione) : null;
				ObjPetitionerCity = ObjPetitioner.Id_Citta != null ? dalCity.GetItem((int)ObjPetitioner.Id_Citta) : null;
			}
			else
			{
				ObjPetitioner = new Richiedente();
				ObjPetitioner.Id_LinguaAbituale = dalLanguage.GetDefault().Id_Lingua;

				ObjPrenotation.Dt_Prenotazione = DateTime.Now;
				ObjPrenotation.Id_LinguaRisposta = dalLanguage.GetDefault().Id_Lingua;
				ObjPrenotation.Id_TipoConferma = dalConfirmationType.GetDefault().Id_TipoConferma;
				ObjPrenotation.Id_TipoVisita = dalVisitType.GetDefault().Id_TipoVisita;
				ObjPrenotation.Id_TipoRisposta = 9; //da lavorare
				ObjPrenotation.Fl_AM = true;
				ObjPrenotation.Fl_PM = true;
			}

			ObjPrenotation.NestedPropertyChanged += new PropertyChangedEventHandler(ObjPrenotation_NestedPropertyChanged);
			ObjPetitioner.ValidateAddress = true;
		}

		#endregion// Constructors




		#region Event Handling

		public void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

			switch (propertyName)
			{
				case "ObjPetitioner":
					if (ObjPetitioner != null && ObjPetitioner.Id_Richiedente > 0)
					{
						int count = 0;
						BaseFilter filter = new BaseFilter();
						filter.AddFilter("Id_Richiedente", Scv_Model.Common.Utilities.ValueType.Int, ObjPetitioner.Id_Richiedente);
						SrcPetitionerPrenotations = new ObservableCollection<V_Prenotazione>(dalPrenotation.GetFilteredList_V_Prenotazione(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count));
					}
					else
						SrcPetitionerPrenotations = null;
					break;
			}
		}

		void ObjPrenotation_NestedPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Tot_Visitatori":
					short remainingVisitors = (short)(ObjPrenotation.Tot_Visitatori - SrcPrenotationVisits.Sum(x => x.Nr_Visitatori));

					if (remainingVisitors > 0)
					{
						VisitaPrenotata v = SrcPrenotationVisits.FirstOrDefault(x => x.Id_Lingua == ObjPrenotation.Id_LinguaRisposta);
						if (v != null)
							v.Nr_Visitatori += remainingVisitors;
						else
						{
							v = new VisitaPrenotata();
							v.Id_Lingua = ObjPrenotation.Id_LinguaRisposta;
							v.Nr_Visitatori = remainingVisitors;
							SrcPrenotationVisits.Add(v);
						}
					}
					break;

				case "Dt_VisiteDA":
				case "Dt_VisiteA":
					if (ObjPrenotation.Dt_VisiteDA < DateTime.Now.Date)
						ObjPrenotation.Dt_VisiteDA = DateTime.Now.Date;

					if (ObjPrenotation.Dt_VisiteA < DateTime.Now.Date)
						ObjPrenotation.Dt_VisiteA = DateTime.Now.Date;
					break;
			}
		}

		#endregion// Event Handling




		#region Main Methods

		public List<Prenotazione> GetPrenotationSingleItem(int id)
		{
			return dalPrenotation.GetSingleItem(id);
		}

		public void ResetRichiedente()
		{
			Richiedente oldPetitioner = ObjPetitioner;

			ObjPetitioner = new Richiedente();
			ObjPetitioner.Cognome = oldPetitioner.Cognome;
			ObjPetitioner.Nome = string.Empty;
			ObjPetitioner.Id_LinguaAbituale = dalLanguage.GetDefault().Id_Lingua;

			ObjPetitionerTitle = null;
			ObjPetitionerOrganization = null;
			ObjPetitionerCity = null;

		}


        public V_Prenotazione GetV_Prenotazione(int idPrenotazione)
        {
            return dalPrenotation.Get_V_Item(idPrenotazione);
        }

		#endregion // Main Methods




		#region Title

		private ObservableCollection<LK_Titolo> availableTitles = null;
		public ObservableCollection<LK_Titolo> AvailableTitles
		{
			get
			{
				if (availableTitles == null)
					availableTitles = new ObservableCollection<LK_Titolo>();
				return availableTitles;
			}
			set { availableTitles = value; }
		}

		private void LoadAvailableTitles()
		{
			BaseFilter filter = new BaseFilter();
			filter.AddSortField("Sigla");
			filter.SortDirection = SortDirection.ASC;
			int count = 0;
			AvailableTitles = new ObservableCollection<LK_Titolo>(dalTitle.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count));
		}

		public LK_Titolo GetTitleByText(string text)
		{
			return dalTitle.GetItemByText(text);
		}

		public LK_Titolo GePetitionerTitleSingleItem(int id)
		{
			return dalTitle.GetItem(id);
		}

		#endregion// Title




		#region Language

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

		public ObservableCollection<LK_Lingua> AvailablePetitionerLanguages
		{
			get
			{
				if (availableLanguages == null)
					availableLanguages = new ObservableCollection<LK_Lingua>();
				return new ObservableCollection<LK_Lingua>(availableLanguages.Where(x => x.Fl_Comunicazione == (bool)true));
			}
		}

		public ObservableCollection<LK_Lingua> AvailablePrenotationLanguages
		{
			get
			{
				if (availableLanguages == null)
					availableLanguages = new ObservableCollection<LK_Lingua>();
				return new ObservableCollection<LK_Lingua>(availableLanguages.Where(x => x.Fl_Comunicazione == (bool)true));
			}
		}

		private void LoadAvailableLanguages()
		{
			AvailableLanguages = new ObservableCollection<LK_Lingua>(dalLanguage.GetItems());
		}

		public List<LK_Lingua> GetPrenotationLanguageSingleItem(int id)
		{
			return dalLanguage.GetSingleItem(id);
		}

		public LK_Lingua GePetitionerLanguageSingleItem(int id)
		{
			return dalLanguage.GetItem(id);
		}

		#endregion// Language




		#region Organization

		private ObservableCollection<LK_Organizzazione> availableOrganizations = null;
		public ObservableCollection<LK_Organizzazione> AvailableOrganizations
		{
			get
			{
				if (availableOrganizations == null)
					availableOrganizations = new ObservableCollection<LK_Organizzazione>();
				return availableOrganizations;
			}
			set { availableOrganizations = value; }
		}

		private void LoadAvailableOrganizations()
		{
			BaseFilter filter = new BaseFilter();
			filter.AddSortField("Descrizione");
			filter.SortDirection = SortDirection.ASC;
			int count = 0;

			AvailableOrganizations = new ObservableCollection<LK_Organizzazione>(dalOrganization.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count));
		}

		public LK_Organizzazione GetOrganizationByText(string text)
		{
			return dalOrganization.GetItemByText(text);
		}

		public LK_Organizzazione GePetitionerOrganizationSingleItem(int id)
		{
			return dalOrganization.GetItem(id);
		}

		#endregion// Organization




		#region City

		private ObservableCollection<LK_Citta> availableCities = null;
		public ObservableCollection<LK_Citta> AvailableCities
		{
			get
			{
				if (availableCities == null)
					availableCities = new ObservableCollection<LK_Citta>();
				return availableCities;
			}
			set { availableCities = value; }
		}

		private void LoadAvailableCities()
		{
			BaseFilter filter = new BaseFilter();
			filter.AddSortField("Nome");
			filter.SortDirection = SortDirection.ASC;
			int count = 0;

			AvailableCities = new ObservableCollection<LK_Citta>(dalCity.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count));
		}

		public LK_Citta GetCityByText(string text)
		{
			return dalCity.GetItemByText(text);
		}

		#endregion// City




		#region Petitioners

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

		private void LoadAvailablePetitioners()
		{
			BaseFilter filter = new BaseFilter();
			filter.AddSortField("Cognome");
			filter.AddSortField("Nome");
			filter.SortDirection = SortDirection.ASC;
			int count = 0;
			AvailablePetitioners = dalPetitioner.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count);
		}

		public void GetPetitionerByText(object sender, DoWorkEventArgs e)
		{
			string text = e.Argument.ToString();
			try
			{
				Richiedente p = dalPetitioner.GetItemByText(text);
				e.Result = p;
			}
			catch (Exception ex)
			{

			}
		}

		public Richiedente GetPetitionerByText(string text)
		{
			return dalPetitioner.GetSingleItem(text);
		}

		public Richiedente GetPrenotationPetinionerSingleItem(int id)
		{
			return dalPetitioner.GetItem(id);
		}

		public LK_Citta GePetitionerCitySingleItem(int cityID)
		{
			return dalCity.GetItem(cityID);
		}

		#endregion// Petitioners




		#region Visit Type

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

		private void LoadAvailableVisitTypes()
		{
			AvailableVisitTypes = new ObservableCollection<LK_TipoVisita>(dalVisitType.GetItems());
		}

		public List<LK_TipoVisita> GetPrenotationVisitTypeSingleItem(int id)
		{
			return dalVisitType.GetSingleItem(id);
		}

		#endregion// Visit Type




		#region Confirmation Type

		private ObservableCollection<LK_TipoConferma> availableConfirmationTypes = null;
		public ObservableCollection<LK_TipoConferma> AvailableConfirmationTypes
		{
			get
			{
				if (availableConfirmationTypes == null)
					availableConfirmationTypes = new ObservableCollection<LK_TipoConferma>();
				return availableConfirmationTypes;
			}
			set { availableConfirmationTypes = value; }
		}

		private void LoadAvailableConfirmationTypes()
		{
			AvailableConfirmationTypes = new ObservableCollection<LK_TipoConferma>(dalConfirmationType.GetItems());
		}

		public List<LK_TipoConferma> GetPrenotationConfirmationTypeSingleItem(int id)
		{
			return dalConfirmationType.GetSingleItem(id);
		}

		#endregion// Visit Type

	}
}
