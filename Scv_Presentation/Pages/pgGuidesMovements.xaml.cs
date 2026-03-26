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
using Scv_Entities;
using Scv_Model;
using Scv_Dal;
using Telerik.Windows.Controls;
using System.Data;
using Scv_Model.Common;
using System.ComponentModel;
using Telerik.Windows;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgGuidesMovements.xaml
	/// </summary>
	public partial class pgGuidesMovements : BaseContentPage, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events




		#region Private Fields

		private int MasterID;

		Movimento_Dal dal = new Movimento_Dal();

		List<V_Mandato> masterTable = null;

		private int selectedMovementType = 0;

		private List<LK_TipoMovimento> availableMovementTypes = null;

		#endregion// Private Fields




		#region Public Properties

		public int SelectedMovementTypeID
		{
			get
			{
				return selectedMovementType;
			}
			set { selectedMovementType = value; OnPropertyChanged(this, "SelectedMovementTypeID"); }
		}

		public List<LK_TipoMovimento> AvailableMovementTypes
		{
			get
			{
				if (availableMovementTypes == null)
				{
					LK_TipoMovimento_Dal dal = new LK_TipoMovimento_Dal();
					availableMovementTypes = dal.GetList().Where(x => x.Fl_Attivo == true && x.Fl_Articolo == false).ToList();
					LK_TipoMovimento obj = new LK_TipoMovimento();
					obj.Id_TipoMovimento = 0;
					obj.Descrizione = string.Empty;
					availableMovementTypes.Insert(0, obj);
				}
				return availableMovementTypes;
			}
		}

		public List<V_Mandato> MasterTable
		{
			get
			{
				if (masterTable == null)
					masterTable = new List<V_Mandato>();
				return masterTable;
			}
			set { masterTable = value; OnPropertyChanged(this, "MasterTable"); }
		}

		#endregion// Public Properties




		#region Constructors

		public pgGuidesMovements()
		{
			InitializeComponent();

			Application.Current.MainWindow.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);

			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgUsers_CommandEvent);

			btnFilter.Click += new RoutedEventHandler(btnFilter_Click);
			btnCancel.Click += new RoutedEventHandler(btnCancel_Click);

			btnCancelDate1.Click += new RoutedEventHandler(btnCancelDate1_Click);
			btnCancelDate2.Click += new RoutedEventHandler(btnCancelDate2_Click);

			cmbTipoMovimento.DataContext = this;

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.IsReadOnly = true;
			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			grdMaster.MouseDoubleClick += new MouseButtonEventHandler(grdMaster_MouseDoubleClick);
			grdMaster.Grouped += new EventHandler<GridViewGroupedEventArgs>(grdMaster_Grouped);
			grdMaster.DataContext = this;

			gridPager.PageIndexChanged += new EventHandler<PageIndexChangedEventArgs>(gridPager_PageIndexChanged);

			dtpDate1.SelectionChanged += new SelectionChangedEventHandler(dtpDate1_SelectionChanged);
			dtpDate2.SelectionChanged += new SelectionChangedEventHandler(dtpDate2_SelectionChanged);

			dtpDate1.SelectedDate = DateTime.Now.Date;
			DoFilter();

			//filtri
			Filter.SetFilter("Fl_Articolo", Utilities.ValueType.Bool, false);
			Filter.AddSortField("Dt_Movimento");
			Filter.SortDirection = SortDirection.DESC;
			gridPager.PageSize = Helper_Dal.GetOptimalGridRows(false);

			BindMaster();
		}

		#endregion// Constructors




		#region Event Handlers

		void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			gridPager.PageSize = Helper_Dal.GetOptimalGridRows(true);
			BindMaster();
		}

		void pgUsers_CommandEvent(ContentPageCommandEventArgs e)
		{
			switch (e.CommandType)
			{
				case ContentPageCommandType.New:
					DoNewItem();
					break;

				case ContentPageCommandType.Open:
					DoOpenItems();
					break;

				case ContentPageCommandType.Delete:
					if (MessageBox.Show("Eliminare i movimenti selezionati? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						DoDeleteItems();
						BindMaster();
						NotifySelection();
					}
					break;

				case ContentPageCommandType.Print:
					switch (e.CommandArgument)
					{
						case "print":
							List<int> selectedIDs = new List<int>();
							foreach (V_Mandato mid in grdMaster.SelectedItems)
								selectedIDs.Add((int)mid.Identificativo);
							DoPrint(selectedIDs);
							break;

						case "printall":
							DoPrint(null);
							break;
					}
					break;
			}
		}

		void frm_Closed(object sender, EventArgs e)
		{
			BindMaster();
			NotifySelection();
		}

		private void grdMaster_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
			{
				if (grd.SelectedItems.Count > 0)
					MasterID = (int)((V_Mandato)grd.SelectedItems[0]).Id_Movimento;

				NotifySelection();
			}
		}

		private void grdMaster_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//RadGridView grd = sender as RadGridView;
			//if (grd != null)
			//    if (grd.SelectedItems.Count > 0)
			//        DoOpenItem(((V_Mandato)grd.SelectedItems[0]), WindowStartupLocation.CenterScreen);
		}

		private void gridPager_PageIndexChanged(object sender, PageIndexChangedEventArgs e)
		{
			//FillEmptyRows(e.NewPageIndex, ((RadDataPager)sender).PageSize);
		}

		void grdMaster_Grouped(object sender, GridViewGroupedEventArgs e)
		{
			grdMaster.CollapseAllGroups();
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
					BindMaster();
				}
			}
		}

		private void PrintSelected(object sender, EventArgs e)
		{
			Button btn = sender as Button;
			if (btn != null)
			{
				int ID = int.Parse(btn.CommandParameter.ToString().Substring(0, 8));

				Movimento_Dal mDal = new Movimento_Dal();
				Parametri_Dal pDal = new Parametri_Dal();

				int count = 0;
				List<MethodArgument> args = new List<MethodArgument>();
				MethodArgument arg = null;
				List<object> argValues = null;

				arg = new MethodArgument();
				arg.Field = "Fl_Articolo";
				arg.ValueType = Utilities.ValueType.Bool;
				argValues = new List<object>();
				argValues.Add(false);
				arg.Values = argValues;
				args.Add(arg);

				arg = new MethodArgument();
				arg.Field = "Identificativo";
				arg.ValueType = Utilities.ValueType.Bool;
				argValues = new List<object>();
				argValues.Add(ID);
				arg.Values = argValues;
				args.Add(arg);

				string[] sort = new string[] { "Dt_Movimento" };

				List<V_Mandato> list = mDal.GetFilteredList_V_Mandato(args, sort, "desc", 0, 0, out count)/*.Where(x => x.Identificativo == ID).ToList();*/ ;
				BasePrintPage wnd = new wndPrintGuideMovement();
				wnd.PrintGuidesMovementsArgs.MainTotal = list.Sum(x => x.PrezzoVendita).ToString();
				wnd.PrintGuidesMovementsArgs.CompensoOrarioGuida = pDal.GetItem("compenso_guida").Valore;
				wnd.DsGuidesMovements = list;
				wnd.Show();
			}
		}

		void btnFilter_Click(object sender, RoutedEventArgs e)
		{
			if (dtpDate1.SelectedDate != null && dtpDate2.SelectedDate != null)
			{
				if (dtpDate1.SelectedDate > dtpDate2.SelectedDate)
				{
					MessageBox.Show("La data 'Dal' non può essere maggiore della data 'Al'.");
					return;
				}
			}
			DoFilter();
		}

		void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DoCancelFilter();
			DoFilter();
		}

		void btnCancelDate1_Click(object sender, RoutedEventArgs e)
		{
			dtpDate1.SelectedDate = null;
			DoFilter();
		}

		void btnCancelDate2_Click(object sender, RoutedEventArgs e)
		{
			dtpDate2.SelectedDate = null;
			DoFilter();
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

		#endregion// Event Handlers




		#region Context Menu

		private void contextMenu_Opening(object sender, RoutedEventArgs e)
		{
			int selectedItems = grdMaster.SelectedItems.Count;
			bool printable = false;

			mnuPrint.IsEnabled = false;
			//mnuDelete.IsEnabled = false;

			foreach (V_Mandato mm in grdMaster.SelectedItems)
				if (mm.Id_Movimento > 0)
					printable = true;

			if (printable)
			{
				mnuPrint.IsEnabled = true;
				//mnuDelete.IsEnabled = true;
			}
		}

		private void contextMenu_Click(object sender, RadRoutedEventArgs e)
		{
			switch (((RadMenuItem)(e.Source)).CommandParameter.ToString())
			{
				case "print":
					List<int> selectedIDs = new List<int>();
					foreach (V_Mandato mid in grdMaster.SelectedItems)
						selectedIDs.Add((int)mid.Identificativo);
					DoPrint(selectedIDs);
					break;
				case "delete":
					if (MessageBox.Show("Eliminare i movimenti selezionati? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						DoDeleteItems();
						BindMaster();
						NotifySelection();
					}

					break;
			}
		}

		#endregion// Context Menu




		#region Private Methods

		private void BindMaster()
		{
			int count = 0;
			MasterTable = dal.GetFilteredList_V_Mandato(Filter.Args, Filter.Sort, Filter.SortDirection.ToString(), Filter.PageSize, Filter.PageNumber, out count);
		}

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				if ((int)((V_Mandato)grdMaster.SelectedItems[i]).Id_Movimento > 0)
					args.SelectedIDs.Add((int)((V_Mandato)grdMaster.SelectedItems[i]).Id_Movimento);

			OnItemSelectionEvent(args);
		}

		private void DoNewItem()
		{
			List<int> allowedPaymentTypeIDs = new List<int>() { 3, 4, 5 };
			BaseDetailPage frm = new wndGuideMovementDetail(0, allowedPaymentTypeIDs);
			frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			frm.ShowDialog();
			BindMaster();
		}

		private void DoOpenItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoOpenItem(((V_Mandato)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
		}

		private void DoOpenItem(V_Mandato obj, WindowStartupLocation startupLocation)
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
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoDeleteItems()
		{
			string message = string.Empty;

			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					message = DoDeleteItem(((V_Mandato)grdMaster.SelectedItems[i]));

			if (message.Length > 0)
			{
				message = "Non essendo stati registrati in data odierna, non è stato possibile eliminare i seguenti movimenti:" + message;
				MessageBox.Show(message);
			}

		}

		private string DoDeleteItem(V_Mandato obj)
		{
			string message = string.Empty;

			if (obj.Dt_Movimento.ToShortDateString() != DateTime.Now.ToShortDateString())
			{
				message += "\n" + obj.Dt_Movimento.ToShortDateString();
			}
			else
			{

				Movimento_Dal dalMovement = new Movimento_Dal();
				LK_TipoMovimento_Dal dalMovementType = new LK_TipoMovimento_Dal();
				try
				{
					BaseFilter filter = new BaseFilter();
					int count = 0;
					filter.SetFilter("Identificativo", Utilities.ValueType.Int, obj.Identificativo);
					List<Movimento> objList = dalMovement.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count);
					LK_TipoMovimento tm = dalMovementType.GetItem(obj.Id_TipoMovimento);
					if (objList != null && tm != null)
						dalMovement.DeleteObject(objList, tm);
				}
				catch (UpdateException e)
				{
					MessageBox.Show("Impossibile eliminare: fra i record selezionati, alcuni sono associati a uno o più buste paga.\nEliminare le buste paga associate ai movimenti selezionati, quindi riprovare", "Errore eliminazione", MessageBoxButton.OK);
				}
			}
			return message;
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
				List<V_Mandato> ds = new List<V_Mandato>();

				int count = 0;
				BaseFilter printFilter = new BaseFilter();
				foreach (MethodArgument arg in Filter.Args)
					printFilter.SetFilter(arg.Field, arg.ValueType, arg.Values, arg.SQLOperator);

				List<V_Mandato> printList = new List<V_Mandato>();
				printList.AddRange(dal.GetFilteredList_V_Mandato(printFilter.Args, Filter.Sort, Filter.SortDirection.ToString(), 0, 0, out count));

				foreach (V_Mandato item in printList)
					if ((selectedIDs != null && selectedIDs.Contains((int)item.Identificativo)) || selectedIDs == null)
						ds.Add(item);

				Parametri_Dal pDal = new Parametri_Dal();

				frm = new wndPrintGuidesMovements();
				frm.Name = windowName;
				frm.DsGuidesMovements = ds;
				frm.PrintGuidesMovementsArgs.MainTotal = ds.Sum(x => x.PrezzoVendita).ToString();
				frm.PrintGuidesMovementsArgs.CompensoOrarioGuida = pDal.GetItem("compenso_guida").Valore;
				frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoFilter()
		{
			//Filtro su date
			List<object> dateValues = new List<object>();
			if (dtpDate1.SelectedDate != null)
				dateValues.Add(dtpDate1.SelectedDate);
			if (dtpDate2.SelectedDate != null)
				dateValues.Add(dtpDate2.SelectedDate);

			Filter.RemoveFilter("Dt_Movimento");

			if (dtpDate1.SelectedDate != null && dtpDate2.SelectedDate == null)
				Filter.SetFilter("Dt_Movimento", Utilities.ValueType.DateTime, dtpDate1.SelectedDate, Utilities.SQLOperator.GreaterThanEqual);

			if (dtpDate1.SelectedDate == null && dtpDate2.SelectedDate != null)
				Filter.SetFilter("Dt_Movimento", Utilities.ValueType.DateTime, dtpDate2.SelectedDate, Utilities.SQLOperator.LessThanEqual);

			if (dtpDate1.SelectedDate != null && dtpDate2.SelectedDate != null)
				Filter.SetFilter("Dt_Movimento", Utilities.ValueType.DateTime, dateValues, Utilities.SQLOperator.Between);

			//Filtro su tipo movimento
			if (SelectedMovementTypeID != 0)
				Filter.SetFilter("Id_TipoMovimento", Utilities.ValueType.Int, SelectedMovementTypeID);
			else
				Filter.RemoveFilter("Id_TipoMovimento");

			BindMaster();

		}

		private void DoCancelFilter()
		{
			dtpDate1.SelectedDate = null;
			dtpDate2.SelectedDate = null;
			SelectedMovementTypeID = 0;
		}

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		private void FillEmptyRows(int pageIndex, int pageSize)
		{
			int populatedRows = MasterTable.Count - (pageIndex * pageSize);
			for (int i = populatedRows; i < pageSize; i++)
				((List<V_Mandato>)grdMaster.ItemsSource).Add(new V_Mandato());
		}

		#endregion// Private Methods
	}
}
