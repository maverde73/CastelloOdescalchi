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
	/// Interaction logic for wndMovementDetail.xaml
	/// </summary>
	public partial class wndMovementDetail : BaseDetailPage, INotifyPropertyChanged
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



		#region Main Binding Properties

		CollectionViewSource cvsMovement = null;
		CollectionViewSource cvsMovementDetails = null;
		CollectionViewSource cvsProgressives = null;
		CollectionViewSource cvsAvailableWares = null;

		#endregion// Main Binding Properties



		#region Constructors

		public wndMovementDetail(int detailID, int movementTypeID, bool lockMovementTypeID, List<int> allowedMovementTypeIDs = null, List<int> allowedPaymentTypeIDs = null)
			: base(detailID)
		{

			InitializeComponent();

			grdStore.Visibility = System.Windows.Visibility.Visible;

			cvsMovement = (CollectionViewSource)FindResource("cvsMovement");
			cvsMovementDetails = (CollectionViewSource)FindResource("cvsMovementDetails");
			cvsProgressives = (CollectionViewSource)FindResource("cvsProgressives");
			cvsAvailableWares = (CollectionViewSource)FindResource("cvsAvailableWares");

			pVM = new MovementViewModel(detailID, movementTypeID, lockMovementTypeID, allowedMovementTypeIDs, allowedPaymentTypeIDs);

			this.Loaded += new RoutedEventHandler(wndMovementDetail_Loaded);

			cvsMovement.Source = pVM.SrcMovement;
			cvsMovementDetails.Source = pVM.SrcMovementDetails;
			cvsProgressives.Source = pVM.SrcProgressives;
			cvsAvailableWares.Source = pVM.AvailableWares;

			cmbMovementType.DataContext = pVM;
			cmbPaymentType.DataContext = pVM;
			cmbStores.DataContext = pVM;
			pnlProgressives.DataContext = pVM;
			grdDetails.DataContext = pVM;

			grdDetails.AddHandler(RadComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnWareSelected));

			btnSave.Click += new RoutedEventHandler(btnSave_Click);
			btnDelete.Click += new RoutedEventHandler(btnDelete_Click);

			if (movementTypeID > 0)
				SetControlsLayout(new LK_TipoMovimento_Dal().GetItem(movementTypeID));
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

			if (((ObservableCollection<Movimento>)cvsMovementDetails.Source).ToList().Where(x => x.IsEmpty == false).Count() == 0)
				message += "\n" + "E' obbligatorio inserire almeno un dettaglio.";

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

			//Calcolo eventuale sconto
			decimal discount = 0;

			//Eliminazione righe dettagli di default (vuote)
			ObservableCollection<Movimento> detailsSource = new ObservableCollection<Movimento>(((ObservableCollection<Movimento>)cvsMovementDetails.Source).ToList().Where(x => x.IsEmpty == false));

			//Creazione dettagli da registrare
			List<Movimento> detailDestination = new List<Movimento>();
			Movimento mv = null;

			//Iterazione fra i dettagli non vuoti e assegnazione alle nuove
			//entità da registrare
			foreach (Movimento m in detailsSource)
			{
				mv = new Movimento();
				mv.Dt_Movimento = (DateTime)dtpMovementDate.SelectedValue.Value.Date;
				mv.Id_TipoMovimento = int.Parse(cmbMovementType.SelectedValue.ToString());
				mv.Id_TipoPagamento = cmbPaymentType.SelectedValue != null ? int.Parse(cmbPaymentType.SelectedValue.ToString()) : (int?)null;
				mv.Id_EsercizioVendita = cmbStores.SelectedValue != null ? int.Parse(cmbStores.SelectedValue.ToString()) : (int?)null;
				mv.Nota = txtNota.Text;
				mv.NotaMovimento = txtNotaMovimento.Text;
				mv.Nr_Pos = txtPos.Text;
				if (
					m.Id_TipoMovimento == 1
					||
					m.Id_TipoMovimento == 2
					||
					m.Id_TipoMovimento == 4
					||
					m.Id_TipoMovimento == 5
					||
					m.Id_TipoMovimento == 6
					)
				{
					mv.PrezzoVendita = 0;
					mv.PrezzoPubblico = 0;
				}
				else
				if (txtSconto.Content != null)
					if (decimal.TryParse(txtSconto.Content.ToString(), out discount))
						mv.PrezzoVendita = m.PrezzoVendita - (m.PrezzoVendita * discount / 100);
				
				mv.RicevutaBolla = m.Id_TipoMovimento >= 3 ? txtRicevutaAnno.Content + "/" + txtRicevutaSimbolo.Content + "/" + txtRicevutaNumero.Content : string.Empty;
				mv.Sconto = discount;
				mv.Id_Articolo = m.Id_Articolo;
				mv.Nr_Pezzi = m.Nr_Pezzi;
				mv.PrezzoVendita = m.PrezzoVendita;
				mv.PrezzoPubblico = m.PrezzoPubblico;
				mv.Id_User = User.Id_User;

				detailDestination.Add(mv);
			}

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
				pVM.RemoveDetailRow(visitID, true);
			}
		}

		private void grdDetails_EditEnded(object sender, GridViewRowEditEndedEventArgs e)
		{
			if (e.EditAction == Telerik.Windows.Controls.GridView.GridViewEditAction.Commit)
			{
				if (
					((Movimento)e.Row.Item).Id_Articolo != null
					&&
					((Movimento)e.Row.Item).Id_Articolo > 0
					&&
					((Movimento)e.Row.Item).Nr_Pezzi != null
					&&
					((Movimento)e.Row.Item).Nr_Pezzi > 0
					)
				{
					((Movimento)e.Row.Item).Id_TipoMovimento = int.Parse(cmbMovementType.SelectedValue.ToString());

					EsistenzaArticolo ea = pVM.GetStorage((int)((Movimento)e.Row.Item).Id_Articolo);
					pVM.SrcMovement[0].EsistenzaMagazzino = ea.EsistenzaMagazzino;
					pVM.SrcMovement[0].EsistenzaUfficio = ea.EsistenzaUfficio;

					LK_TipoMovimento tm = new LK_TipoMovimento_Dal().GetSingleItem(int.Parse(cmbMovementType.SelectedValue.ToString()))[0];

					if (pVM.ArticleAlreadyExists((int)((Movimento)e.Row.Item).Id_Articolo, ((Movimento)e.Row.Item).Id_Movimento))
					{
						Articolo_Dal dal = new Articolo_Dal();
						e.Handled = true;
						MessageBox.Show("Questo movimento contiene già un articolo" + dal.GetItem((int)((Movimento)e.Row.Item).Id_Articolo).Descrizione + ". Modificare la quantità di quello già esistente.");
						pVM.RemoveDetailRow(((Movimento)e.Row.Item).Id_Movimento, true);
					}

					switch (tm.Segno_Maga)
					{
						case "-":
							if (ea.EsistenzaMagazzino < 0)
							{
								MessageBox.Show("Il numero di pezzi impostato per questo articolo supera l'esistenza in magazzino.");
								pVM.RemoveDetailRow(((Movimento)e.Row.Item).Id_Movimento, true);

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
								pVM.RemoveDetailRow(((Movimento)e.Row.Item).Id_Movimento, true);
								//return;
							}
							break;
					}

					if (
						((Movimento)e.Row.Item).Id_TipoMovimento == 3
						||
						((Movimento)e.Row.Item).Id_TipoMovimento == 7
						||
						((Movimento)e.Row.Item).Id_TipoMovimento == 8
						||
						((Movimento)e.Row.Item).Id_TipoMovimento == 9
						||
						((Movimento)e.Row.Item).Id_TipoMovimento == 11
						||
						((Movimento)e.Row.Item).Id_TipoMovimento == 12
						||
						((Movimento)e.Row.Item).Id_TipoMovimento == 14
						)
						((Movimento)e.Row.Item).Totale = (int)((Movimento)e.Row.Item).Nr_Pezzi * (decimal)((Movimento)e.Row.Item).PrezzoVendita;
					else
						((Movimento)e.Row.Item).Totale = 0;

					SetMainTotal();
				}
				else
				{
					pVM.RemoveDetailRow(((Movimento)e.Row.Item).Id_Movimento, false);
				}

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
				pVM.RemoveDetailRow(((Movimento)e.Row.Item).Id_Movimento, false);
				pVM.AddNewDetailRow();
			}
		}

		private void grdDetails_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
		{

			((Movimento)e.Row.Item).Editing = true;
		}

		private void cmbMovimentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SetControlsLayout((LK_TipoMovimento)e.AddedItems[0]);
		}

		private void cmbPaymentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			grdNote.Visibility = pVM.ObjMovement.ValidateAuthorization ? Visibility.Visible : Visibility.Collapsed;

			pVM.ObjMovement.ValidatePosNumber = (pVM.ObjMovement.Id_TipoPagamento == 4);
			pVM.ObjMovement.ValidateAuthorization = (pVM.ObjMovement.Id_TipoMovimento == 4);

			grdPos.Visibility = pVM.ObjMovement.ValidatePosNumber ? Visibility.Visible : Visibility.Collapsed;
		}

		protected void OnWareSelected(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				//Ricavo la cella del combobox
				var cell = (e.OriginalSource as RadComboBox).ParentOfType<GridViewCell>();

				if (cell != null)
				{
					//Ricavo l'item (Movimento) dalla cella
					Movimento m = cell.ParentRow.Item as Movimento;

					if (m != null)
					{
						//Cerco la riga in SrcMovementDetails che contiene il Movimento 
						//relativo al combobox di cui ho cambiato la selezione. Se la trovo
						//ricavo l'articolo corrispondente, ne estraggo il prezzso e lo
						//assegno al Movimento. Questo aggiorna il textblock della colonna del prezzo
						foreach (Movimento sm in pVM.SrcMovementDetails)
						{
							if (sm.Id_Movimento == m.Id_Movimento)
							{
								decimal prezzoPubblico = 0;
								Articolo art = new Articolo_Dal().GetItem(((Articolo)e.AddedItems[0]).Id_Articolo);
								if (art != null)
									prezzoPubblico = art.PrezzoVendita != null ? (decimal)art.PrezzoVendita : 0;
								sm.PrezzoVendita = GetPrice(((Articolo)e.AddedItems[0]).Id_Articolo);
								sm.PrezzoPubblico = prezzoPubblico;
							}
						}
					}
				}

				EsistenzaArticolo ea = pVM.GetStorage(((Articolo)e.AddedItems[0]).Id_Articolo);
				pVM.SrcMovement[0].EsistenzaMagazzino = ea.EsistenzaMagazzino;
				pVM.SrcMovement[0].EsistenzaUfficio = ea.EsistenzaUfficio;
			}
		}

		protected void cmbStores_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
				pVM.SrcMovement[0].Sconto = ((EsercizioVendita)e.AddedItems[0]).Sconto;

			foreach (Movimento m in pVM.SrcMovementDetails.Where(x => x.IsEmpty == false))
				m.PrezzoVendita = GetPrice((int)m.Id_Articolo);

			SetTotalPrices();

		}

		protected void SetTotalPrices()
		{
			foreach (Movimento m in pVM.SrcMovementDetails.Where(x => x.IsEmpty == false))
				m.Totale = (decimal)m.PrezzoVendita * (short)m.Nr_Pezzi;
			SetMainTotal();
		}

		protected void SetMainTotal()
		{
			pVM.SrcMovement[0].TotaleComplessivo = pVM.SrcMovementDetails.Sum(x => x.Totale);
		}

		void wndMovementDetail_Loaded(object sender, RoutedEventArgs e)
		{
			pVM.Loading = false;
		}

		#endregion // Event Handling



		#region Public Methods


		#endregion// Public Methods



		#region Private Methods

		private decimal GetPrice(int articleID)
		{
			Articolo a = new Articolo_Dal().GetSingleItem(articleID)[0];
			decimal discount = 0;
			if (txtSconto.Content != null)
				decimal.TryParse(txtSconto.Content.ToString(), out discount);
			decimal price = (decimal)a.PrezzoVendita - ((decimal)a.PrezzoVendita * discount / 100);

			return price;
		}

		private void SetControlsLayout(LK_TipoMovimento movementType)
		{
			grdStore.Visibility = Visibility.Collapsed;
			grdNote.Visibility = Visibility.Collapsed;
			grdPos.Visibility = Visibility.Collapsed;
			pnlPaymentType.Visibility = Visibility.Collapsed;

			pVM.SrcMovement[0].ValidatePayment = false;
			pVM.SrcMovement[0].ValidateAuthorization = false;
			pVM.SrcMovement[0].ValidatePosNumber = false;
			pVM.SrcMovement[0].ValidateStore = false;

			if (cmbMovementType.SelectedValue != null)
			{
				LK_TipoMovimento m = new LK_TipoMovimento_Dal().GetSingleItem(movementType.Id_TipoMovimento)[0];
				pVM.SrcMovement[0].ValidateStore = m.Fl_Esercizi;
				pVM.SrcMovement[0].ValidateAuthorization = m.Fl_Autorizzazione;
				pVM.SrcMovement[0].ValidatePayment = m.Fl_Pagamento;

				grdStore.Visibility = m.Fl_Esercizi ? Visibility.Visible : Visibility.Collapsed;

				grdNote.Visibility = m.Fl_Autorizzazione ? Visibility.Visible : Visibility.Collapsed;

				pnlPaymentType.Visibility = m.Fl_Pagamento ? Visibility.Visible : Visibility.Collapsed;
			}

			pVM.GetProgressiveByMovementType(movementType.Id_TipoMovimento);

			if (pVM.SrcMovementDetails.Count > 1)
			{
				pVM.SrcMovementDetails.Clear();
				pVM.SrcMovement[0].EsistenzaMagazzino = 0;
				pVM.SrcMovement[0].EsistenzaUfficio = 0;

				pVM.AddNewDetailRow();
			}
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