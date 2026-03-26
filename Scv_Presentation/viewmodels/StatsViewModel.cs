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
using System.Configuration;

namespace Presentation
{
    public class StatsViewModel : INotifyPropertyChanged
    {
        #region Public Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Variables
        LK_TipoVisita_Dal lK_TipoVisita_Dal = new LK_TipoVisita_Dal();
        Parametri_Dal parametri_Dal = new Parametri_Dal();
        Stats_Dal stats_Dal = new Stats_Dal();
        int numeroDiAnniPrec = 0;
        #endregion

        #region Properties
        private ObservableCollection<BoolStringClass> srcTipiVisita = null;
        public ObservableCollection<BoolStringClass> SrcTipiVisita
        {
            get
            {
                return srcTipiVisita;
            }
            set
            {
                srcTipiVisita = value;
                OnPropertyChanged(this, "SrcTipiVisita");
            }
        }

        private ObservableCollection<BoolStringClass> srcAnni = null;
        public ObservableCollection<BoolStringClass> SrcAnni
        {
            get
            {
                if (srcAnni == null)
                    srcAnni = new ObservableCollection<BoolStringClass>();
                return srcAnni;
            }
            set
            {
                srcAnni = value;
                OnPropertyChanged(this, "SrcAnni");
            }
        }

        private ObservableCollection<BoolStringClass> srcMesi = null;
        public ObservableCollection<BoolStringClass> SrcMesi
        {
            get
            {
                if (srcMesi == null)
                    srcMesi = new ObservableCollection<BoolStringClass>();
                return srcMesi;
            }
            set
            {
                srcMesi = value;
                OnPropertyChanged(this, "SrcMesi");
            }
        }


        private List<V_TipoVisita_TipoBiglietto_Grouped> srcStats = null;
        public List<V_TipoVisita_TipoBiglietto_Grouped> SrcStats
        {
            get
            {
                if (srcStats == null)
                    srcStats = new List<V_TipoVisita_TipoBiglietto_Grouped>();
                return srcStats;
            }
            set
            {
                srcStats = value;
                OnPropertyChanged(this, "SrcStats");
            }
        }
        #endregion

        #region Constructors
        public StatsViewModel()
        {
            LoadAnni();
            LoadMesi();
            LoadTipiVisita();
        }
        #endregion

        #region Methods
        private void LoadTipiVisita()
        {
           var tipiVisita = lK_TipoVisita_Dal.GetItems().OrderBy(tvx => tvx.Ordine).ToList();
           SrcTipiVisita = new ObservableCollection<BoolStringClass>(tipiVisita.Select(tvx => new BoolStringClass()
           {
               TheValue = tvx.Id_TipoVisita,
               TheText = tvx.Descrizione
           }).ToList());
        }

        private void LoadAnni()
        {
            numeroDiAnniPrec = Convert.ToInt32(parametri_Dal.GetItem("prev_years_nr").Valore);
            int currentYear = DateTime.Now.Year;
            int minYear = currentYear - numeroDiAnniPrec;
            List<BoolStringClass> anni = new List<BoolStringClass>();
            while (currentYear >= minYear)
            {
                anni.Add(new BoolStringClass { TheValue = currentYear, TheText = currentYear.ToString() });
                currentYear--;
               
            }

            SrcAnni = new ObservableCollection<BoolStringClass>(anni);
        }

        private void LoadMesi()
        {
            List<BoolStringClass> mesi = new List<BoolStringClass>();
            int mese = 0;
            string meseDescrizione = "";
            for (int i = 1; i <= 12; i++)
            {
                mese = i;
                switch (mese)
                {
                    case 1:
                        meseDescrizione = "gennaio";
                        break;

                    case 2:
                        meseDescrizione = "febbraio";
                        break;

                    case 3:
                        meseDescrizione = "marzo";
                        break;

                    case 4:
                        meseDescrizione = "aprile";
                        break;

                    case 5:
                        meseDescrizione = "maggio";
                        break;

                    case 6:
                        meseDescrizione = "giugno";
                        break;

                    case 7:
                        meseDescrizione = "luglio";
                        break;

                    case 8:
                        meseDescrizione = "agosto";
                        break;

                    case 9:
                        meseDescrizione = "settembre";
                        break;

                    case 10:
                        meseDescrizione = "ottobre";
                        break;

                    case 11:
                        meseDescrizione = "novembre";
                        break;

                    case 12:
                        meseDescrizione = "dicembre";
                        break;
                }

                mesi.Add(new BoolStringClass { TheValue = i, TheText = meseDescrizione.ToString() });
            }

            SrcMesi = new ObservableCollection<BoolStringClass>(mesi);
        }


        public void SetStats()
        {
            List<int> anni = SrcAnni.Where(ax => ax.IsSelected == true).Select(ax => ax.TheValue).ToList();
            List<int> mesi = SrcMesi.Where(ax => ax.IsSelected == true).Select(ax => ax.TheValue).ToList();
            List<int> tipiVisite = this.SrcTipiVisita.Where(ax => ax.IsSelected == true).Select(ax => ax.TheValue).ToList();
            SrcStats = stats_Dal.GetList(anni, mesi, tipiVisite);
        }
        #endregion

        #region Events Handling

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

            //switch (propertyName)
            //{
            //    case "CurrentDate":
            //    case "SelectedNewVisitType":
            //        SrcEvidenzeGiornaliere.Clear();
            //        SrcTickets.Clear();
            //        AddTicketRow();
            //        break;
            //}
        }

        #endregion
    }
}
