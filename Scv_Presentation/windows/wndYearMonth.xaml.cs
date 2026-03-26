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
using Scv_Model;
using Scv_Dal;
using System.ComponentModel;
using Scv_Entities;

namespace Presentation
{
    /// <summary>
    /// Interaction logic for wndYearMonth.xaml
    /// </summary>
    public partial class wndYearMonth : BaseParamWindow
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events



		#region Variables


		#endregion



		#region Public Properties



		#endregion// Public Properties



		#region Constructors

		public wndYearMonth()
		{
			DataContext = this;
			InitializeComponent();

            btnOK.Click += new RoutedEventHandler(btnOK_Click);

			PropertyChanged += new PropertyChangedEventHandler(BasePropertyChanged);

			SelectedYear.Number = DateTime.Now.Year;
			SelectedMonth.Number = DateTime.Now.Month;

			BindStores();

			SelectedStoreID = 0;

			cmbYear.DataContext = this;
			cmbMonth.DataContext = this;
			cmbStore.DataContext = this;
		}

		#endregion// Constructors



        #region Events

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
			this.DialogResult = true;
            this.Close();
        }

		void SelectedYear_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(sender, "SelectedYear");
		}

		void SelectedMonth_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(sender, "SelectedMonth");
		}

		void BasePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(sender, e.PropertyName);
		}

        #endregion// Events



        #region Private Methods

		private void BindStores()
		{
			EsercizioVendita_Dal dal = new EsercizioVendita_Dal();
			int count = 0;
			Stores = dal.GetFilteredList(Filter.Args, Filter.Sort, Filter.SortDirection.ToString(), Filter.PageSize, Filter.PageNumber, out count);
			EsercizioVendita obj = new EsercizioVendita();
			obj.Descrizione = "Tutti";
			obj.Id_EsercizioVendita = 0;
			Stores.Insert(0, obj);
		}

        #endregion// Private Methods



        #region Event Handlers

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        #endregion// Event Handlers
    }
}
