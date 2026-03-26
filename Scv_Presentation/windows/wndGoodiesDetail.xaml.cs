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
using System.Data;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for wndGoodiesDetail.xaml
	/// </summary>
	public partial class wndGoodiesDetail : BaseDetailPage, INotifyPropertyChanged
	{
		#region Events

		#endregion 



		#region Private Fields

		private ObservableCollection<ValidationError> validationErrors = null;

		private List<ShiftItem> days = null;

		private List<DisabledShiftItem> closings = null;

		private int _errors = 0;

        GoodiesViewModel pVM = null;

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

		CollectionViewSource cvsGoodie = null;

		#endregion// Main Binding Properties



		#region Constructors

		public wndGoodiesDetail(int detailID)
		        : base(detailID)
		{

			InitializeComponent();

			cvsGoodie = (CollectionViewSource)FindResource("cvsGoodie");

            pVM = new GoodiesViewModel(detailID);
				
			cvsGoodie.Source = pVM.SrcGoodies;

			cmbGoodieType.DataContext = pVM;

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

            Articolo_Dal dal = new Articolo_Dal();

            Articolo Goodie = ((List<Articolo>)cvsGoodie.Source)[0];

			DetailID = dal.InsertOrUpdate(Goodie);

            this.Close();
		}
    
		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Eliminare il record? (L'azione non è annullabile)", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				Articolo_Dal dal = new Articolo_Dal();

				Articolo Goodie = ((List<Articolo>)cvsGoodie.Source)[0];

				try
				{
					dal.DeleteObject(Goodie);
				}
				catch (Exception ex)
				{
					if (ex.GetType() == typeof(UpdateException))
					{
						MessageBox.Show("Impossibile eliminare l'articolo perchè risulta incluso in uno o più movimenti di magazzino.\nEliminare prima i suddetti movimenti, quindi riprovare.", "Errore eliminazione", MessageBoxButton.OK);
					}
				}

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
            //btnSave.Visibility = DetailID == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            //btnUpdate.Visibility = DetailID > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            //btnDelete.Visibility = DetailID > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
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