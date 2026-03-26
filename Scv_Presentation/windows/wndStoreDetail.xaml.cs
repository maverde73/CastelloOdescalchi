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
	/// Interaction logic for wndStoreDetail.xaml
	/// </summary>
	public partial class wndStoreDetail : BaseDetailPage, INotifyPropertyChanged
	{
		#region Events

		#endregion 



		#region Private Fields

		private ObservableCollection<ValidationError> validationErrors = null;

		private int _errors = 0;

        StoreViewModel pVM = null;

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



		#region Main Binding Properties

		CollectionViewSource cvsStore = null;

		#endregion// Main Binding Properties



		#region Constructors

		public wndStoreDetail(int detailID)
		        : base(detailID)
		{

			InitializeComponent();

			cvsStore = (CollectionViewSource)FindResource("cvsStore");

            pVM = new StoreViewModel(detailID);
						
			cvsStore.Source = pVM.SrcStore;

            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
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

			EsercizioVendita_Dal dal = new EsercizioVendita_Dal();

			EsercizioVendita Store = ((List<EsercizioVendita>)cvsStore.Source)[0];
			Store.Id_User = User.Id_User;

			DetailID = dal.InsertOrUpdate(Store);

            this.Close();
		}
    
		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Eliminare il record? (L'azione non è annullabile)", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				EsercizioVendita_Dal dal = new EsercizioVendita_Dal();

				EsercizioVendita Store = ((List<EsercizioVendita>)cvsStore.Source)[0];

				dal.DeleteObject(Store);

				this.Close();
			}
		}

		#endregion // Event Handling



		#region Public Methods


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