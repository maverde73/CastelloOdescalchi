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
	/// Interaction logic for wndGuideMovementDetail.xaml
	/// </summary>
	public partial class wndGuideMovementDetail : BaseDetailPage, INotifyPropertyChanged
	{
		#region Events

		#endregion 



		#region Private Fields

		private ObservableCollection<ValidationError> validationErrors = null;

		private int _errors = 0;

        GuideMovementViewModel pVM = null;
		
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

		CollectionViewSource cvsMovement = null;
		CollectionViewSource cvsMovementDetails = null;
		CollectionViewSource cvsProgressives = null;

		#endregion// Main Binding Properties



		#region Constructors

		public wndGuideMovementDetail(int detailID, List<int> allowedPaymentTypeIDs = null)
		        : base(detailID)
		{
			InitializeComponent();

			grdGuide.Visibility = System.Windows.Visibility.Visible;
			grdRefund.Visibility = System.Windows.Visibility.Collapsed;

			cvsMovement = (CollectionViewSource)FindResource("cvsMovement");
			cvsMovementDetails = (CollectionViewSource)FindResource("cvsMovementDetails");
			cvsProgressives = (CollectionViewSource)FindResource("cvsProgressives");

			pVM = new GuideMovementViewModel(detailID, allowedPaymentTypeIDs);

			cvsMovement.Source = pVM.SrcMovement;
			cvsMovementDetails.Source = pVM.SrcMovementDetails;
			cvsProgressives.Source = pVM.SrcProgressives;

			cmbMovementType.DataContext = pVM;
			cmbPaymentType.DataContext = pVM;
			cmbGuides.DataContext = pVM;
			pnlProgressives.DataContext = pVM;

            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnDelete.Click += new RoutedEventHandler(btnDelete_Click);

			pVM.SrcMovement[0].ValidatePayment = true;
			pVM.SrcMovement[0].ValidateAuthorization = true;
			pVM.SrcMovement[0].ValidateStore = false;
			pVM.SrcMovement[0].ValidateGuide = true;
			pVM.SrcMovement[0].ValidatePrice = true;
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
            
            //Eliminazione righe dettagli di default (vuote)
            ObservableCollection<Movimento> detailsSource = new ObservableCollection<Movimento>(((ObservableCollection<Movimento>)cvsMovementDetails.Source).ToList().Where(x => x.IsEmpty == false));
            
            //Creazione dettagli da registrare
            List<Movimento> detailDestination = new List<Movimento>();
            Movimento mv = null;

			////Iterazione fra i dettagli non vuoti e assegnazione alle nuove
			////entità da registrare
			//foreach (Movimento m in detailsSource)
			//{
            mv = new Movimento();
            mv.Dt_Movimento = (DateTime)dtpMovementDate.SelectedValue.Value.Date;
            mv.Id_TipoMovimento = int.Parse(cmbMovementType.SelectedValue.ToString());
            mv.Id_TipoPagamento = cmbPaymentType.SelectedValue != null ? int.Parse(cmbPaymentType.SelectedValue.ToString()) : (int?)null;
			mv.Id_Guida = cmbGuides.SelectedValue != null ? int.Parse(cmbGuides.SelectedValue.ToString()) : (int?)null;
            mv.Nota = txtNota.Text;
            mv.RicevutaBolla = txtRicevutaAnno.Content + "/" + txtRicevutaSimbolo.Content + "/" + txtRicevutaNumero.Content;
			decimal compenso = 0;
			decimal.TryParse(txtCompenso.Text, out compenso);
			mv.PrezzoVendita = compenso;
			mv.NominativoRimborso = txtNominativo.Text;
			mv.Id_User = User.Id_User;
            detailDestination.Add(mv);
			//}

            //Creazione DAL Tipi movimento
            LK_TipoMovimento_Dal dalTM = new LK_TipoMovimento_Dal();

            //Si ricava il tipo movimento
            LK_TipoMovimento tm = dalTM.GetSingleItem(int.Parse(cmbMovementType.SelectedValue.ToString()))[0];

            dal.InsertOrUpdate(detailDestination, tm, txtRicevutaAnno.Content.ToString(), txtRicevutaNumero.Content.ToString());

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
				pVM.RemoveDetailRow(visitID);
			}
		}

		private void grdDetails_EditEnded(object sender, GridViewRowEditEndedEventArgs e)
		{
			if (e.EditAction == Telerik.Windows.Controls.GridView.GridViewEditAction.Commit)
			{
				//if (/*((VisitaPrenotata)e.Row.Item).Nr_Visitatori +*/ pVM.GetVisitorsCount() > nudVisitors.Value)
				//{
				//    e.Handled = true;
				//    MessageBox.Show("Il numero di visitatori impostato per questa visita supera il totale impostato nella prenotazione.");
				//    pVM.RemoveDetailRow(((VisitaPrenotata)e.Row.Item).Id_VisitaPrenotata);
				//    //return;
				//}

				((Movimento)e.Row.Item).Id_TipoMovimento = int.Parse(cmbMovementType.SelectedValue.ToString());

				EsistenzaArticolo ea = pVM.GetStorage((int)((Movimento)e.Row.Item).Id_Articolo);
				pVM.SrcMovement[0].EsistenzaMagazzino = ea.EsistenzaMagazzino;
				pVM.SrcMovement[0].EsistenzaUfficio = ea.EsistenzaUfficio;

				LK_TipoMovimento tm = new LK_TipoMovimento_Dal().GetSingleItem(int.Parse(cmbMovementType.SelectedValue.ToString()))[0];

				switch (tm.Segno_Maga)
				{
					case "-":
						if (ea.EsistenzaMagazzino < 0)
						{
							MessageBox.Show("Il numero di pezzi impostato per questo articolo supera l'esistenza in magazzino.");
							pVM.RemoveDetailRow(((Movimento)e.Row.Item).Id_Movimento);

							//return;
						}
						break;
				}

				switch (tm.Segno_Uff)
				{
					case "-":
						if (ea.EsistenzaUfficio < 0)
						{
							MessageBox.Show("Il numero di pezzi impostato per questo articolo supera l'esistenza in ufficio.");
							pVM.RemoveDetailRow(((Movimento)e.Row.Item).Id_Movimento);
							//return;
						}
						break;
				}

				((Movimento)e.Row.Item).Totale = (int)((Movimento)e.Row.Item).Nr_Pezzi * (int)((Movimento)e.Row.Item).PrezzoVendita;

				SetMainTotal();

				if (((Movimento)e.Row.Item).IsEmpty)
				{
					((Movimento)e.Row.Item).IsEmpty = false;
					((Movimento)e.Row.Item).IsErasable = true;
					pVM.SrcMovement.ToList().Where(x => x.IsEmpty).ToList().ForEach(y => y.IsEmpty = false);
					pVM.SrcMovement.ToList().Where(x => !x.IsEmpty).ToList().ForEach(y => y.IsErasable = true);
					pVM.AddNewDetailRow();
				}

				((Movimento)e.Row.Item).Editing = false;
			}
			else
			{
				//grdVisits.CancelEdit();
			}
		}

		private void grdDetails_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
		{

			((Movimento)e.Row.Item).Editing = true;
		}

		private void cmbMovimentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

			if (cmbMovementType.SelectedValue != null)
			{
				grdGuide.Visibility = Visibility.Collapsed;
				grdRefund.Visibility = Visibility.Collapsed;
				grdNote.Visibility = Visibility.Collapsed;
				pnlPaymentType.Visibility = Visibility.Collapsed;

				pVM.SrcMovement[0].ValidateGuide = false;
				pVM.SrcMovement[0].ValidateRefundName = false;
				pVM.SrcMovement[0].ValidateAuthorization = false;
				pVM.SrcMovement[0].ValidateStore = false;
				pVM.SrcMovement[0].ValidatePrice = false;

				LK_TipoMovimento m = new LK_TipoMovimento_Dal().GetSingleItem((((LK_TipoMovimento)e.AddedItems[0]).Id_TipoMovimento))[0];
				pVM.SrcMovement[0].ValidateGuide = !m.Fl_Rimborso;
				pVM.SrcMovement[0].ValidateRefundName = m.Fl_Rimborso;
				pVM.SrcMovement[0].ValidateAuthorization = m.Fl_Autorizzazione;
				pVM.SrcMovement[0].ValidatePayment = m.Fl_Pagamento;
				pVM.SrcMovement[0].ValidatePrice = m.Fl_Rimborso || m.Fl_Pagamento;

				grdGuide.Visibility = !m.Fl_Rimborso ? Visibility.Visible : Visibility.Collapsed;
				grdRefund.Visibility = m.Fl_Rimborso ? Visibility.Visible : Visibility.Collapsed;
				grdNote.Visibility = m.Fl_Autorizzazione || m.Fl_Rimborso ? Visibility.Visible : Visibility.Collapsed;
				pnlPaymentType.Visibility = m.Fl_Pagamento ? Visibility.Visible : Visibility.Collapsed;
			}			

			pVM.GetProgressiveByMovementType(((LK_TipoMovimento)e.AddedItems[0]).Id_TipoMovimento);
		}

		private void cmbPaymentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			pVM.ObjMovement.ValidateAuthorization = (pVM.ObjMovement.Id_TipoPagamento != 3 && pVM.ObjMovement.Id_TipoPagamento != 0);
			grdNote.Visibility = pVM.ObjMovement.ValidateAuthorization || pVM.ObjMovement.Id_TipoMovimento == 14 ? Visibility.Visible : Visibility.Collapsed;
		}

		protected void cmbGuides_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//pVM.GetProgressiveByMovementType(((LK_TipoMovimento)e.AddedItems[0]).Id_TipoMovimento);

			if (e.AddedItems.Count > 0)
				txtCompensoOra.Content = ((Guida)e.AddedItems[0]).Compenso.ToString("c");

			SetMainTotal();
		}

		protected void SetMainTotal()
		{
			pVM.SrcMovement[0].TotaleComplessivo = pVM.SrcMovement[0].PrezzoVendita != null ? (decimal)pVM.SrcMovement[0].PrezzoVendita : 0;
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