using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using Scv_Entities;
using Scv_Dal;
using System.Collections.ObjectModel;
using Scv_Model;

namespace Presentation
{
	public class GuideViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events


		#region Properties

		bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());

		public Guida_Dal dalGuide = new Guida_Dal();
		public LK_Lingua_Dal dalLanguage = new LK_Lingua_Dal();
		public LK_Collegio_Dal dalCollege = new LK_Collegio_Dal();
		public LK_Titolo_Dal dalTitle = new LK_Titolo_Dal();
		public LK_Chiusura_Dal dalCLosing = new LK_Chiusura_Dal();
		public LK_Lingua_Dal dalLanguages = new LK_Lingua_Dal();
		public GuidaLingua_Dal dalGuideLanguages = new GuidaLingua_Dal();

		private List<Guida> srcGuide = null;
		public List<Guida> SrcGuide
		{
            get 
			{
				if (srcGuide == null)
					srcGuide = new List<Guida>() { new Guida() };					
				return srcGuide; 			
			}
			//set { srcGuide = value; OnPropertyChanged(this, "SrcGuide"); }
		}

		private List<V_GuidaLingua> srcGuideLanguages = null;
		public List<V_GuidaLingua> SrcGuideLanguages
		{
			get
			{
				if(srcGuideLanguages == null)
					srcGuideLanguages = new List<V_GuidaLingua>();
				return srcGuideLanguages;
			}
			set { srcGuideLanguages = value; }
		}

		private Guida objGuide = null;
		public Guida ObjGuide
		{
			get
			{
				if (objGuide == null)
					objGuide = new Guida();
				return objGuide;
			}
			set { objGuide = value; }
		}

		#endregion // Properties


		#region Constructors

		public GuideViewModel(int detailID)
		{
			LoadAvailableTitles();
			LoadAvailableLanguages();
			LoadAvailableColleges();

			if (detailID > 0)
			{
				srcGuide = dalGuide.GetSingleItem(detailID);
				SrcGuideLanguages = dalGuideLanguages.GetItemsByGuideID(detailID);
			}
			else
			{
				Guida gd = new Guida();
				gd.Id_Titolo = 0;
				gd.Id_Collegio = null;
				srcGuide = new List<Guida>() {gd };
				SrcGuideLanguages = new List<V_GuidaLingua>();
			}

			foreach (V_GuidaLingua gl in SrcGuideLanguages)
				foreach (LK_Lingua l in AvailableLanguages)
					if (gl.Id_Lingua == l.Id_Lingua)
					{
						l.IsSelected = true;
						l.IsDefault = gl.Fl_Madre != null ? (bool)gl.Fl_Madre : false;
					}

			ObjGuide = SrcGuide[0];
		}

		#endregion// Constructors


		#region Event Handling

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handling


		#region Main Methods

        public List<Guida> GetGuideSingleItem(int id)
        {
            return dalGuide.GetSingleItem(id);
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
			filter.SortDirection = SortDirection.ASC;
			int count = 0;
			AvailableTitles = dalTitle.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count);
        }
        
        #endregion// Title


        #region College

        private ObservableCollection<LK_Collegio> availableColleges = null;
        public ObservableCollection<LK_Collegio> AvailableColleges
        {
            get
            {
                if (availableColleges == null)
                    availableColleges = new ObservableCollection<LK_Collegio>();
                return availableColleges;
            }
            set { availableColleges = value; }
        }

        private void LoadAvailableColleges()
        {
			BaseFilter filter = new BaseFilter();
			filter.AddSortField("Descrizione");
			filter.SortDirection = SortDirection.ASC;
			int count = 0;
			AvailableColleges = dalCollege.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count);
			LK_Collegio c = new LK_Collegio();
			c.Descrizione = string.Empty;
			c.Id_Collegio = 0;
			availableColleges.Insert(0, c);
        }

        #endregion// College

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

		private void LoadAvailableLanguages()
		{
			AvailableLanguages = new ObservableCollection<LK_Lingua>(dalLanguages.GetList());
		}

		#endregion// College

	}
}
