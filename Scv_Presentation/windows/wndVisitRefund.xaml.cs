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
	/// Interaction logic for wndVisitRefund.xaml
	/// </summary>
	public partial class wndVisitRefund : BaseDetailPage, INotifyPropertyChanged
	{
		#region Events

		#endregion 



		#region Private Fields

		private ObservableCollection<ValidationError> validationErrors = null;

		private int _errors = 0;

        RefundViewModel vm = null;
		
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

		public wndVisitRefund(int paymentID)
		        : base()
		{
			
			vm = new RefundViewModel(paymentID);
			this.DataContext = vm;

			InitializeComponent();

			vm.ObjMovement.Id_User = User.Id_User;

            btnSave.Click += new RoutedEventHandler(btnSave_Click);
		}


		#endregion // Constructors



		#region Event Handling

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
            //Eventuale messaggio errore
            string message = string.Empty;

            //Risultato validazione
            if (ValidationErrors.Count > 0)
                foreach (ValidationError err in ValidationErrors)
                    message += "\n" + err.ErrorContent.ToString();

			//Verifica che il rimborso non superi il pagamento
			if (vm.ObjMovement.PrezzoVendita > vm.ObjPaymentAmount)
				message += "\nIl rimborso di euro " + vm.ObjMovement.PrezzoVendita.ToString() + " supera il pagamento di euro " + vm.ObjPaymentAmount.ToString() + ".";

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

			vm.ObjMovement.Id_User = User.Id_User;

			dal.InsertOrUpdateRefund(vm.ObjMovement, vm.ObjPaymentID);

			////Iterazione fra i dettagli non vuoti e assegnazione alle nuove
			////entità da registrare
			//foreach (Movimento m in detailsSource)
			//{
			//mv = new Movimento();
			//mv.Dt_Movimento = (DateTime)dtpMovementDate.SelectedValue.Value.Date;
			//mv.Id_TipoMovimento = int.Parse(cmbMovementType.SelectedValue.ToString());
			//mv.Id_TipoPagamento = cmbPaymentType.SelectedValue != null ? int.Parse(cmbPaymentType.SelectedValue.ToString()) : (int?)null;
			//mv.Id_Guida = cmbGuides.SelectedValue != null ? int.Parse(cmbGuides.SelectedValue.ToString()) : (int?)null;
			//mv.Nota = txtNota.Text;
			//mv.RicevutaBolla = txtRicevutaAnno.Content + "/" + txtRicevutaSimbolo.Content + "/" + txtRicevutaNumero.Content;
			//decimal compenso = 0;
			//decimal.TryParse(txtCompenso.Text, out compenso);
			//mv.PrezzoVendita = compenso;
			//mv.NominativoRimborso = txtNominativo.Text;
			//mv.Id_User = User.Id_User;
			//detailDestination.Add(mv);
			//}

            //Creazione DAL Tipi movimento
			//LK_TipoMovimento_Dal dalTM = new LK_TipoMovimento_Dal();

            //Si ricava il tipo movimento

			//dal.InsertOrUpdate(detailDestination, tm, txtRicevutaAnno.Content.ToString(), txtRicevutaNumero.Content.ToString());

            this.Close();
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