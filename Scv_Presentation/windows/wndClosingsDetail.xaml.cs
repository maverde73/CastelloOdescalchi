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
	/// Interaction logic for wndClosingsDetail.xaml
	/// </summary>
	public partial class wndClosingsDetail : BaseDetailPage, INotifyPropertyChanged
	{
		#region Events

		#endregion 



		#region Private Fields

		private LK_Chiusura_Dal dal = new LK_Chiusura_Dal();

		private ObservableCollection<ValidationError> validationErrors = null;

		private int _errors = 0;

        ClosingsViewModel pVM = null;

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

		CollectionViewSource cvsClosings = null;

		#endregion// Main Binding Properties



		#region Constructors

		public wndClosingsDetail(int detailID)
		        : base(detailID)
		{

			InitializeComponent();

			cvsClosings = (CollectionViewSource)FindResource("cvsClosings");

            pVM = new ClosingsViewModel(detailID);

			cvsClosings.Source = pVM.SrcClosings;

			calendar.DataContext = pVM;

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

			LK_Chiusura_Dal dal = new LK_Chiusura_Dal();

			LK_Chiusura Closings = ((List<LK_Chiusura>)cvsClosings.Source)[0];
			Closings.Anno = (short)(calendar.SelectedDate.Value.Year);
			Closings.Mese = (short)calendar.SelectedDate.Value.Month;
			Closings.Giorno = (short)(calendar.SelectedDate.Value.Day);
			DetailID = dal.InsertOrUpdate(Closings);

            this.Close();
		}
    
		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Eliminare il record? (L'azione non è annullabile)", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				LK_Chiusura g = dal.GetItem(DetailID);
				if (g != null)
					dal.DeleteObject(g);
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