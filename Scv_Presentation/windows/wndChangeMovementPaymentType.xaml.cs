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
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for wndChangeMovementPaymentType.xaml
	/// </summary>
	public partial class wndChangeMovementPaymentType : BaseDetailPage, INotifyPropertyChanged
	{
		#region Events

		#endregion



		#region Private Fields

		private ObservableCollection<ValidationError> validationErrors = null;

		private int _errors = 0;

		MovementViewModel pVM = null;

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

		public wndChangeMovementPaymentType(int detailID, int movementTypeID, List<int> allowedPaymentTypeIDs = null)
			: base(detailID)
		{

			InitializeComponent();

			pVM = new MovementViewModel(detailID, movementTypeID, false, null, allowedPaymentTypeIDs);

			this.DataContext = pVM;

			btnSave.Click += new RoutedEventHandler(btnSave_Click);

			SetControlsLayout();
		}


		#endregion // Constructors



		#region Event Handling

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			if (pVM.ObjMovement.Dt_Movimento.Date != DateTime.Now.Date)
			{
				{
					MessageBox.Show("Impossibile modificare un pagamento di un movimento non eseguito oggi.", "Errore modifca pagamento");
					return;
				}
			}

			//Eventuale messaggio errore
			string message = string.Empty;

			//Risultato validazione
			if (ValidationErrors.Count > 0)
				foreach (ValidationError err in ValidationErrors)
					message += "\n" + err.ErrorContent.ToString();

			//Se il messaggio di errore non è vuoto
			//notifica all'utente e uscita dal metodo
			if (message.Length > 0)
			{
				message = "Impossibile continuare a causa dei seguenti errori:" + message;
				MessageBox.Show(message, "Errori", MessageBoxButton.OK);
				return;
			}

			//Creazione DAL
			Movimento_Dal dal = new Movimento_Dal();

			dal.ChangeMovementPaymentType((int)pVM.ObjMovement.Identificativo, (int)pVM.ObjMovement.Id_TipoPagamento, pVM.ObjMovement.Nr_Pos);

			this.Close();
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Eliminare il record? (L'azione non è annullabile)", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				this.Close();
			}
		}

		private void DeleteDetail(object sender, RoutedEventArgs e)
		{
			RadButton btn = sender as RadButton;
			if (btn != null)
			{
				int visitID = 0;
				int.TryParse(btn.CommandParameter.ToString(), out visitID);
				pVM.RemoveDetailRow(visitID, true);
			}
		}

		private void cmbPaymentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			pVM.ObjMovement.Id_TipoPagamento = ((LK_TipoPagamento)e.AddedItems[0]).Id_TipoPagamento;
			pVM.ObjMovement.ValidatePosNumber = (pVM.ObjMovement.Id_TipoPagamento == 4);
			pVM.ObjMovement.ValidateAuthorization = (pVM.ObjMovement.Id_TipoMovimento == 4);

			if(pVM.ObjMovement.Id_TipoPagamento != 4)
				pVM.ObjMovement.Nr_Pos = string.Empty;

			SetControlsLayout();
		}

		#endregion // Event Handling



		#region Public Methods


		#endregion// Public Methods



		#region Private Methods


		private void SetControlsLayout()
		{
			pnlNota.Visibility = pVM.ObjMovement.ValidateAuthorization ? Visibility.Visible : Visibility.Collapsed;
			pnlPos.Visibility = pVM.ObjMovement.ValidatePosNumber ? Visibility.Visible : Visibility.Collapsed;
		}

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