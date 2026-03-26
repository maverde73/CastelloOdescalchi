using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Scv_Entities;
using Scv_Model;
using Scv_Dal;
using Telerik.Windows.Controls;
using Scv_Model.Common;
using Telerik.Windows.Controls.GridView;
using System.ComponentModel;
using Telerik.Windows;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgInvoices.xaml
	/// </summary>
	public partial class pgInvoices : BaseContentPage, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events



		#region Variables

		private int MasterID = 0;

		private Movimento_Dal dal = new Movimento_Dal();

		private List<Invoice> masterTable = new List<Invoice>();

		private List<YearItem> years = null;

		private List<MonthItem> months = null;

		private YearItem selectedYear = null;

		private MonthItem selectedMonth = null;

		private int selectedStoreID = 0;

		private List<EsercizioVendita> availableStores = null;

		#endregion// Variables



		#region Public Properties

		public List<YearItem> Years
		{
			get
			{
				if (years == null)
					years = Helper_Dal.GetYears(2000, DateTime.Now.Year);
				return years;
			}
		}

		public List<MonthItem> Months
		{
			get
			{
				if (months == null)
					months = Helper_Dal.GetMonths();
				return months;
			}
		}

		public YearItem SelectedYear
		{
			get
			{
				if (selectedYear == null)
					selectedYear = new YearItem();
				return selectedYear;
			}
			set { selectedYear = value; OnPropertyChanged(this, "SelectedYear"); }
		}

		public MonthItem SelectedMonth
		{
			get
			{
				if (selectedMonth == null)
					selectedMonth = new MonthItem();
				return selectedMonth;
			}
			set { selectedMonth = value; OnPropertyChanged(this, "SelectedMonth"); }
		}

		public int SelectedStoreID
		{
			get
			{
				return selectedStoreID;
			}
			set { selectedStoreID = value; OnPropertyChanged(this, "SelectedStoreID"); }
		}

		public List<EsercizioVendita> AvailableStores
		{
			get
			{
				if (availableStores == null)
				{
					EsercizioVendita_Dal dal = new EsercizioVendita_Dal();
					availableStores = dal.GetList();
					EsercizioVendita obj = new EsercizioVendita();
					obj.Id_EsercizioVendita = 0;
					obj.Descrizione = "Tutti";
					availableStores.Insert(0, obj);
				}
				return availableStores;
			}
		}

		public List<Invoice> MasterTable
		{
			get
			{
				if (masterTable == null)
					masterTable = new List<Invoice>();
				return masterTable;
			}
			set { masterTable = value; OnPropertyChanged(this, "MasterTable"); }
		}

		#endregion// Public Properties



		#region Constructors

		public pgInvoices()
		{
			InitializeComponent();

			Application.Current.MainWindow.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);

			//filtri
			Filter.AddSortField("Dt_Fattura");
			Filter.SortDirection = SortDirection.ASC;
			gridPager.PageSize = Helper_Dal.GetOptimalGridRows(true);

			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgUsers_CommandEvent);

			SelectedYear.PropertyChanged += new PropertyChangedEventHandler(SelectedYear_PropertyChanged);
			SelectedMonth.PropertyChanged += new PropertyChangedEventHandler(SelectedMonth_PropertyChanged);

			btnFilter.Click += new RoutedEventHandler(btnFilter_Click);
			btnCancel.Click += new RoutedEventHandler(btnCancel_Click);

			btnInvoiceDates.Click += new RoutedEventHandler(btnInvoiceDates_Click);

			chkAll.Click += new RoutedEventHandler(chkAll_Click);

			SelectedYear.Number = DateTime.Now.Year;
			SelectedMonth.Number = DateTime.Now.Month;

			cmbYear.DataContext = this;
			cmbMonth.DataContext = this;
			cmbStore.DataContext = this;

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			grdMaster.RowDetailsVisibilityChanged += new EventHandler<GridViewRowDetailsEventArgs>(grdMaster_RowDetailsVisibilityChanged);
			grdMaster.IsReadOnly = true;
			grdMaster.DataContext = this;

			gridPager.PageIndexChanged += new EventHandler<PageIndexChangedEventArgs>(gridPager_PageIndexChanged);

		}

		#endregion// Constructors



		#region Events

		void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			gridPager.PageSize = Helper_Dal.GetOptimalGridRows(true);
			BindMaster(SelectedYear.Number, SelectedMonth.Number);
		}

		void pgUsers_CommandEvent(ContentPageCommandEventArgs e)
		{
			switch (e.CommandType)
			{
				case ContentPageCommandType.Open:
					DoOpenItems();
					break;

				case ContentPageCommandType.Print:
					switch (e.CommandArgument)
					{
						case "printAll":
							DoPrint(null);
							break;

						case "printSelected":
							List<int> selectedIDs = new List<int>();
							foreach (Invoice mid in grdMaster.SelectedItems)
								if (mid.Id_Fattura != null)
									selectedIDs.Add((int)mid.Id_Fattura);
							DoPrint(selectedIDs);
							break;

						case "printSintesi":
							DoPrintSintesi();
							break;

					}
					break;
			}
		}

		void grdMaster_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
			{
				if (grd.SelectedItems.Count > 0)
					if (((Invoice)grd.SelectedItems[0]).Id_Fattura != null)
						MasterID = (int)((Invoice)grd.SelectedItems[0]).Id_Fattura;

				NotifySelection();
			}
		}

		private void grdMaster_RowLoaded(object sender, RowLoadedEventArgs e)
		{
			Invoice mm = e.DataElement as Invoice;
			if (mm != null && mm.Id_Fattura == null)
			{
				if (e.Row.Cells.Count > 0)
					e.Row.Cells[0].Visibility = System.Windows.Visibility.Hidden;
				if (e.Row.Cells.Count > 1)
					e.Row.Cells[1].Visibility = System.Windows.Visibility.Hidden;
				if (e.Row.Cells.Count > 6)
					e.Row.Cells[6].Visibility = System.Windows.Visibility.Hidden;
			}
		}

		private void grdMaster_RowDetailsVisibilityChanged(object sender, GridViewRowDetailsEventArgs e)
		{
			RadGridView grd = e.DetailsElement as RadGridView;
			if (grd != null)
				grd.DataContext = ((Invoice)e.Row.Item).Details;
		}

		private void gridPager_PageIndexChanged(object sender, PageIndexChangedEventArgs e)
		{
			//FillEmptyRows(e.NewPageIndex, ((RadDataPager)sender).PageSize);
		}

		void frm_Closed(object sender, EventArgs e)
		{
			BindMaster(SelectedYear.Number, SelectedMonth.Number);
			NotifySelection();
		}

		private void DeleteSelected(object sender, EventArgs e)
		{
			Button btn = sender as Button;
			if (btn != null)
			{
				if (MessageBox.Show("Eliminare il movimento " + btn.CommandParameter.ToString() + "? L'operazione non potrà essere annullata!", "Eliminazione", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
				{
					int ID = int.Parse(btn.CommandParameter.ToString().Substring(0, 8));

					Movimento_Dal dal = new Movimento_Dal();
					List<Movimento> list = dal.GetMovementByIdentifier(ID);
					if (list != null && list.Count > 0)
					{
						LK_TipoMovimento_Dal dalTM = new LK_TipoMovimento_Dal();
						LK_TipoMovimento tm = dalTM.GetSingleItem(list[0].Id_TipoMovimento)[0];

						dal.DeleteObject(list, tm);
					}
					BindMaster(SelectedYear.Number, SelectedMonth.Number);
				}
			}
		}

		void SelectedYear_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(sender, "SelectedYear");
		}

		void SelectedMonth_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(sender, "SelectedMonth");
		}

		void btnFilter_Click(object sender, RoutedEventArgs e)
		{
			DoFilter();
		}

		void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DoCancelFilter();
			DoFilter();
		}

		void btnInvoiceDates_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Registrare le date di pagamento per le fatture? L'azione non può essere annullata!", "Registrazione date pagamento fatture", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
				return;

			DoRecordPaymentDates();
			BindMaster(SelectedYear.Number, SelectedMonth.Number);
		}

		void chkAll_Click(object sender, RoutedEventArgs e)
		{
			if (chkAll.IsChecked == true)
				grdMaster.SelectAll();
			else
				grdMaster.SelectedItems.Clear();
		}

		#endregion// Events



		#region Context Menu

		private void contextMenu_Opening(object sender, RoutedEventArgs e)
		{
			int selectedItems = grdMaster.SelectedItems.Count;
			bool printable = false;

			mnuPrint.IsEnabled = false;

			foreach (Invoice mm in grdMaster.SelectedItems)
				if (mm.Id_Fattura != null)
					printable = true;

			if (printable)
				mnuPrint.IsEnabled = true;
		}

		private void contextMenu_Click(object sender, RadRoutedEventArgs e)
		{
			switch (((RadMenuItem)(e.Source)).CommandParameter.ToString())
			{
				case "printAll":
					DoPrint(null);
					break;

				case "printSelected":
					List<int> selectedIDs = new List<int>();
					foreach (Invoice mid in grdMaster.SelectedItems)
						selectedIDs.Add((int)mid.Id_Fattura);
					DoPrint(selectedIDs);
					break;
			}
		}

		#endregion// Context Menu



		#region Private Methods

		private void BindMaster(int yearNumber, int monthNumber)
		{
			if (yearNumber > 0 && monthNumber > 0)
			{
				MasterTable = dal.GetInvoiceList(Filter, yearNumber, monthNumber);
			}
		}

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				if (((Invoice)grdMaster.SelectedItems[i]).Id_Fattura != null)
					args.SelectedIDs.Add((int)((V_Movimento)grdMaster.SelectedItems[i]).Id_Movimento);

			OnItemSelectionEvent(args);
		}

		private void DoOpenItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoOpenItem(((V_Movimento)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
		}

		private void DoOpenItem(V_Movimento obj, WindowStartupLocation startupLocation)
		{
			bool open = true;
			string windowName = string.Format("Guide_{0}", obj.Id_Movimento.ToString());
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
				frm = new wndUserDetail(obj.Id_Movimento);
				frm.User = User;
				frm.Name = windowName;
				frm.WindowStartupLocation = startupLocation;
				//frm.Closed += new EventHandler<Telerik.Windows.Controls.WindowClosedEventArgs>(frm_Closed);
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoPrint(List<int> selectedIDs)
		{
			bool open = true;
			string windowName = "Fatture";
			BasePrintPage frm = null;
			Movimento_Dal dal = new Movimento_Dal();

			foreach (Window wnd in Application.Current.Windows)
			{
				if (wnd.Name == windowName && wnd.IsVisible)
				{
					frm = (BasePrintPage)wnd;
					frm.Focus();
					open = false;
				}
			}
			if (open)
			{
				Movimento_Dal dalMovement = new Movimento_Dal();
				List<Invoice> ds = new List<Invoice>();
				List<Invoice> m = new List<Invoice>(); ;
				BaseFilter filter = new BaseFilter();

				List<Invoice> printList = new List<Invoice>();
				BaseFilter printFilter = new BaseFilter();
				foreach (MethodArgument arg in Filter.Args)
					printFilter.SetFilter(arg.Field, arg.ValueType, arg.Values, arg.SQLOperator);

				printFilter.AddSortField("Dt_Fattura");
				printFilter.SortDirection = SortDirection.ASC;
				printList.AddRange(dal.GetInvoiceList(printFilter, selectedYear.Number, SelectedMonth.Number));

				foreach (Invoice item in printList)
					if ((selectedIDs != null && item.Id_Fattura != null && selectedIDs.Contains((int)item.Id_Fattura)) || selectedIDs == null)
						if (item.Id_Fattura != null)
							ds.AddRange(dalMovement.GetInvoice((int)item.Id_Fattura));

				if (ds.Count == 0)
				{
					MessageBox.Show("Nessuna fattura selezionata.");
					return;
				}

				frm = new wndPrintInvoices();
				frm.Name = windowName;
				frm.DsInvoices = ds;
				frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoPrintSintesi()
		{
			bool open = true;
			string windowName = "SintesiVenditeArticoli";
			BasePrintPage frm = null;
			Movimento_Dal dal = new Movimento_Dal();

			foreach (Window wnd in Application.Current.Windows)
			{
				if (wnd.Name == windowName && wnd.IsVisible)
				{
					frm = (BasePrintPage)wnd;
					frm.Focus();
					open = false;
				}
			}
			if (open)
			{
				List<V_SintesiArticoli> printList = new Movimento_Dal().GetSintesiArticoli(SelectedYear.Number);
				PrintMovementArgs args = new PrintMovementArgs();
				args.ReportLabel = "Totale Vendite PUBBLICAZIONI " + SelectedYear.Number.ToString();

				frm = new wndPrintSintesiArticoli();
				frm.Name = windowName;
				frm.DsSintesiArticoli = printList;
				frm.PrintMovementArgs = args;
				frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoFilter()
		{

			//Filtro su esercizio vendita
			if (SelectedStoreID != 0)
				Filter.SetFilter("Id_EsercizioVendita", Utilities.ValueType.Int, SelectedStoreID);
			else
				Filter.RemoveFilter("Id_EsercizioVendita");

			BindMaster(SelectedYear.Number, SelectedMonth.Number);

		}

		private void DoCancelFilter()
		{
			SelectedStoreID = 0;
		}

		private void DoRecordPaymentDates()
		{
			Fattura_Dal dal = new Fattura_Dal();
			dal.RecordPaymentDates(MasterTable);
		}

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

			switch (propertyName)
			{
				case "SelectedYear":
				case "SelectedMonth":
					BindMaster(SelectedYear.Number, SelectedMonth.Number);
					break;
			}

		}

		private void FillEmptyRows(int pageIndex, int pageSize)
		{
			int populatedRows = MasterTable.Count - (pageIndex * pageSize);
			for (int i = populatedRows; i < pageSize; i++)
				((List<Invoice>)grdMaster.ItemsSource).Add(new Invoice());
		}

		#endregion// Private Methods

	}
}
