using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Scv_Dal;
using Scv_Entities;
using System.Collections.ObjectModel;
using Scv_Model;

namespace Presentation
{
    public class ScheduledToursListViewModel : INotifyPropertyChanged
    {
        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion




        #region Variables

        private VisitaProgrammata_Dal dalVisitaProgrammata = new VisitaProgrammata_Dal();
        private Guida_Dal dalGuida = new Guida_Dal();
        private GuidaDisponibile_Dal dalGuideDisponibili = new GuidaDisponibile_Dal();
        private DateTime visitDate = DateTime.Now.Date;
        private BaseFilter filter = null;
        private ObservableCollection<Hour> availablesHours = null;
        public string ora_inizio_visite_am = string.Empty;
        public string ora_ultima_visita = string.Empty;
        private Parametri_Dal dalParametri = new Parametri_Dal();
        private V_EvidenzeGiornaliere selectedItem = null;
        private int totaleInteri = 0;
        private int totaleRidotti = 0;
        private int totaleOmaggio = 0;
        private string totaleBiglietti = string.Empty;

        private int totaleInteriEmessi = 0;
        private int totaleRidottiEmessi = 0;
        private int totaleOmaggioEmessi = 0;
        private string totaleBigliettiEmessi = string.Empty;


        #endregion




        #region Properties

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

        public DateTime VisitDate
        {
            get { return visitDate; }
            set { visitDate = value; OnPropertyChanged(this, "VisitDate"); }
        }

