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
using Scv_Dal;
using Scv_Model;
using Telerik.Windows.Controls;
using Scv_Entities;
using Presentation.Helpers;
using System.Collections;
using Telerik.Windows.Controls.GridView;
using System.Windows.Controls.Primitives;
using Scv_Model.Common;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.ComponentModel;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgGuidesAssignment.xaml
	/// </summary>
	public partial class pgGuidesAssignment : BaseContentPage
	{
		#region Private Fields

		VisitaProgrammata_Dal dalScheduledTours = new VisitaProgrammata_Dal();

		List<V_EvidenzeGiornaliere> masterTable = null;

		GuideAssignmentViewModel pVM = null;

		DateTime? selectedDate = null;

		BaseFilter guidesFilter = null;

		bool allAvvised = false;

		#endregion// Private Fields



		#region Public Properties

		public List<V_EvidenzeGiornaliere> MasterTable
		{
			get
			{
				if (masterTable == null)
					masterTable = new List<V_EvidenzeGiornaliere>();
				return masterTable;
			}
			set { masterTable = value; }
		}

		public BaseFilter GuidesFilter
		{
			get
			{
				if (guidesFilter == null)
					guidesFilter = new BaseFilter();
				return guidesFilter;
			}
			set { guidesFilter = value; }
		}

		#endregion



		#region Constructors

		public pgGuidesAssignment()
		{
			DateTime start = DateTime.Now;

			pVM = new GuideAssignmentViewModel();
			this.DataContext = pVM;

			InitializeComponent();

			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgGuides_CommandEvent);
			bntApply.Click += new RoutedEventHandler(bntApply_Click);

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.Grouped += new EventHandler<GridViewGroupedEventArgs>(grdMaster_Grouped);

			this.Loaded += new RoutedEventHandler(pgGuidesAssignment_Loaded);
			clnVisits.SelectionChanged += new SelectionChangedEventHandler(clnVisits_SelectionChanged);
			clnVisits.DisplayDateChanged += new EventHandler<Telerik.Windows.Controls.Calendar.CalendarDateChangedEventArgs>(clnVisits_DisplayDateChanged);
			txtFltCognome.TextChanged += new TextChangedEventHandler(txtFltCognome_TextChanged);
			btnCancelSelection.Click += new RoutedEventHandler(btnCancelSelection_Click);

			DateTime dtStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			DateTime dtEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).Subtract(new TimeSpan(24, 0, 0));
			List<object> values = new List<object> { dtStart, dtEnd };

			selectedDate = dtStart;

			//filtro guide
			GuidesFilter.AddSortField("Cognome");
			GuidesFilter.AddSortField("Nome");
			GuidesFilter.SetFilter("Fl_Attivo", Utilities.ValueType.Bool, true);

			pVM.LoadAvailableGuidesFilter(GuidesFilter);

			Filter.SetFilter("Dt_Visita", Utilities.ValueType.DateTime, values, Utilities.SQLOperator.Between);

			Filter.AddSortField("Dt_Visita");
			Filter.AddSortField("Ora_Visita");
			Filter.SortDirection = SortDirection.DESC;

			BindMaster();

			TimeSpan end = DateTime.Now.Subtract(start);
			Console.WriteLine("Costruttore Completato in " + end.Seconds.ToString() + " secondi");

		}


		#endregion



		#region Events Handling

		private void grdMaster_SelectionChanged(object sender, SelectionChangeEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
			{
				NotifySelection();
			}
		}

		private void pgGuides_CommandEvent(ContentPageCommandEventArgs e)
		{
			MailCreationResult result = new MailCreationResult();

			switch (e.CommandType)
			{
				case ContentPageCommandType.Filter:

					MethodArgument arg = null;

					switch (e.CommandArgument)
					{
						case "guides":
							arg = e.Filter.GetFilter("Id_Guida");
							if (arg != null)
								Filter.SetFilter(arg.Field, arg.ValueType, arg.Values);
							else
								Filter.RemoveFilter("Id_Guida");

							BindMaster();
							break;

						case "guidesLookup":
							arg = e.Filter.GetFilter("Cognome");
							if (arg != null)
								Filter.SetFilter(arg.Field, arg.ValueType, arg.Values);
							else
								Filter.RemoveFilter("Cognome");
							break;

						case "calendar":
							arg = e.Filter.GetFilter("Dt_Visita");
							if (arg != null)
							{
								Filter.SetFilter(arg.Field, arg.ValueType, arg.Values, arg.SQLOperator);
								selectedDate = (DateTime)Filter.GetFilter("Dt_Visita").Values[0];
							}
							else
							{
								Filter.RemoveFilter("Dt_Visita");
								selectedDate = null;
							}
							BindMaster();
							break;
					}
					break;

				case ContentPageCommandType.Send:
					DoApply();
					break;

				case ContentPageCommandType.Print:
					switch (e.CommandArgument)
					{
						case "all":
							DoPrintAll();
							break;
					}
					break;

				case ContentPageCommandType.Other:

					switch (e.CommandArgument)
					{
						case "avvall":
							SelectAllGuides();
							break;

						case "accall":
							ConfirmAllGuides();
							break;
					}
					break;

			}
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.RemovedItems.Count > 0)
			{

			}
		}

		private void OnSelectionChanged(object sender, Telerik.Windows.Controls.RadSelectionChangedEventArgs e)
		{
			if (e.RemovedItems.Count > 0)
			{

			}
		}
		
		private void grdMaster_Grouped(object sender, GridViewGroupedEventArgs e)
		{
			grdMaster.CollapseAllGroups();
		}

		private void lstGuides_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				if (((Guida)e.AddedItems[0]).Id_Guida > 0)
					Filter.SetFilter("Id_Guida", Utilities.ValueType.Int, ((Guida)e.AddedItems[0]).Id_Guida);
				else
					Filter.RemoveFilter("Id_Guida");

				BindMaster();
			}
		}

		private void txtFltCognome_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox obj = sender as TextBox;
			if (obj != null)
			{
				if (txtFltCognome.Text.Length > 0)
					GuidesFilter.SetFilter("Cognome", Utilities.ValueType.String, new List<object>() { txtFltCognome.Text }, Utilities.SQLOperator.StartWith);
				else
					GuidesFilter.RemoveFilter("Cognome");

				pVM.LoadAvailableGuidesFilter(GuidesFilter);
			}
		}

		private void clnVisits_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RadCalendar c = sender as RadCalendar;
			if (c != null)
			{
				if (c.SelectedDate != null)
					Filter.SetFilter("Dt_Visita", Utilities.ValueType.DateTime, c.SelectedDate);
				else
					Filter.RemoveFilter("Dt_Visita");

				BindMaster();
			}
		}

		private void clnVisits_DisplayDateChanged(object sender, Telerik.Windows.Controls.Calendar.CalendarDateChangedEventArgs e)
		{
			SelectAllMonth((RadCalendar)sender);
		}

		private void btnCancelSelection_Click(object sender, RoutedEventArgs e)
		{
			SelectAllMonth(clnVisits);
		}

		private void bntApply_Click(object sender, RoutedEventArgs e)
		{
			DoApply();
		}

		private void GuidesUpdated(object sender, RunWorkerCompletedEventArgs e)
		{
			MessageBox.Show("Guide assegnate con successo.", "Assegnazione guide");
			SendGuidesMail(new ObservableCollection<V_EvidenzeGiornaliere>(pVM.SrcEvidenzeGiornaliere));
			BindMaster();
			pVM.PgValue = 0;
			pVM.IsEnabled = true;
		}

		private void frm_Closed(object sender, EventArgs e)
		{
			BindMaster();
			NotifySelection();
		}

		void pgGuidesAssignment_Loaded(object sender, RoutedEventArgs e)
		{
		}

		#endregion// Events



		#region Private Methods

		private void SelectAllMonth(RadCalendar c)
		{
			if (c != null)
			{
				ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
				args.CommandType = ContentPageCommandType.Filter;
				args.CommandArgument = "calendar";

				if (c.DisplayDate != null)
				{
					DateTime dtStart = new DateTime(c.DisplayDate.Year, c.DisplayDate.Month, 1);
					DateTime dtEnd = new DateTime(c.DisplayDate.Year, c.DisplayDate.Month, 1).AddMonths(1).Subtract(new TimeSpan(24, 0, 0));
					List<object> values = new List<object> { dtStart, dtEnd };

					c.SelectedDate = null;

					Filter.SetFilter("Dt_Visita", Utilities.ValueType.DateTime, values, Utilities.SQLOperator.Between);
				}
				else
					Filter.RemoveFilter("Dt_Visita");

				BindMaster();
			}
		}

		private void BindMaster()
		{
			pVM.BindMaster(Filter);
		}

		private void SendGuidesMail(ObservableCollection<V_EvidenzeGiornaliere> visits)
		{
			LK_TestoStandard_Dal dalText = new LK_TestoStandard_Dal();
			Guida_Dal dalGuide = new Guida_Dal();
			GuidaLingua_Dal dalGuideLanguage = new GuidaLingua_Dal();

			MailCreationResult result = new MailCreationResult();
			Mailer mailer = new Mailer();

			string recipientsLabel = string.Empty;
			string subjectLabel = string.Empty;
			string testText = string.Empty;
			string subject = string.Empty;
			int languageID = 0;
			V_GuidaLingua gl = null;

			List<Guida> mailList = new List<Guida>();
			Guida g = null;
			List<int> guidesIDs = new List<int>();
			List<V_EvidenzeGiornaliere> guideVisits = null;

			foreach (V_EvidenzeGiornaliere evg in visits)
				if (evg.Fl_AvvisaGuida == true)
				{
					if (!guidesIDs.Contains((int)evg.Id_Guida))
					{
						guidesIDs.Add((int)evg.Id_Guida);
						g = dalGuide.GetItem((int)evg.Id_Guida);
						if (g != null)
							mailList.Add(g);
					}
				}

			if (mailList.Count > 0)
				foreach (Guida gd in mailList)
				{
					guideVisits = visits.Where(x => x.Id_Guida == gd.Id_Guida).ToList();
					gl = dalGuideLanguage.GetItemsByGuideID(gd.Id_Guida).FirstOrDefault(x => x.Fl_Madre == true);
					if (gl == null)
						gl = dalGuideLanguage.GetItemsByGuideID(gd.Id_Guida).ToList()[0];

					if (gl != null)
					{
						languageID = 1;//IOtaliano fixed
						recipientsLabel = dalText.GetText(languageID, "FAX_TO").Testo;//1 = Italiano
						subjectLabel = dalText.GetText(languageID, "FAX_SUBJ").Testo;//1 = Italiano
						subject = "Assegnazione visite al Castello Odescalchi di Bracciano";

						result = mailer.CreateGuideVisitNotice(guideVisits, guideVisits[0].Dt_Visita.Month, guideVisits[0].Dt_Visita.Year);
						EmailPreviewItem item = new EmailPreviewItem(recipientsLabel, gd.Email, gd.Cognome + " " + gd.Nome, subjectLabel, subject, ".jpg", result.Text);
						BaseDetailPage frm = new wndEmailPreview(item);
						frm.ShowDialog();
						if (frm.DialogResult == true)
						{
							switch (frm.EmailDestination)
							{
								case Scv_Model.EmailDestination.Send:
									try
									{
										//string sender = new Parametri_Dal().GetItem("mailService_altern_from").Valore.ToString();
										MailMessage message = new MailMessage("guideFrom@email.it", g.Email, subject, result.Text);
										if (message != null)
                                            DoSendMail(message, visits.ToList(), pVM.ForwardToSenderGuide, DateSaveTarget.Guide, null, gd.Cognome + " " + gd.Nome);
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
			else
			{
				result.success = false;
				result.ErrorList += "\nNon ci sono guide selezionate per l'avviso.";
			}

			if (!result.success)
				MessageBox.Show(result.ErrorList);
			else
				MessageBox.Show("Gli avvisi sono stati spediti con successo");
		}

        private void DoSendMail(MailMessage message, List<V_EvidenzeGiornaliere> ScheduledVisits, bool forwardToSender, DateSaveTarget saveDate, int? prenotationID, string guideName = "")
		{
			Email_Dal dal = new Email_Dal();
            if (dal.InsertOrUpdate(message, true, forwardToSender, saveDate, prenotationID, ScheduledVisits.ToList(), guideName) > 0)
				MessageBox.Show("Il messaggio è stato correttamente inserito nella coda di invio");
			else
				MessageBox.Show("Non è stato possibile inviare il messaggio a causa di un problema tecnico");
		}

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				args.SelectedIDs.Add((int)((V_EvidenzeGiornaliere)grdMaster.SelectedItems[i]).Id_VisitaProgrammata);

			OnItemSelectionEvent(args);
		}

		private void CheckChangedGuides()
		{
			foreach (V_EvidenzeGiornaliere o in pVM.SrcEvidenzeGiornaliere)
			{
				foreach (V_EvidenzeGiornaliere old in pVM.SrcOldEvidenzeGiornaliere)
					if (old.Dt_Visita == o.Dt_Visita && old.Ora_Visita == o.Ora_Visita && old.Id_Lingua == o.Id_Lingua)
						if (o.Id_Guida != old.Id_Guida)
							o.Dt_InvioAvviso = (DateTime?)null;
			}
		}

		private void DoApply()
		{
			CheckChangedGuides();
			pVM.IsEnabled = false;
			BackgroundWorker updateGuidesWorker = new BackgroundWorker();
			updateGuidesWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(pVM.DoUpdateGuides);
			updateGuidesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GuidesUpdated);
			updateGuidesWorker.RunWorkerAsync();
		}

		private void SelectAllGuides()
		{
			pVM.SrcEvidenzeGiornaliere.ForEach(x =>  x.Fl_AvvisaGuida = (x.Id_Guida!= null && x.Id_Guida >0) ? (!allAvvised) ? true : false : false);
			allAvvised = !allAvvised;
		}

		private void ConfirmAllGuides()
		{
			pVM.SrcEvidenzeGiornaliere.ForEach(x => x.Fl_AccettaGuida = (x.Id_Guida != null && x.Id_Guida > 0) ? true : false);
		}

		private void DoPrintAll()
		{
			string windowName = "Firma_guide";
			BasePrintPage frm = null;

			PrintAssegnazioniGuideVisiteArgs args = new PrintAssegnazioniGuideVisiteArgs();
			args.VisitDate = selectedDate.Value.Date;

			frm = new wndPrintGuidesAssignmentList();
			frm.Name = windowName;
			frm.DsEvidenzeGiornaliere = pVM.SrcEvidenzeGiornaliere;
			frm.PrintAssegnazioneGuideVisiteArgs = args;
			frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			frm.Closed += new EventHandler(frm_Closed);
			frm.ShowDialog();
		}

		#endregion// Private Methods

	}
}
