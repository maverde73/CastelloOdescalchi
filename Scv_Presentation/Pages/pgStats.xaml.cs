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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Scv_Dal;
using Scv_Entities;
using System.ComponentModel;
using Scv_Model.Common;

namespace Presentation.Pages
{
    /// <summary>
    /// Interaction logic for pgSettingsPersonal.xaml
    /// </summary>
    public partial class pgStats : BaseContentPage, INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion



        #region Private Fields

        private ObservableCollection<ValidationError> validationErrors = null;

        private int _errors = 0;

        StatsViewModel vm = null;

        #endregion// Private fields



        #region Public Properties

        public LK_User SelectedItem
        {
            get { return this.User; }
            set { this.User = value; OnPropertyChanged(this, "SelectedItem"); }
        }


        #endregion // Properties



        #region Main Binding Properties


        #endregion// Main Binding Properties



        #region Constructors

        public pgStats()
            : base()
        {
            this.DataContext = this;
            InitializeComponent();
           

            vm = new StatsViewModel();
            lbxAnni.DataContext = vm;
            lbxMesi.DataContext = vm;
            lbxTipiVisita.DataContext = vm;
        }

        #endregion // Constructors



        #region Event Handling

        protected void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            DoPrint();
        }

        void frm_Closed(object sender, EventArgs e)
        {

        }

        #endregion // Event Handling



        #region Public Methods
        private void DoPrint()
        {
            string windowName = "Statistiche";
            BasePrintPage frm = null;
            frm = new wndPrintMandatoAmministrazione();
            frm.Name = windowName;
            vm.SetStats();
            frm.StatsTipiVisite = "xxxx";
            frm.DsStats = vm.SrcStats;
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //frm.Closed += new EventHandler(frm_Closed);
            frm.ShowDialog();
        }

        #endregion// Public Methods



        #region Private Methods


        #endregion// Private Methods



        #region Overrides

        #endregion// Overrides



        #region Error Handling

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



    }
}