        public BaseFilter Filter
        {
            get
            {
                if (filter == null)
                    filter = new BaseFilter();
                return filter;
            }
            set { filter = value; }
        }

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
                OnPropertyChanged(this, "AvailablesHours");
                availablesHours = value;
            }
        }

        public V_EvidenzeGiornaliere SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; OnPropertyChanged(this, "SelectedItem"); }
        }



        public int TotaleInteri
        {
            get { return totaleInteri; }
            set { totaleInteri = value; OnPropertyChanged(this, "TotaleInteri"); }
        }

        public int TotaleRidotti
        {
            get { return totaleRidotti; }
            set { totaleRidotti = value; OnPropertyChanged(this, "TotaleRidotti"); }
        }

        public int TotaleOmaggio
        {
            get { return totaleOmaggio; }
            set { totaleOmaggio = value; OnPropertyChanged(this, "TotaleOmaggio"); }
        }

        public string TotaleBiglietti
        {
            get { return totaleBiglietti; }
            set { totaleBiglietti = value; OnPropertyChanged(this, "TotaleBiglietti"); }
        }


        public int TotaleInteriEmessi
        {
            get { return totaleInteriEmessi; }
            set { totaleInteriEmessi = value; OnPropertyChanged(this, "TotaleInteriEmessi"); }
        }

        public int TotaleRidottiEmessi
        {
            get { return totaleRidottiEmessi; }
            set { totaleRidottiEmessi = value; OnPropertyChanged(this, "TotaleRidottiEmessi"); }
        }

        public int TotaleOmaggioEmessi
        {
            get { return totaleOmaggioEmessi; }
            set { totaleOmaggioEmessi = value; OnPropertyChanged(this, "TotaleOmaggioEmessi"); }
        }

        public string TotaleBigliettiEmessi
        {
            get { return totaleBigliettiEmessi; }
            set { totaleBigliettiEmessi = value; OnPropertyChanged(this, "TotaleBigliettiEmessi"); }
        }

        private List<LK_TipoRisposta> availableStatusTypes = null;
        public List<LK_TipoRisposta> AvailableStatusTypes
        {
            get
            {
                if (availableStatusTypes == null)
                {
                    LK_TipoRisposta_Dal dal = new LK_TipoRisposta_Dal();
                    availableStatusTypes = dal.GetList(9);
                    LK_TipoRisposta obj = new LK_TipoRisposta();
                    obj.Id_TipoRisposta = 0;
                    obj.Descrizione = "Tutte";
                    availableStatusTypes.Insert(0, obj);
                }
                return availableStatusTypes;
            }
        }


        private bool loading = false;
        public bool Loading
        {
            get { return loading; }
            set { loading = value; }
        }
        private int selectedStatusTypeID = 0;
        public int SelectedStatusTypeID
        {
            get
            {
                return selectedStatusTypeID;
            }
            set { selectedStatusTypeID = value; OnPropertyChanged(this, "SelectedStatusTypeID"); }
        }


        #endregion




        #region Constructors

        public ScheduledToursListViewModel()
        {
            Loading = true;
            ora_inizio_visite_am = dalParametri.GetItem("ora_inizio_visite_am").Valore;
            ora_ultima_visita = dalParametri.GetItem("ora_ultima_visita").Valore;
            LoadAvailableHours();
            Filter.AddSortField("Dt_Visita");
            Filter.SortDirection = SortDirection.DESC;
        }

        #endregion




        #region Events Handling

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null && (!Loading))
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

            switch (propertyName)
            {
                case "VisitDate":
                    if (!Loading || true)
                        LoadMaster(VisitDate);
                    break;
            }
        }

        #endregion// Event Handling




        #region Methods

        private void LoadAvailableHours()
        {
            AvailablesHours = new ObservableCollection<Hour>(Helper_Dal.GetHours(ora_inizio_visite_am, ora_ultima_visita, 15));
        }

        public void LoadMaster()
        {
            //DateTime start = DateTime.Now;
            int count = 0;
            if (Filter.Args.Count > 0)
                SrcEvidenzeGiornaliere = new ObservableCollection<V_EvidenzeGiornaliere>(dalVisitaProgrammata.GetEvidenzeGiornaliere(Filter.Args, Filter.Sort, Filter.SortDirection.ToString(), Filter.PageSize, Filter.PageNumber, out count));
            else
                SrcEvidenzeGiornaliere = new ObservableCollection<V_EvidenzeGiornaliere>(dalVisitaProgrammata.GetEvidenzeGiornaliere());
            /*
			TotaleInteri = 0;
			TotaleRidotti = 0;
			TotaleOmaggio = 0;

			int tot = 0;
			foreach (V_EvidenzeGiornaliere evg in SrcEvidenzeGiornaliere)
			{
				TotaleInteri += evg.Nr_Interi != null ? (int)evg.Nr_Interi : 0;
				TotaleRidotti += evg.Nr_Ridotti != null ? (int)evg.Nr_Ridotti : 0;
				TotaleOmaggio += evg.Nr_Omaggio != null ? (int)evg.Nr_Omaggio : 0;
				tot = TotaleInteri + TotaleRidotti + TotaleOmaggio;
			}

			TotaleBiglietti = "Totale pren.: " + tot.ToString();

			TotaleInteriEmessi = 0;
			TotaleRidottiEmessi = 0;
			TotaleOmaggioEmessi = 0;

			int totEmessi = 0;
			foreach (V_EvidenzeGiornaliere evg in SrcEvidenzeGiornaliere)
			{
				TotaleInteriEmessi += evg.Nr_InteriConsegnati != null ? (int)evg.Nr_InteriConsegnati : 0;
				TotaleRidottiEmessi += evg.Nr_RidottiConsegnati != null ? (int)evg.Nr_RidottiConsegnati : 0;
				TotaleOmaggioEmessi += evg.Nr_OmaggioConsegnati != null ? (int)evg.Nr_OmaggioConsegnati : 0;
				totEmessi = TotaleInteriEmessi + TotaleRidottiEmessi + TotaleOmaggioEmessi;
			}

			TotaleBigliettiEmessi = "Totale emessi: " + totEmessi.ToString();
            */
        }

        public void LoadMaster(DateTime vDate)
        {
            //DateTime start = DateTime.Now;
            SrcEvidenzeGiornaliere = new ObservableCollection<V_EvidenzeGiornaliere>(dalVisitaProgrammata.GetEvidenzeGiornaliere(vDate));

        }

        #endregion
    }
}