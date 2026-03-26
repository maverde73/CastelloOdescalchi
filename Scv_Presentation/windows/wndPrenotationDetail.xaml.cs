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
using System.Data.Objects.DataClasses;
using Scv_Model;
using Scv_Dal;
using Scv_Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Telerik.Windows.Controls;
using Thera.Biglietteria.Boca;
using System.IO;
using System.Threading;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for wndPrenotationDetail.xaml
	/// </summary>
	/// 
	public partial class wndPrenotationDetail : BaseDetailPage, INotifyPropertyChanged
	{

		#region Events


		#endregion



		#region Private Fields

		private ObservableCollection<ValidationError> validationErrors = null;

		private int _errors = 0;

		PrenotationViewModel pVM = null;

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

		public wndPrenotationDetail(int detailID)
			: base(detailID)
		{
			LoadingFromDb = DetailID > 0 ? true : false;

			InitializeComponent();
			pVM = new PrenotationViewModel(detailID);
			this.DataContext = pVM;

			//L'impostazione del SearchText serve per superare un bug nell'attuale
			//versione Telerik, che impedisce di cancellare un SearchText se non impostato esplicitamente. In questo caso,
			//Impostanto i SelectedItems dei vari RadAutoCompleteBox, nella Text Area di ognuno appare il valore caricato
			//ma in realta il SearchText è nullo, anche se si vede il valore. Senza questo workaround non si cancella il
			//testo se si pone a null il rispettivo SelectedItem
			SetSearchLookupTexts();

			dtpVisitDate1.SelectableDateStart = DateTime.Now.Date;
			dtpVisitDate2.SelectableDateStart = DateTime.Now.Date;

			btnSave.Click += new RoutedEventHandler(btnSave_Click);
			txtEmail.TextChanged += new TextChangedEventHandler(txtEmail_TextChanged);
			dtpVisitDate1.SelectionChanged += new SelectionChangedEventHandler(dtpVisitDate1_SelectionChanged);
			dtpVisitDate2.SelectionChanged += new SelectionChangedEventHandler(dtpVisitDate2_SelectionChanged);
			chkNewPetitioner.Click += new RoutedEventHandler(chkNewPetitioner_Click);
			cmbPetitioner.SelectionChanged += new SelectionChangedEventHandler(cmbPetitioner_SelectionChanged);
			cmbPetitioner.SearchTextChanged += new EventHandler(cmbPetitioner_SearchTextChanged);
			cmbPetitioner.SearchText = "";

			SetPetitionerTabs(true);

			Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}

		#endregion // Constructors



		#region Event Handling

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			string message = string.Empty;

			if (ValidationErrors.Count > 0)
				foreach (ValidationError err in ValidationErrors)
					message += "\n" + err.ErrorContent.ToString();

			if (
				txtEmail.Text.Length == 0
				&&
				txtTelCellulare.Text.Length == 0
				&&
				txtTelefonoCasa.Text.Length == 0
				&&
				txtTelUfficio.Text.Length == 0
				)
				message += "\nE' obbligatorio inserire l'email oppure un telefono.";

			if (message.Length > 0)
			{
				message = "Impossibile continuare con il salvataggio:" + message;
				MessageBox.Show(message, "Attenzione", MessageBoxButton.OK);
				return;
			}

			Prenotazione_Dal prnDal = new Prenotazione_Dal();

			string[] resp = pVM.ObjPrenotation.Responsabile.Split(' ');
			pVM.ObjPrenotation.Responsabile = string.Empty;
			for (int i = 0; i < resp.Length; i++)
			{
				if (pVM.ObjPrenotation.Responsabile.Length > 0)
					pVM.ObjPrenotation.Responsabile += " ";
				pVM.ObjPrenotation.Responsabile += Helper_Dal.UpperCaseWords(resp[i]);
			}

            string nome = string.IsNullOrEmpty(pVM.ObjPetitioner.Nome) ? "" : pVM.ObjPetitioner.Nome;

            pVM.ObjPetitioner.Nome = Helper_Dal.UpperCaseWords(nome);
			pVM.ObjPetitioner.Cognome = Helper_Dal.UpperCaseWords(pVM.ObjPetitioner.Cognome);

            if (cmbSesso.SelectedValue != null && cmbSesso.IsEnabled)
                pVM.ObjPetitioner.Sesso = cmbSesso.SelectedValue.ToString();
            else
                pVM.ObjPetitioner.Sesso = null;
               
            LK_Titolo petitionerTitle = pVM.ObjPetitionerTitle;
            //if (petitionerTitle == null)
            //{
            //    if (cmbPetitionerTitle.SearchText.Length > 0)
            //    {
            //        petitionerTitle = new LK_Titolo();
            //        petitionerTitle.Sigla = cmbPetitionerTitle.SearchText.Length > 15 ? cmbPetitionerTitle.SearchText.Substring(0, 15) : cmbPetitionerTitle.SearchText;
            //    }
            //}

			LK_Organizzazione petitionerOrganization = pVM.ObjPetitionerOrganization;
			if (petitionerOrganization == null)
			{
				if (cmbPetitionerOrganization.SearchText.Length > 0)
				{
					petitionerOrganization = new LK_Organizzazione();
					petitionerOrganization.Descrizione = cmbPetitionerOrganization.SearchText.Length > 30 ? cmbPetitionerOrganization.SearchText.Substring(0, 30) : cmbPetitionerOrganization.SearchText;
				}
			}

			LK_Citta petitionerCity = pVM.ObjPetitionerCity;
			if (petitionerCity  == null)
			{
				if (cmbPetitionerCity.SearchText.Length > 0)
				{
					petitionerCity = new LK_Citta();
					petitionerCity.Nome = cmbPetitionerCity.SearchText.Length > 30 ? cmbPetitionerCity.SearchText.Substring(0, 30) : cmbPetitionerCity.SearchText;
					petitionerCity.CAP = txtCap.Text.Length > 5 ? txtCap.Text.Substring(0, 5) : txtCap.Text;
					petitionerCity.Nazione = txtNazione.Text.Length > 30 ? txtNazione.Text.Substring(0, 30) : txtNazione.Text;
					petitionerCity.Provincia = txtProvince.Text.Length > 2 ? txtProvince.Text.Substring(0, 2) : txtProvince.Text;
				}
			}

            int id = 0;

            //if (pVM.ObjPrenotation.Id_Prenotazione == 0)
            //{
            //    VisitaPrenotata v = new VisitaPrenotata();
            //    v.Id_Lingua = pVM.ObjPrenotation.Id_LinguaRisposta;
            //    v.Nr_Visitatori = pVM.ObjPrenotation.Tot_Visitatori;
            //    pVM.SrcPrenotationVisits.Add(v);
            //}

            if (!prnDal.InsertOrUpdate(User.Id_User, pVM.ObjPrenotation, pVM.ObjPetitioner, petitionerTitle, petitionerOrganization, petitionerCity, pVM.SrcPrenotationVisits.ToList(), out id))
                MessageBox.Show("Si è verificato un errore durante il salvataggio. Nessuna modifica è stata apportata. \nPremere nuovamente 'Salva' per apportare le modifiche.", "Errore salvataggio");
            else
            {
                SavedObj = pVM.GetV_Prenotazione(id);
                OnDetailWindowClosing(new ClosingDetailWindowEventArgs(id, DetailID == 0 ? true : true));
                this.Close();
            }
		}

		private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (txtEmail.Text.Length > 0)
				SetPetitionerTabs(false);
			else
				SetPetitionerTabs(true);
		}

		private void dtpVisitDate1_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0 && dtpVisitDate2.SelectedDate != null)
				if (dtpVisitDate2.SelectedDate < (DateTime)e.AddedItems[0])
					dtpVisitDate2.SelectedDate = (DateTime)e.AddedItems[0];
		}

		private void dtpVisitDate2_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0 && dtpVisitDate1.SelectedDate != null)
				if ((DateTime)e.AddedItems[0] < dtpVisitDate1.SelectedDate)
					dtpVisitDate1.SelectedDate = (DateTime)e.AddedItems[0];
		}

		private void chkNewPetitioner_Click(object sender, RoutedEventArgs e)
		{
			CheckBox obj = sender as CheckBox;
			if (obj != null)
			{
				if ((bool)obj.IsChecked)
					ClearPetitionerForm(false);
			}
		}

		private void PetitionerChanghed(object sender, EventArgs e)
		{
			chkNewPetitioner.IsEnabled = true;
			if (!LoadingFromDb)
				pVM.ObjPrenotation.Responsabile = Helper_Dal.UpperCaseWords(txtCognome.Text) + " " + Helper_Dal.UpperCaseWords(txtNome.Text);

			if (txtEmail.Text.Length > 0)
			{
				SetPetitionerTabs(false);
				pVM.ObjPrenotation.Id_TipoConferma = 1;
			}
			else
			{
				SetPetitionerTabs(true);
				if (txtTelefonoCasa.Text.Length == 0 && txtTelUfficio.Text.Length == 0 && txtTelCellulare.Text.Length == 0)
					pVM.ObjPrenotation.Id_TipoConferma = 4;
				if (txtTelUfficio.Text.Length > 0 && txtTelefonoCasa.Text.Length == 0 && txtTelCellulare.Text.Length == 0)
					pVM.ObjPrenotation.Id_TipoConferma = 3;
				if (txtTelefonoCasa.Text.Length > 0 && txtTelUfficio.Text.Length == 0 && txtTelCellulare.Text.Length == 0)
					pVM.ObjPrenotation.Id_TipoConferma = 2;
			}
		}

		#endregion // Event Handling



		#region Private Methods

		private void SetPetitionerTabs(bool mode)
		{
			txtIndirizzo.IsTabStop = mode;
			txtTelCellulare.IsTabStop = mode;
			txtTelefonoCasa.IsTabStop = mode;
			txtTelUfficio.IsTabStop = mode;

            if (pVM.ObjPetitioner.Sesso != string.Empty)
            {
                cmbSesso.SelectedValue = pVM.ObjPetitioner.Sesso;
            }
		}

		private void ClearPetitionerForm(bool ClearSurname)
		{

			pVM.ResetRichiedente();
			
			//cmbPetitionerTitle.SearchText = string.Empty;
			cmbPetitionerOrganization.SearchText = string.Empty;
			cmbPetitionerCity.SearchText = string.Empty;

			if (ClearSurname)
				txtCognome.Text = string.Empty;
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

		private string ValidateUserControls(string message)
		{
			return message;
		}

		#endregion// Error Handling



		#region Title

		private void cmbTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//if (e.AddedItems.Count > 0)
			//{
			//    LK_Titolo selection = e.AddedItems[0] as LK_Titolo;
			//    pVM.ObjPetitionerTitle = pVM.GePetitionerTitleSingleItem(selection.Id_Titolo);
			//}
		}

		#endregion// Title



		#region Organization

		private void cmbOrganization_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//if (e.AddedItems.Count > 0)
			//{
			//    LK_Organizzazione selection = e.AddedItems[0] as LK_Organizzazione;
			//    pVM.ObjPetitionerOrganization = pVM.GePetitionerOrganizationSingleItem(selection.Id_Organizzazione);
			//}
		}

		#endregion// Organization



		#region City

		private void cmbCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//if (e.AddedItems.Count > 0)
			//{
			//    LK_Citta selection = e.AddedItems[0] as LK_Citta;
			//    pVM.ObjPetitionerCity = pVM.GePetitionerCitySingleItem(selection.Id_Citta);
			//}
		}

		#endregion// City



		#region Petitioner

		private void cmbPetitioner_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Richiedente selection = e.AddedItems[0] as Richiedente;
			if (selection != null && selection.Id_Richiedente > 0)
			{
				chkNewPetitioner.IsChecked = false;

				pVM.ObjPetitioner = pVM.GetPrenotationPetinionerSingleItem(selection.Id_Richiedente);
				
				pVM.ObjPetitionerCity = selection.Id_Citta != null ? pVM.GePetitionerCitySingleItem((int)selection.Id_Citta) : null;

				pVM.ObjPetitionerTitle = selection.Id_Titolo != null ? pVM.GePetitionerTitleSingleItem((int)selection.Id_Titolo) : null;
				
				pVM.ObjPetitionerOrganization = selection.Id_Organizzazione != null ? pVM.GePetitionerOrganizationSingleItem((int)selection.Id_Organizzazione) : null;
				
				pVM.ObjPetitionerLanguage = pVM.GePetitionerLanguageSingleItem((int)selection.Id_LinguaAbituale);

                if (!string.IsNullOrEmpty(pVM.ObjPetitioner.Sesso))
                    cmbSesso.SelectedValue = pVM.ObjPetitioner.Sesso;
                else
                    cmbSesso.SelectedValue = "M";
                
                
                //L'impostazione del SearchText serve per superare un bug nell'attuale
				//versione Telerik, che impedisce di cancellare un SearchText se non impostato esplicitamente. In questo caso,
				//Impostanto i SelectedItems dei vari RadAutoCompleteBox, nella Text Area di ognuno appare il valore caricato
				//ma in realta il SearchText è nullo, anche se si vede il valore. Senza questo workaround non si cancella il
				//testo se si pone a null il rispettivo SelectedItem
				SetSearchLookupTexts();

                if (LoadingFromDb)
                    LoadingFromDb = false;
                else
                {
                    string nome = string.IsNullOrEmpty(pVM.ObjPetitioner.Nome) ? "" : pVM.ObjPetitioner.Nome;
                    pVM.ObjPrenotation.Responsabile = pVM.ObjPetitioner != null ? (pVM.ObjPetitioner.Cognome + " " + nome) : string.Empty;
                }
			}
		}

		private void cmbPetitioner_SearchTextChanged(object sender, EventArgs e)
		{
			RadAutoCompleteBox rac = sender as RadAutoCompleteBox;
			if (rac != null)
			{
				if (rac.SearchText.Length > 0)
				{
					pVM.ObjPetitioner.Cognome = rac.SearchText;

                    if (!LoadingFromDb)
                    {
                        string nome = string.IsNullOrEmpty(pVM.ObjPetitioner.Nome) ? "" : pVM.ObjPetitioner.Nome;
                        pVM.ObjPrenotation.Responsabile = pVM.ObjPetitioner.Cognome + " " + nome;
                    }
				}
			}
		}

		#endregion// Petitioner



		#region Utils

		private void SetSearchLookupTexts()
		{
			cmbPetitionerCity.SearchText = pVM.ObjPetitionerCity != null ? pVM.ObjPetitionerCity.Nome : string.Empty;
			//cmbPetitionerTitle.SearchText = pVM.ObjPetitionerTitle != null ? pVM.ObjPetitionerTitle.Sigla : string.Empty;
			cmbPetitionerOrganization.SearchText = pVM.ObjPetitionerOrganization != null ? pVM.ObjPetitionerOrganization.Descrizione : string.Empty;

		}

		#endregion// Utils

        private void chkPF_Checked(object sender, RoutedEventArgs e)
        {
            cmbSesso.IsEnabled = true;
            cmbSesso.SelectedValue = "M";
        }

        private void chkPF_Unchecked(object sender, RoutedEventArgs e)
        {
            cmbSesso.IsEnabled = false;
            cmbSesso.SelectedValue = null;
        }
	}
}