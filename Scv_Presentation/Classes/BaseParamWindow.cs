using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Scv_Model;
using System.Data.Objects.DataClasses;
using System.ComponentModel;
using Telerik.Windows.Controls;
using Scv_Entities;
using Scv_Dal;

namespace Presentation
{
	public class BaseParamWindow : Window, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion //Events


        #region Private Fields

		private BaseFilter filter = null;

        private List<YearItem> years = null;

		private List<MonthItem> months = null;

		private YearItem selectedYear = null;

		private MonthItem selectedMonth = null;

		private List<EsercizioVendita> stores = null;

		private int selectedStoreID = 0;

        #endregion// Private Fields




		#region Public Properties

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

        public List<YearItem> Years
        {
            get
            {
                if (years == null)
                    years = Helper_Dal.GetYears(2000, DateTime.Now.Year);
                return years;
            }
        }

        public List<MonthItem> Months
        {
            get
            {
                if (months == null)
                    months = Helper_Dal.GetMonths();
                return months;
            }
        }

        public YearItem SelectedYear
        {
            get
            {
                if (selectedYear == null)
                    selectedYear = new YearItem();
                return selectedYear;
            }
            set { selectedYear = value; OnPropertyChanged(this, "SelectedYear"); }
        }

        public MonthItem SelectedMonth
        {
            get
            {
                if (selectedMonth == null)
                    selectedMonth = new MonthItem();
                return selectedMonth;
            }
            set { selectedMonth = value; OnPropertyChanged(this, "SelectedMonth"); }
        }

		public List<EsercizioVendita> Stores
		{
			get
			{
				if (stores == null)
					stores = new List<EsercizioVendita>();
				return stores;
			}
			set { stores = value; OnPropertyChanged(this, "Stores"); }
		}

		public int SelectedStoreID
		{
			get { return selectedStoreID; }
			set { selectedStoreID = value; OnPropertyChanged(this, "SelectedStoreID"); }
		}

		#endregion //Public Properties


		#region Constructors

		public BaseParamWindow()
        {
        }

		#endregion // Constructors


		#region Event handlers

		protected void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion // Event handlers


	}
}
