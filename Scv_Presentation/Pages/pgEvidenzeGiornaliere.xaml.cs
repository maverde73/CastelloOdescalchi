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
using Scv_Model;
using Scv_Dal;
using Telerik.Windows.Controls;
using Scv_Entities;
using System.Data;
using Telerik.Windows;
using Scv_Model.Common;
using Telerik.Windows.Controls.GridView;
using Presentation.Helpers;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using System.Drawing;
using VisitsSummaryManager;
using System.Net.Mail;
using System.Configuration;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgEvidenzeGiornaliere.xaml
	/// </summary>
	public partial class pgEvidenzeGiornaliere : BaseContentPage
	{
		#region Variables

		private int MasterID;
		VisitaProgrammata_Dal dalVisitaProgrammata = new VisitaProgrammata_Dal();
		VisitaPrenotata_Dal dalVisitaPrenotata = new VisitaPrenotata_Dal();
		Prenotazione_Dal dalPrenotazione = new Prenotazione_Dal();
		ScheduledToursListViewModel vM = null;
        private List<IGroup> expandedGroups = new List<IGroup>();

		#endregion// Private Fields

        #region Properties



        #endregion




        #region Constructors

        public pgEvidenzeGiornaliere()
		{
			InitializeComponent();

			this.Loaded += new RoutedEventHandler(pgEvidenzeGiornaliere_Loaded);
            vM = new ScheduledToursListViewModel();
            this.DataContext = vM;
			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgPrenotations_CommandEvent);
			Application.Current.MainWindow.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);
			vM.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(vM_PropertyChanged);

            btnFilter.Click += new RoutedEventHandler(btnFilter_Click);
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnExportExcel.Click += new RoutedEventHandler(btnExcel_Click);

            btnCancelReference.Click += new RoutedEventHandler(btnCancelReference_Click);
            btnCancelNumeroOrdine.Click += new RoutedEventHandler(btnCancelNumeroOrdine_Click);
            btnCancelDate1.Click += new RoutedEventHandler(btnCancelDate1_Click);
            btnCancelDate2.Click += new RoutedEventHandler(btnCancelDate2_Click);

            dtpDate1.SelectionChanged += new SelectionChangedEventHandler(dtpDate1_SelectionChanged);
            dtpDate2.SelectionChanged += new SelectionChangedEventHandler(dtpDate2_SelectionChanged);

            /*
            List<VisitSummaryElement> list = new List<VisitSummaryElement>();
            VisitSummaryElement element = null;
            foreach (Hour h in vM.AvailablesHours)
            {
                element = new VisitSummaryElement();
                element.Hour = h.Time;
                list.Add(element);
            }
            visitSummary.InitializeList(list);

            InitVistSummary();
            */
			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			grdMaster.RowActivated += new EventHandler<RowEventArgs>(grdMaster_RowActivated);
			grdMaster.Height = Helper_Dal.GetOptimalScrollViewerHeight(false);

            Filter.AddSortField("Dt_Visita");
            Filter.SortDirection = SortDirection.DESC;
            gridPager.PageSize = Helper_Dal.GetOptimalGridRows(true);

            dtpDate1.SelectedValue = DateTime.Now.Date;
            btnFilter_Click(null, null);
		}


		#endregion// Constructors




		#region Events

		void vM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "SrcEvidenzeGiornaliere":
					//InitVistSummary();
					break;
			}
		}

		void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			grdMaster.Height = Helper_Dal.GetOptimalScrollViewerHeight(false);
            BindGrid();
		}

		void pgPrenotations_CommandEvent(ContentPageCommandEventArgs e)
		{
			MailCreationResult result = new MailCreationResult();

			switch (e.CommandType)
			{
				case ContentPageCommandType.Print:
					switch (e.CommandArgument)
					{
						case "giornalieraguide":
							DoPrintGiornalieraGuide();
							break;
					}
					break;

				case ContentPageCommandType.Schedule:
					DoOpenItems();
					vM.Loading = true;
					vM.LoadMaster();
					vM.Loading = false;
					break;

				case ContentPageCommandType.Send:
					Mailer mailer = new Mailer();

					List<V_EvidenzeGiornaliere> listAll = new List<V_EvidenzeGiornaliere>();
					foreach (V_EvidenzeGiornaliere o in grdMaster.Items)
						listAll.Add(o);

					result = mailer.CreateGuideVisitNotice(listAll, dtpDate1.SelectedDate != null ? dtpDate1.SelectedDate.Value.Month : DateTime.Now.Month, dtpDate1.SelectedDate != null ? dtpDate1.SelectedDate.Value.Year : DateTime.Now.Year);
					if (!result.success)
						MessageBox.Show(result.ErrorList);
					else
						MessageBox.Show("Gli avvisi sono stati spediti con successo");

					vM.LoadMaster();
					break;

				case ContentPageCommandType.Other:
					switch (e.CommandArgument)
					{
						case "annullaprenotazione":
							if (MessageBox.Show("Annullare la prenotazione?", "Annullamento prenotazione", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
							{
								DoCancelPrenotation(((V_EvidenzeGiornaliere)grdMaster.SelectedItem).Id_Prenotazione);
								vM.LoadMaster();
							}
							break;

					}
					break;
			}
		}

		private void grdMaster_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
		{
			if (vM.Loading) return;

			RadGridView grd = sender as RadGridView;
			if (grd != null)
			{
				if (grd.SelectedItems.Count > 0)
				{
					MasterID = (int)((V_EvidenzeGiornaliere)grd.SelectedItems[0]).Id_VisitaProgrammata;
					NotifySelection();
				}
			}
		}

		void grdMaster_RowActivated(object sender, RowEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
				if (grd.SelectedItems.Count > 0 && ((V_EvidenzeGiornaliere)grd.SelectedItems[0]).Id_VisitaPrenotata > 0)
				{
					DoOpenItem(((V_EvidenzeGiornaliere)grd.SelectedItems[0]), WindowStartupLocation.CenterScreen);
					vM.LoadMaster();
				}
		}

		void pgEvidenzeGiornaliere_Loaded(object sender, RoutedEventArgs e)
		{
			vM.Loading = false;
		}

		private void grdMaster_FilterOperatorsLoading(object sender, Telerik.Windows.Controls.GridView.FilterOperatorsLoadingEventArgs e)
		{
			if (
				e.Column.UniqueName == "ora"
				||
				e.Column.UniqueName == "lingua"
				||
				e.Column.UniqueName == "nominativo"
				||
				e.Column.UniqueName == "tipovisita"
				||
				e.Column.UniqueName == "visitatore"
			)
				e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.Contains;
		}

        private void grdMaster_GroupRowIsExpandedChanged(object sender, Telerik.Windows.Controls.GridView.GroupRowEventArgs e)
        {
            if (e.Row.IsExpanded)
                expandedGroups.Add(e.Row.Group);
            else
                if (expandedGroups.Contains(e.Row.Group))
                    expandedGroups.Remove(e.Row.Group);
        }

        private void gridPager_PageIndexChanged(object sender, PageIndexChangedEventArgs e)
        {
            grdMaster.CurrentItem = null;
        }

		protected void frm_DetailWindowClosing(object sender, ClosingDetailWindowEventArgs e)
		{
			vM.LoadMaster();
			NotifySelection();
		}


        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (dtpDate1.SelectedDate != null && dtpDate2.SelectedDate != null)
            {
                if (dtpDate1.SelectedDate > dtpDate2.SelectedDate)
                {
                    MessageBox.Show("La data 'Dal' non può essere maggiore della data 'Al'.");
                    return;
                }
            }
            BindGrid();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DoCancelFilter();
            BindGrid();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            ExcelReport report = new ExcelReport();
            var now = DateTime.Now.ToString("dd/MM/yyyy HH.mm.ss");
            string excelreportfolder = ConfigurationManager.AppSettings["excelreportfolder"];
            string filePath = string.Format(@"{0}Report_Visite_{1}.xlsx",excelreportfolder, now.Replace("/", "_").Replace(".", "_").Replace(" ", "_"));

            //dtpDate1.SelectedDate = null;
            //dtpDate2.SelectedDate = null;
            string filtri = "";
            if (dtpDate1.SelectedDate != null)
            {
                filtri = "Dal " + Convert.ToDateTime(dtpDate1.SelectedDate).ToString("dd/MM/yyyy");
            }

            if (dtpDate2.SelectedDate != null)
            {
                if(filtri != "")
                    filtri =  filtri + " al " + Convert.ToDateTime(dtpDate2.SelectedDate).ToString("dd/MM/yyyy");
                else
                    filtri = "Al " + Convert.ToDateTime(dtpDate2.SelectedDate).ToString("dd/MM/yyyy");

            }

            filtri = filtri.Replace("/", "_");

            if (filtri == "")
                filtri = "Visite";


            report.SaveExcel(vM.SrcEvidenzeGiornaliere, filePath, filtri);

            //ShellExecute shellExecute = new ShellExecute();
            //shellExecute.Verb = ShellExecute.OpenFile;
            //shellExecute.Path = filePath;
            //shellExecute.Execute();

            Execute(filePath);
        }


        private void btnCancelReference_Click(object sender, RoutedEventArgs e)
        {
            txtReference.Text = string.Empty;
            BindGrid();
        }

        private void btnCancelNumeroOrdine_Click(object sender, RoutedEventArgs e)
        {
            txtNumeroOrdine.Text = string.Empty;
            BindGrid();
        }

        private void btnCancelDate1_Click(object sender, RoutedEventArgs e)
        {
            dtpDate1.SelectedDate = null;
            BindGrid();
        }

        private void btnCancelDate2_Click(object sender, RoutedEventArgs e)
        {
            dtpDate2.SelectedDate = null;
            BindGrid();
        }

        void dtpDate1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDate1.SelectedValue != null && (dtpDate2.SelectedValue == null || dtpDate2.SelectedValue.Value.Date < dtpDate1.SelectedValue.Value.Date))
                dtpDate2.SelectedValue = dtpDate1.SelectedValue;
        }

        void dtpDate2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDate2.SelectedValue != null && (dtpDate1.SelectedValue == null || dtpDate1.SelectedValue.Value.Date > dtpDate2.SelectedValue.Value.Date))
                dtpDate1.SelectedValue = dtpDate2.SelectedValue;
        }


		#endregion// Events




		#region Context Menu

		private void contextMenu_Opening(object sender, RoutedEventArgs e)
		{
			int selectedItems = grdMaster.SelectedItems.Count;
			mnuCancel.IsEnabled = false;
			if (selectedItems > 0)
			{
				mnuCancel.IsEnabled = true;
			}
		}

		private void contextMenu_Click(object sender, RadRoutedEventArgs e)
		{
			switch (((RadMenuItem)(e.Source)).CommandParameter.ToString())
			{
				case "cancel":
					if (MessageBox.Show("Annullare la prenotazione?", "Annullamento prenotazione", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						DoCancelPrenotation(((V_EvidenzeGiornaliere)grdMaster.SelectedItem).Id_Prenotazione);
						vM.LoadMaster();
					}
					break;
			}
		}

		#endregion// Context Menu




		#region Private Methods

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				if ((int)((V_EvidenzeGiornaliere)grdMaster.SelectedItems[i]).Id_VisitaPrenotata > 0)
					args.SelectedIDs.Add((int)((V_EvidenzeGiornaliere)grdMaster.SelectedItems[i]).Id_VisitaProgrammata);

			OnItemSelectionEvent(args);
		}

		private void DoOpenItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoOpenItem(((V_EvidenzeGiornaliere)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
		}

		private void DoOpenItem(V_EvidenzeGiornaliere obj, WindowStartupLocation startupLocation)
		{
			bool open = true;
			string windowName = string.Format("Prenotation_{0}", obj.Id_VisitaProgrammata.ToString());
			BaseDetailPage frm = null;
			foreach (Window wnd in Application.Current.Windows)
			{
				if (wnd.Name == windowName && wnd.IsVisible)
				{
					frm = (BaseDetailPage)wnd;
					frm.Focus();
					open = false;
				}
			}
			if (open)
			{
				VisitaPrenotata v = dalVisitaPrenotata.GetItem(obj.Id_VisitaPrenotata);
				if (v != null)
				{
					frm = new wndScheduleTours(v.Id_Prenotazione, User);
					frm.User = User;
					frm.Name = windowName;
					frm.WindowStartupLocation = startupLocation;
					frm.DetailWindowClosing += new CommonDelegates.ClosingDetailWindowEventHandler(frm_DetailWindowClosing);
					frm.ShowDialog();
				}
			}
		}

		private void DoScheduleTours(V_EvidenzeGiornaliere obj, WindowStartupLocation startupLocation)
		{
			bool schedule = true;
			string windowName = string.Format("Prenotation_{0}", obj.Id_VisitaProgrammata.ToString());
			BaseDetailPage frm = null;
			foreach (Window wnd in Application.Current.Windows)
			{
				if (wnd.Name == windowName && wnd.IsVisible)
				{
					frm = (BaseDetailPage)wnd;
					frm.Focus();
					schedule = false;
				}
			}
			if (schedule)
			{
				frm = new wndScheduleTours(obj.Id_VisitaProgrammata, User);
				frm.Name = windowName;
				frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				//frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoScheduleToursMultiple()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoScheduleTours(((V_EvidenzeGiornaliere)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
		}

		private void DoPrintGiornalieraGuide()
		{
			Guida_Dal dal = new Guida_Dal();
			List<V_AssegnazioniGuideVisite> list = dal.GetAssegnazioniGuideVisite(vM.VisitDate);

			BasePrintPage frm = new wndPrintAssegnazioneGuideVisite();
			frm.DsAssegnazioneGuideVisite = list;
			frm.PrintAssegnazioneGuideVisiteArgs.VisitDate = vM.VisitDate;
			frm.ShowDialog();
		}

        /*
		private void InitVistSummary()
		{
			List<V_EvidenzeGiornaliere> evList = null;

			//Aggiornamento sommario visite
			evList = dalVisitaProgrammata.GetV_EvidenzeGiornaliereVisitsSummary(vM.SrcEvidenzeGiornaliere);

            foreach (VisitSummaryElement e in visitSummary.Elements)
                visitSummary.SetItem(e.Hour, string.Empty, string.Empty);

            foreach (V_EvidenzeGiornaliere e in evList)
                visitSummary.SetItem(e.Ora_Visita, e.SiglaLingua, ((e.Nr_Interi != null ? (short)e.Nr_Interi : 0) + (e.Nr_Omaggio != null ? (short)e.Nr_Omaggio : 0) + (e.Nr_Ridotti != null ? (short)e.Nr_Ridotti : 0)).ToString());



			//Aggiornamento totali visite
			int totVisitors = 0;
			List<VisitSummaryElement> list = new List<VisitSummaryElement>();
			evList = dalVisitaProgrammata.GetV_EvidenzeGiornaliereGroupByLanguageSold(vM.SrcEvidenzeGiornaliere);

			VisitSummaryElement element = null;
			int tot = 0;
			foreach (V_EvidenzeGiornaliere evg in evList)
			{
				tot =
					(evg.Nr_InteriConsegnati != null ? (int)evg.Nr_InteriConsegnati : 0)
					+
					(evg.Nr_RidottiConsegnati != null ? (int)evg.Nr_RidottiConsegnati : 0)
					+
					(evg.Nr_OmaggioConsegnati != null ? (int)evg.Nr_OmaggioConsegnati : 0)
					;

				element = new VisitSummaryElement();
				element.Hour = string.Empty;
				element.Lang = evg.SiglaLingua;
				element.Number = tot.ToString();
				totVisitors += tot;

				list.Add(element);
			}
			vTotal.InitializeList(list);
		}
        */
		private void DoCancelPrenotation(int prenotationID)
		{
			Prenotazione_Dal dalPrenotation = new Prenotazione_Dal();
			VisitaProgrammata_Dal dalVisit = new VisitaProgrammata_Dal();
			Prenotazione p = dalPrenotation.GetItem(prenotationID);
			if (p != null)
			{
				p.Id_TipoRisposta = 6;
				dalPrenotation.ChangePrenotationResponseTypeID(p);
				List<V_EvidenzeGiornaliere> visits = dalVisit.GetEvidenzeGiornaliere(p.Protocollo).ToList();
				SendPetitionerMail(p, visits, false);
			}
		}

		private void SendPetitionerMail(Prenotazione prenotation, List<V_EvidenzeGiornaliere> visits, bool currentConfirmStatus)
		{
			VisitaProgrammata_Dal dalVisit = new VisitaProgrammata_Dal();
			Richiedente_Dal dalPetitioner = new Richiedente_Dal();
			Parametri_Dal dalParameters = new Parametri_Dal();
			LK_TestoStandard_Dal dalText = new LK_TestoStandard_Dal();
			Mailer mailer = new Mailer();

			List<VisitaProgrammata> v = new List<VisitaProgrammata>();
			Richiedente petitioner = null;
			string recipientsLabel = string.Empty;
			string subjectLabel = string.Empty;
			string testText = string.Empty;

			bool forwardToSenderVisitor = false;
			bool.TryParse(dalParameters.GetItem("forwardToSenderVisitor").Valore, out forwardToSenderVisitor);

			recipientsLabel = dalText.GetText(prenotation.Id_LinguaRisposta, "FAX_TO").Testo;
			subjectLabel = dalText.GetText(prenotation.Id_LinguaRisposta, "FAX_SUBJ").Testo;

			foreach (V_EvidenzeGiornaliere vp in visits)
				if (vp.Id_Prenotazione == prenotation.Id_Prenotazione)
					v.Add(dalVisit.GetItem(vp.Id_VisitaProgrammata));

			MailCreationResult result = new MailCreationResult();

			string subject = string.Empty;
			string body = string.Empty;

            mailer.GetSubjectAndBody(prenotation.Id_LinguaRisposta, (int)prenotation.Id_TipoRisposta, out subject, out body);

			testText += "<b>subject:</b><br />" + subject + "<br /><br /><b>Body</b>:<br />" + body + "<br />----------------------<br />";

			petitioner = dalPetitioner.GetItem(prenotation.Id_Richiedente);

			if (petitioner != null)
			{
                string nome = string.IsNullOrEmpty(petitioner.Nome) ? "" : petitioner.Nome;
                EmailPreviewItem item = new EmailPreviewItem(recipientsLabel, petitioner.Email, petitioner.Cognome + " " + nome, subjectLabel, subject, ".jpg", body);
				BaseDetailPage frm = new wndEmailPreview(item);
				frm.ShowDialog();
				if (frm.DialogResult == true)
				{
					switch (frm.EmailDestination)
					{
						case Scv_Model.EmailDestination.Send:
							try
							{
								//string sender = new Parametri_Dal().GetItem("mailService_default_from").Valore.ToString();
                                MailMessage message = new MailMessage("petitionerFrom@email.it", petitioner.Email, subject, result.Text);
								if (message != null)
									DoSendMail(message, visits, forwardToSenderVisitor, DateSaveTarget.Visitor, prenotation.Id_Prenotazione);
							}
							catch (Exception e)
							{
								string message = "Si è verificato un errore nella creazione del messaggio Email";

								if (e.TargetSite.ReflectedType.Name == "MailAddressParser")
									message += ":\nIl richiedente ha un indirizzo di email non valido.";

								MessageBox.Show(message);
							}
							break;

						case Scv_Model.EmailDestination.Print:

							break;

						case Scv_Model.EmailDestination.Copy:

							break;
					}
				}
			}
		}

		private void DoSendMail(MailMessage message, List<V_EvidenzeGiornaliere> ScheduledVisits, bool forwardToSender, DateSaveTarget saveDate, int? prenotationID)
		{
			Email_Dal dal = new Email_Dal();
			if (dal.InsertOrUpdate(message, true, forwardToSender, saveDate, prenotationID, ScheduledVisits) > 0)
				MessageBox.Show("Il messaggio è stato correttamente inserito nella coda di invio");
			else
				MessageBox.Show("Non è stato possibile inviare il messaggio a causa di un problema tecnico");
		}


        private void DoFilter()
        {
            Filter.RemoveFilter("NProtocollo");
            if (txtReference.Text.Length > 0)
                Filter.AddFilter("NProtocollo", Utilities.ValueType.String, txtReference.Text);

            Filter.RemoveFilter("NumeroOrdine");
            if (txtNumeroOrdine.Text.Length > 0)
                Filter.AddFilter("NumeroOrdine", Utilities.ValueType.String, txtNumeroOrdine.Text);

            //Filtro su date
            List<object> dateValues = new List<object>();
            if (dtpDate1.SelectedDate != null)
                dateValues.Add(dtpDate1.SelectedDate.Value.Date);
            if (dtpDate2.SelectedDate != null)
                dateValues.Add(dtpDate2.SelectedDate.Value.Date);

            Filter.RemoveFilter("Dt_Visita");

            if (dtpDate1.SelectedDate != null && dtpDate2.SelectedDate == null)
                Filter.SetFilter("Dt_Visita", Utilities.ValueType.DateTime, dtpDate1.SelectedDate.Value.Date, Utilities.SQLOperator.GreaterThanEqual);

            if (dtpDate1.SelectedDate == null && dtpDate2.SelectedDate != null)
                Filter.SetFilter("Dt_Visita", Utilities.ValueType.DateTime, dtpDate2.SelectedDate.Value.Date, Utilities.SQLOperator.LessThanEqual);

            if (dtpDate1.SelectedDate != null && dtpDate2.SelectedDate != null)
            {
                if (dtpDate1.SelectedDate != dtpDate2.SelectedDate)
                    Filter.SetFilter("Dt_Visita", Utilities.ValueType.DateTime, dateValues, Utilities.SQLOperator.Between);
                else
                    Filter.SetFilter("Dt_Visita", Utilities.ValueType.DateTime, dtpDate1.SelectedDate.Value.Date, Utilities.SQLOperator.Equal);
            }

            //Filtro su tipo movimento
            if (vM.SelectedStatusTypeID != 0)
                Filter.SetFilter("Id_TipoRisposta", Utilities.ValueType.Int, vM.SelectedStatusTypeID);
            else
                Filter.RemoveFilter("Id_TipoRisposta");

            vM.Filter = Filter;

        }

        private void DoCancelFilter()
        {
            txtReference.Text = string.Empty;
            txtNumeroOrdine.Text = string.Empty;
            dtpDate1.SelectedDate = null;
            dtpDate2.SelectedDate = null;
            vM.SelectedStatusTypeID = 0;
        }


        private void BindGrid()
        {
            DoFilter();
            vM.LoadMaster();
        }


		#endregion// Private Methods

        private  System.Diagnostics.Process Execute(string path)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(path);

            info.RedirectStandardError = false;
            info.RedirectStandardOutput = false;
            info.UseShellExecute = true;

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = info;

            p.Start();

            return p;
        }

	}
}
