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
using VisitsSummaryManager;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgTicketOffice.xaml
	/// </summary>
	public partial class pgTicketOffice : BaseContentPage
	{

		#region Variables

		private int MasterID;
		VisitaProgrammata_Dal dalVisitaProgrammata = new VisitaProgrammata_Dal();
		VisitaPrenotata_Dal dalVisitaPrenotata = new VisitaPrenotata_Dal();
		Prenotazione_Dal dalPrenotazione = new Prenotazione_Dal();
		ScheduledToursListViewModel vM = null;

		#endregion// Variables



		#region Constructors

		public pgTicketOffice()
		{
			InitializeComponent();

			vM = new ScheduledToursListViewModel();
            vM.VisitDate = DateTime.Now.Date;
			this.DataContext = vM;
 			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgPrenotations_CommandEvent);
			Application.Current.MainWindow.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);
			vM.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(vM_PropertyChanged);
			this.Loaded += new RoutedEventHandler(pgTicketOffice_Loaded);

			//List<VisitSummaryElement> list = new List<VisitSummaryElement>();
			//VisitSummaryElement element = null;
			//foreach (Hour h in vM.AvailablesHours)
			//{
			//    element = new VisitSummaryElement();
			//    element.Hour = h.Time;
			//    list.Add(element);
			//}
			//visitSummary.InitializeList(list);

			InitVistSummary();

			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			//grdMaster.MouseDoubleClick += new MouseButtonEventHandler(grdMaster_MouseDoubleClick);
			grdMaster.RowActivated += new EventHandler<RowEventArgs>(grdMaster_RowActivated);			
			grdMaster.Height = Helper_Dal.GetOptimalScrollViewerHeight(false);
		}

		#endregion// Constructors



		#region Events Handlers

		private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			grdMaster.Height = Helper_Dal.GetOptimalScrollViewerHeight(false);
            vM.LoadMaster(vM.VisitDate);
		}

		void vM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "SrcEvidenzeGiornaliere":
					InitVistSummary();
					break;
			}
		}

		private void pgPrenotations_CommandEvent(ContentPageCommandEventArgs e)
		{
			MailCreationResult result = new MailCreationResult();

			switch (e.CommandType)
			{
				case ContentPageCommandType.Print:
					switch (e.CommandArgument)
					{
						case "printtickets":
							DoPrintTickets();
                            vM.LoadMaster(vM.VisitDate);
							break;
					}
					break;
			}
		}

		private void grdMaster_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
			{
				if (grd.SelectedItems.Count > 0)
					MasterID = (int)((V_EvidenzeGiornaliere)grd.SelectedItems[0]).Id_VisitaProgrammata;

				NotifySelection();
			}
		}

		void grdMaster_RowActivated(object sender, RowEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
				if (grd.SelectedItems.Count > 0)
					DoPrintTicket(((V_EvidenzeGiornaliere)grd.SelectedItems[0]), WindowStartupLocation.CenterScreen);
		}

		void pgTicketOffice_Loaded(object sender, RoutedEventArgs e)
		{
			vM.Loading = false;
			vM.VisitDate = DateTime.Now.Date;
		}

		#endregion// Events



		#region Context Menu

		private void contextMenu_Opening(object sender, RoutedEventArgs e)
		{
			int selectedItems = grdMaster.SelectedItems.Count;
			bool printable = false;

			mnuPrint.IsEnabled = false;

			foreach (V_EvidenzeGiornaliere mm in grdMaster.SelectedItems)
				if (mm.Id_VisitaPrenotata > 0)
					printable = true;

			if (printable)
				mnuPrint.IsEnabled = true;
		}

		private void contextMenu_Click(object sender, RadRoutedEventArgs e)
		{
			switch (((RadMenuItem)(e.Source)).CommandParameter.ToString())
			{
				case "printtickets":
					DoPrintTickets();
                    vM.LoadMaster(vM.VisitDate);
					break;
			}
		}

		#endregion// Context Menu



		#region Private Methods

		private void InitVistSummary()
		{
			//Aggiornamento totali visite per lingue
			int totVisitors = 0;
			List<VisitSummaryElement> list = new List<VisitSummaryElement>();
			List<V_EvidenzeGiornaliere> evList = dalVisitaProgrammata.GetV_EvidenzeGiornaliereGroupByLanguageSold(vM.SrcEvidenzeGiornaliere);

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

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				if ((int)((V_EvidenzeGiornaliere)grdMaster.SelectedItems[i]).Id_VisitaPrenotata > 0)
					args.SelectedIDs.Add((int)((V_EvidenzeGiornaliere)grdMaster.SelectedItems[i]).Id_VisitaProgrammata);

			OnItemSelectionEvent(args);
		}

		private void DoPrintTickets()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoPrintTicket(((V_EvidenzeGiornaliere)grdMaster.SelectedItems[i]), WindowStartupLocation.CenterScreen);
		}

		private void DoPrintTicket(V_EvidenzeGiornaliere obj, WindowStartupLocation startupLocation)
		{
			bool open = true;
			string windowName = string.Format("Visita_{0}", obj.Id_VisitaProgrammata.ToString());
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
				VisitaProgrammata v = dalVisitaProgrammata.GetItem(obj.Id_VisitaProgrammata);
				if (v != null)
				{
					if (v.Dt_Visita.Date != DateTime.Now.Date)
					{
						MessageBox.Show("Impossibile emettere biglietti per una visita non programmata OGGI", "Errore stampa biglietti");
						return;
					}
					frm = new wndTicketDetails(v.Id_VisitaProgrammata, User);
					frm.Name = windowName;
					frm.WindowStartupLocation = startupLocation;
					frm.DetailWindowClosing += new CommonDelegates.ClosingDetailWindowEventHandler(frm_DetailWindowClosing);
					frm.ShowDialog();
                    vM.LoadMaster(vM.VisitDate);

					foreach(V_EvidenzeGiornaliere o in grdMaster.Items)
					{
						if(o.Id_VisitaProgrammata == obj.Id_VisitaProgrammata)
						{
							vM.SelectedItem = o;
							grdMaster.ScrollIntoView(vM.SelectedItem);
							break;
						}
					}
				}
			}
		}

		private void frm_DetailWindowClosing(object sender, ClosingDetailWindowEventArgs e)
		{
			NotifySelection();
		}

		#endregion// Private Methods
	}
}
