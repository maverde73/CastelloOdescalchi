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
	/// Interaction logic for pgBills.xaml
	/// </summary>
	public partial class pgBills : BaseContentPage, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events



		#region Variables

		private int MasterID = 0;

		private Movimento_Dal dal = new Movimento_Dal();

		private List<MovementMaster> masterTable = null;

		private List<YearItem> years = null;

		private List<MonthItem> months = null;

		private YearItem selectedYear = null;

		private MonthItem selectedMonth = null;

		private int selectedStoreID = 0;

		private int selectedBillStatusID = 0;

		private List<EsercizioVendita> availableStores = null;

		private List<BillStatusItem> availableStatus = null;

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

		public int SelectedBillStatusID
		{
			get
			{
				return selectedBillStatusID;
			}
			set { selectedBillStatusID = value; OnPropertyChanged(this, "SelectedBillStatusID"); }
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

		public List<BillStatusItem> AvailableStatus
		{
			get
			{
				if (availableStatus == null)
				{
					availableStatus = new List<BillStatusItem>();
					availableStatus.Add(new BillStatusItem(1, "Non fatturate"));
					availableStatus.Add(new BillStatusItem(2, "Fatturate"));
					availableStatus.Add(new BillStatusItem(3, "Tutte"));
				}
				return availableStatus;
			}
		}

		public List<MovementMaster> MasterTable
		{
			get
			{
				if (masterTable == null)
					masterTable = new List<MovementMaster>();
				return masterTable;
			}
			set { masterTable = value; OnPropertyChanged(this, "MasterTable"); }
		}

		#endregion// Public Properties



		#region Constructors

		public pgBills()
		{
			InitializeComponent();

			Application.Current.MainWindow.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);

			//filtri
			Filter.SetFilter("Fl_Articolo", Utilities.ValueType.Bool, true);
			Filter.AddSortField("Dt_Movimento");
			Filter.SortDirection = SortDirection.DESC;
			gridPager.PageSize = Helper_Dal.GetOptimalGridRows(true);

			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgUsers_CommandEvent);

			SelectedYear.PropertyChanged += new PropertyChangedEventHandler(SelectedYear_PropertyChanged);
			SelectedMonth.PropertyChanged += new PropertyChangedEventHandler(SelectedMonth_PropertyChanged);

			btnFilter.Click += new RoutedEventHandler(btnFilter_Click);
			btnCancel.Click += new RoutedEventHandler(btnCancel_Click);

			btnInvoices.Click += new RoutedEventHandler(btnInvoices_Click);

			chkAll.Click += new RoutedEventHandler(chkAll_Click);

			SelectedBillStatusID = 1;
			SelectedYear.Number = DateTime.Now.Year;
			SelectedMonth.Number = DateTime.Now.Month;

			cmbYear.DataContext = this;
			cmbMonth.DataContext = this;
			cmbStore.DataContext = this;
			cmbBillStatus.DataContext = this;

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			grdMaster.RowDetailsVisibilityChanged += new EventHandler<GridViewRowDetailsEventArgs>(grdMaster_RowDetailsVisibilityChanged);
			grdMaster.IsReadOnly = true;
			grdMaster.DataContext = this;

			gridPager.PageIndexChanged += new EventHandler<PageIndexChangedEventArgs>(gridPager_PageIndexChanged);

		}

		#endregion// Constructors



		#region Events

		private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			gridPager.PageSize = Helper_Dal.GetOptimalGridRows(true);
			BindMaster(SelectedYear.Number, SelectedMonth.Number);
		}

		private void pgUsers_CommandEvent(ContentPageCommandEventArgs e)
		{
			switch (e.CommandType)
			{
				case ContentPageCommandType.New:
					DoNewItem();
					break;

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
							foreach (MovementMaster mid in grdMaster.SelectedItems)
								selectedIDs.Add(mid.ID);
							DoPrint(selectedIDs);
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
					MasterID = (int)((MovementMaster)grd.SelectedItems[0]).ID;

				NotifySelection();
			}
		}

		private void grdMaster_RowDetailsVisibilityChanged(object sender, GridViewRowDetailsEventArgs e)
		{
			RadGridView grd = e.DetailsElement as RadGridView;
			if (grd != null)
				grd.DataContext = ((MovementMaster)e.Row.Item).Details;
		}

		private void grdMaster_RowLoaded(object sender, RowLoadedEventArgs e)
		{
			MovementMaster mm = e.DataElement as MovementMaster;
			if (mm != null)
			{
				e.Row.Cells[1].Visibility = mm.IsFatturato ? Visibility.Hidden : Visibility.Visible;

				if (mm.ID == 0)
				{
					if (e.Row.Cells.Count > 0)
						e.Row.Cells[0].Visibility = System.Windows.Visibility.Hidden;
					if (e.Row.Cells.Count > 1)
						e.Row.Cells[1].Visibility = System.Windows.Visibility.Hidden;
				}
			}
		}

		private void gridPager_PageIndexChanged(object sender, PageIndexChangedEventArgs e)
		{
			//FillEmptyRows(e.NewPageIndex, ((RadDataPager)sender).PageSize);
		}

		private void frm_Closed(object sender, EventArgs e)
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

		private void SelectedYear_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(sender, "SelectedYear");
		}

		private void SelectedMonth_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(sender, "SelectedMonth");
		}

		private void btnFilter_Click(object sender, RoutedEventArgs e)
		{
			DoFilter();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DoCancelFilter();
			DoFilter();
		}

		private void btnInvoices_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Registrare le fatture per le bolle selezionate? L'azione non può essere annullata!", "Registrazione fatture", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
				return;

			DoCreateInvoices();
			BindMaster(SelectedYear.Number, SelectedMonth.Number);
		}

		private void chkAll_Click(object sender, RoutedEventArgs e)
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
			foreach (MovementMaster mm in grdMaster.SelectedItems)
				if (mm.ID > 0)
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
					foreach (MovementMaster mid in grdMaster.SelectedItems)
						selectedIDs.Add((int)mid.ID);
					DoPrint(selectedIDs);
					break;
			}
		}

		#endregion// Context Menu



		#region Private Methods

		private void BindMaster(int yearNumber, int monthNumber)
		{
			List<MovementMaster> countList = new List<MovementMaster>();
			if (yearNumber > 0 && monthNumber > 0)
			{
				MasterTable = dal.GetBills(Filter, yearNumber, monthNumber, SelectedBillStatusID);
			}
		}

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				if ((int)((MovementMaster)grdMaster.SelectedItems[i]).ID > 0)
					args.SelectedIDs.Add((int)((MovementMaster)grdMaster.SelectedItems[i]).ID);

			OnItemSelectionEvent(args);
		}

		private void DoNewItem()
		{
			BaseDetailPage frm = new wndMovementDetail(0, 0, false);
			frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			frm.ShowDialog();
			BindMaster(SelectedYear.Number, SelectedMonth.Number);
		}

		private void DoOpenItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoOpenItem(((MovementMaster)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
		}

		private void DoOpenItem(MovementMaster obj, WindowStartupLocation startupLocation)
		{
			bool open = true;
			string windowName = string.Format("Guide_{0}", obj.ID.ToString());
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
				frm = new wndUserDetail(obj.ID);
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
			string windowName = "Movimenti";
			BasePrintPage frm = null;
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
				List<V_Movimento> ds = new List<V_Movimento>();
				V_Movimento obj = null;

				BaseFilter printFilter = new BaseFilter();
				foreach (MethodArgument arg in Filter.Args)
					printFilter.SetFilter(arg.Field, arg.ValueType, arg.Values, arg.SQLOperator);

				List<MovementMaster> printList = new List<MovementMaster>();
				printList.AddRange(dal.GetBills(printFilter, SelectedYear.Number, SelectedYear.Number, SelectedBillStatusID));

				foreach (MovementMaster item in printList)
					if ((selectedIDs != null && selectedIDs.Contains((int)item.ID)) || selectedIDs == null)
						foreach (MovementDetail d in item.Details)
						{
							if (d.MovementID > 0)
							{
								obj = new V_Movimento();
								obj.Articolo = d.Article;
								obj.Dt_Movimento = item.Date.Value;
								obj.EsercizioVendita = item.Store;
								obj.Id_Articolo = d.ArticleID;
								obj.Id_EsercizioVendita = item.StoreID;
								obj.Id_Fattura = item.BillID;
								obj.Id_Movimento = item.ID;
								obj.Id_TipoMovimento = item.MovementTypeID;
								obj.Identificativo = item.ID;
								obj.ISBN = d.ISBN;
								obj.NotaMovimento = item.MovementNote;
								obj.Nota = item.Note;
								obj.Nr_Pezzi = (short)d.Quantity;
								obj.PrezzoPubblico = d.PublicPrice;
								obj.PrezzoVendita = d.Price;
								obj.RicevutaBolla = item.Document;
								obj.Sconto = item.StoreDiscount;
								obj.TipoArticolo = d.ArticleType;
								obj.TipoMovimento = item.MovementType;
								obj.TipoPagamento = item.PaymentType;
								obj.TotaleBolla = item.Total;
								ds.Add(obj);
							}
						}

				frm = new wndPrintDeliveredBills();
				frm.Name = windowName;
				frm.DsWarehouseMovements = ds;
				frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoFilter()
		{

			//Filtro su tipo movimento
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

		private void DoCreateInvoices()
		{
			Fattura_Dal dal = new Fattura_Dal();
			List<MovementMaster> list = new List<MovementMaster>();
			foreach (MovementMaster item in grdMaster.SelectedItems)
				if (!item.IsFatturato)
					list.Add(item);

			dal.InsertOrUpdate(list);

		}

		private void FillEmptyRows(int pageIndex, int pageSize)
		{
			int populatedRows = MasterTable.Count - (pageIndex * pageSize);
			for (int i = populatedRows; i < pageSize; i++)
				((List<MovementMaster>)grdMaster.ItemsSource).Add(new MovementMaster());
		}

		#endregion// Private Methods


	}
}
