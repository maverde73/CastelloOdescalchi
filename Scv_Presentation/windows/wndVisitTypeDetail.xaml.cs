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
using System.ComponentModel;
using System.Collections.ObjectModel;
using Scv_Dal;
using Scv_Entities;
using ShiftManager;
using System.Data.Objects.DataClasses;

namespace Presentation
{
    /// <summary>
    /// Interaction logic for wndVisitTypeDetail.xaml
    /// </summary>
    public partial class wndVisitTypeDetail : BaseDetailPage, INotifyPropertyChanged
    {
        #region Events

        #endregion



        #region Private Fields

        private ObservableCollection<ValidationError> validationErrors = null;

        private int _errors = 0;

        TipoVisitaViewModel vtVM = null;

        LK_TipoVisita_Dal dal = new LK_TipoVisita_Dal();

        #endregion// Private fields



        #region Public Properties

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

        #endregion // Properties






        #region Constructors

        public wndVisitTypeDetail(int detailID)
            : base(detailID)
        {
            InitializeComponent();
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnDelete.Click += new RoutedEventHandler(btnDelete_Click);

            DetailID = detailID;

            if (DetailID == 0)
                btnDelete.Visibility = System.Windows.Visibility.Hidden;
            
            vtVM = new TipoVisitaViewModel(detailID);
            grdTicketTypes.DataContext = vtVM;
            grdTicketTypes.ItemsSource = vtVM.SrcTicketTypes;
            txtNome.DataContext = vtVM;
            txtSimbolo.DataContext = vtVM;
            nudOrdine.DataContext = vtVM;
        }


        #endregion // Constructors
        #region Event Handling

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string message = string.Empty;

            if (ValidationErrors.Count > 0)
                foreach (ValidationError err in ValidationErrors)
                    message += "\n" + err.ErrorContent.ToString();

            if (message.Length > 0)
            {
                message = "Impossibile continuare a causa dei seguenti errori:" + message;
                MessageBox.Show(message, "Errori", MessageBoxButton.OK);
                return;
            }

            SetTicketTypeInfo();

            DetailID = dal.InsertOrUpdate(vtVM.ObjTipoVisita);

            this.Close();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Eliminare il record? (L'azione non è annullabile)", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                try
                {
                    if (dal.CheckIfCanDelete(DetailID))
                    {
                        dal.DeleteItem(DetailID);
                        this.Close();
                    }
                    else
                        MessageBox.Show("Eliminazione non consentita. L'elemento è già stato associato.", "Attenzione", MessageBoxButton.OK);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Errore eliminazione", MessageBoxButton.OK);
                }
               
            }
        }

        #endregion // Event Handling



        #region Public Methods
        private void SetTicketTypeInfo()
        {
            if (vtVM.SrcTicketTypes.Count > 0)
            {
               TipoVisitaViewModel.TipoBiglietto tt =  vtVM.SrcTicketTypes.FirstOrDefault(tx => tx.Tipo == "Intero");
               vtVM.ObjTipoVisita.Interi = tt.Attivo;
               vtVM.ObjTipoVisita.PrezzoIntero = tt.Prezzo;

               tt = vtVM.SrcTicketTypes.FirstOrDefault(tx => tx.Tipo == "Ridotto");
               vtVM.ObjTipoVisita.Ridotti = tt.Attivo;
               vtVM.ObjTipoVisita.PrezzoRidotto = tt.Prezzo;

               tt = vtVM.SrcTicketTypes.FirstOrDefault(tx => tx.Tipo == "Scontato");
               vtVM.ObjTipoVisita.Scontati = tt.Attivo;
               vtVM.ObjTipoVisita.PrezzoScontato = tt.Prezzo;

               tt = vtVM.SrcTicketTypes.FirstOrDefault(tx => tx.Tipo == "Omaggio");
               vtVM.ObjTipoVisita.Omaggio = tt.Attivo;

               tt = vtVM.SrcTicketTypes.FirstOrDefault(tx => tx.Tipo == "Cumulativo");
               vtVM.ObjTipoVisita.Cumulativo = tt.Attivo;
               vtVM.ObjTipoVisita.PrezzoCumulativo = tt.Prezzo;

            }
        }


        #endregion// Public Methods



        #region Private Methods

        #endregion// Private Methods



        #region Overrides

        protected override void SetLayout()
        {
        }

        #endregion// Overrides



        #region Error Handling

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