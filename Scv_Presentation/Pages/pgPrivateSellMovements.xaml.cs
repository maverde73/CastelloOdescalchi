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
using System.Data;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgWarehouseMovements.xaml
	/// </summary>
	public partial class pgPrivateSellMovements : BaseContentPage, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events



		#region Variables

		private int MasterID;
		private Movimento_Dal dal = new Movimento_Dal();
		private List<int> selectedMovementType = null;
		private List<LK_TipoMovimento> availableMovementTypes = null;
		private List<MovementMaster> masterList = null;

		#endregion// Variables



		#region Public Properties

		public List<int> SelectedMovementTypeID
		{
			get
			{
				if (selectedMovementType == null)
					selectedMovementType = new List<int>();
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
					availableMovementTypes = dal.GetList().Where(x => x.Fl_Attivo == true && x.Fl_Articolo == true).ToList();
					LK_TipoMovimento obj = new LK_TipoMovimento();
					obj.Id_TipoMovimento = 0;
					obj.Descrizione = string.Empty;
					availableMovementTypes.Insert(0, obj);
				}
				return availableMovementTypes;
			}
		}

		public List<MovementMaster> MasterList
		{
			get
			{
				if (masterList == null)
					masterList = new List<MovementMaster>();
				return masterList;
			}
			set { masterList = value; OnPropertyChanged(this, "MasterList"); }
		}

		#endregion// Public Properties



		#region Constructors

		public pgPrivateSellMovements()
		{
			InitializeComponent();

			Application.Current.MainWindow.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);

			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgUsers_CommandEvent);

			btnFilter.Click += new RoutedEventHandler(btnFilter_Click);
			btnCancel.Click += new RoutedEventHandler(btnCancel_Click);

			btnCancelDate1.Click += new RoutedEventHandler(btnCancelDate1_Click);
			btnCancelDate2.Click += new RoutedEventHandler(btnCancelDate2_Click);

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.IsReadOnly = true;
			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			grdMaster.MouseDoubleClick += new MouseButtonEventHandler(grdMaster_MouseDoubleClick);
			grdMaster.RowDetailsVisibilityChanged += new EventHandler<GridViewRowDetailsEventArgs>(grdMaster_RowDetailsVisibilityChanged);
			grdMaster.DataContext = this;

			gridPager.PageIndexChanged += new EventHandler<PageIndexChangedEventArgs>(gridPager_PageIndexChanged);

			dtpDate1.SelectionChanged += new SelectionChangedEventHandler(dtpDate1_SelectionChanged);
			dtpDate2.SelectionChanged += new SelectionChangedEventHandler(dtpDate2_SelectionChanged);

			dtpDate1.SelectedDate = DateTime.Now.Date;

			//filtri
			Filter.SetFilter("Fl_Articolo", Utilities.ValueType.Bool, true);
			Filter.AddSortField("Dt_Movimento");
			Filter.SortDirection = SortDirection.DESC;
			gridPager.PageSize = Helper_Dal.GetOptimalGridRows(false);
			SelectedMovementTypeID.Add(3);
			SelectedMovementTypeID.Add(4);			
			DoFilter();

			BindMaster();
		}

		#endregion// Constructors



		#region Events Handlers

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
							foreach (MovementMaster mid in grdMaster.SelectedItems)
								selectedIDs.Add(mid.ID);
							DoPrint(selectedIDs);
							break;

						case "printall":
							DoPrint(null);
							break;

						case "printlist":
							DoPrintList(null);
							break;

					}
					break;

				case ContentPageCommandType.Update:
					switch (e.CommandArgument)
					{
						case "changemovementpaymenttype":
							DoChangeMovementPaymentTypes();
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
					MasterID = (int)((MovementMaster)grd.SelectedItems[0]).ID;

				NotifySelection();
			}
		}

		private void grdMaster_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//RadGridView grd = sender as RadGridView;
			//if (grd != null)
			//    if (grd.SelectedItems.Count > 0)
			//        DoOpenItem(((V_Movimento)grd.SelectedItems[0]), WindowStartupLocation.CenterScreen);
		}

		private void grdMaster_RowDetailsVisibilityChanged(object sender, GridViewRowDetailsEventArgs e)
		{
			RadGridView grd = e.DetailsElement as RadGridView;
			if (grd != null)
			{
				int id = ((MovementMaster)e.Row.Item).ID;
				foreach (MovementMaster mm in MasterList)
				{
					if (mm.ID == id)
					{
						grd.DataContext = mm.Details;
						return;
					}
				}
			}
		}

		private void grdMaster_RowLoaded(object sender, RowLoadedEventArgs e)
		{
			MovementMaster mm = e.DataElement as MovementMaster;
			if (mm != null)
				if (mm.ID == 0)
				{
					if (e.Row.Cells.Count > 0)
						e.Row.Cells[0].Visibility = System.Windows.Visibility.Hidden;
					if (e.Row.Cells.Count > 1)
						e.Row.Cells[1].Visibility = System.Windows.Visibility.Hidden;
					if (e.Row.Cells.Count > 7)
						e.Row.Cells[7].Visibility = System.Windows.Visibility.Hidden;
				}
		}

		private void gridPager_PageIndexChanged(object sender, PageIndexChangedEventArgs e)
		{
			//FillEmptyRows(e.NewPageIndex, ((RadDataPager)sender).PageSize);
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
			DoFilter();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DoCancelFilter();
			DoFilter();
		}

		private void btnCancelDate1_Click(object sender, RoutedEventArgs e)
		{
			dtpDate1.SelectedDate = null;
			DoFilter();
		}

		private void btnCancelDate2_Click(object sender, RoutedEventArgs e)
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

		#endregion// Events Handlers



		#region Context Menu

		private void contextMenu_Opening(object sender, RoutedEventArgs e)
		{
			int selectedItems = grdMaster.SelectedItems.Count;
			bool printable = false;

			MovementMaster m = grdMaster.SelectedItem as MovementMaster;
			printable = m != null ? m.ID > 0 : false;

			mnuPrint.IsEnabled = false;
			//mnuDelete.IsEnabled = false;
			mnuChangeMovementPaymentType.IsEnabled = false;

			if (printable)
			{
				mnuPrint.IsEnabled = true;
				//mnuDelete.IsEnabled = true;
				mnuChangeMovementPaymentType.IsEnabled = m.IsMoney;
			}
		}

		private void contextMenu_Click(object sender, RadRoutedEventArgs e)
		{
			switch (((RadMenuItem)(e.Source)).CommandParameter.ToString())
			{
				case "print":
					List<int> selectedIDs = new List<int>();
					foreach (MovementMaster mid in grdMaster.SelectedItems)
						selectedIDs.Add(mid.ID);
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

				case "changeMovementPaymentType":
					DoChangeMovementPaymentTypes();
					BindMaster();
					NotifySelection();
					break;
			}
		}

		#endregion// Context Menu




		#region Private Methods

		private void BindMaster()
		{
			int count = 0;
			MasterList = dal.GetFilteredList_MovimentoMasterDetail(Filter.Args, Filter.Sort, Filter.SortDirection.ToString(), Filter.PageSize, Filter.PageNumber, out count)
				.Where(x => x.MovementTypeID == SelectedMovementTypeID[0] || x.MovementTypeID == SelectedMovementTypeID[1])
				.ToList()
				;
				
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
			List<int> allowedMovementTypesIDs = new List<int>() { 3, 4 };
			List<int> allowedPaymentTypeIDs = new List<int>() { 3, 4, 5 };

			BaseDetailPage frm = new wndMovementDetail(0, 3, false, allowedMovementTypesIDs, allowedPaymentTypeIDs);
			frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			frm.User = User;
			frm.ShowDialog();
			BindMaster();
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
					frm.User = User;
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
					message = DoDeleteItem(((MovementMaster)grdMaster.SelectedItems[i]));

			if (message.Length > 0)
			{
				message = "Non essendo stati registrati in data odierna, non è stato possibile eliminare i seguenti movimenti:" + message;
				MessageBox.Show(message);
			}
		}

		private string DoDeleteItem(MovementMaster obj)
		{
			string message = string.Empty;

			if (obj.Date.Value.ToShortDateString() != DateTime.Now.ToShortDateString())
			{
				message += "\n" + obj.Date.Value.ToShortDateString();
			}
			else
			{

				Movimento_Dal dalMovement = new Movimento_Dal();
				LK_TipoMovimento_Dal dalMovementType = new LK_TipoMovimento_Dal();
				try
				{
					BaseFilter filter = new BaseFilter();
					int count = 0;
					filter.SetFilter("Identificativo", Utilities.ValueType.Int, obj.ID);
					List<Movimento> objList = dalMovement.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count);
					LK_TipoMovimento tm = dalMovementType.GetItem(obj.MovementTypeID);
					if (objList != null && tm != null)
						dalMovement.DeleteObject(objList, tm);
				}
				catch (UpdateException e)
				{
					MessageBox.Show("Impossibile eliminare: fra i record selezionati, alcuni sono associati a uno o più fatture.\nEliminare le fatture associate ai movimenti selezionati, quindi riprovare", "Errore eliminazione", MessageBoxButton.OK);
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
				List<V_Movimento> ds = new List<V_Movimento>();
				V_Movimento m = null;

				List<MovementMaster> printList = new List<MovementMaster>();

				printList = MasterList;

				printList.ForEach(x => x.Date = x.Date.Value.Date);

				//foreach (MovementMaster item in printList)
				foreach (MovementMaster item in MasterList)
					{
					if ((selectedIDs != null && selectedIDs.Contains(item.ID)) || selectedIDs == null)
					{
						foreach (MovementDetail md in item.Details)
						{
							if (md.MovementID > 0)
							{
								m = new V_Movimento();
								m.Dt_Movimento = item.Date.Value;
								m.Articolo = md.Article;
								m.EsercizioVendita = item.Store;
								m.Id_Articolo = md.ArticleID;
								m.Id_EsercizioVendita = item.StoreID;
								m.Id_Movimento = item.ID;
								m.Id_TipoMovimento = item.MovementTypeID;
								m.Identificativo = item.ID;
								m.ISBN = md.ISBN;
								m.Nota = item.Note;
								m.NotaMovimento = item.MovementNote;
								m.Nr_Pos = item.PosNumber;
								m.Nr_Pezzi = (short)md.Quantity;
								m.PrezzoPubblico = md.PublicPrice;
								m.PrezzoVendita = md.Price;
								m.RicevutaBolla = item.Document;
								m.Sconto = item.StoreDiscount;
								m.TipoArticolo = md.ArticleType;
								m.TipoMovimento = item.MovementType;
								m.TipoPagamento = item.PaymentType;

								ds.Add(m);
							}
						}
					}
				}

				frm = new wndPrivateMovements();
				frm.Name = windowName;
				frm.DsWarehouseMovements = ds;
				frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoPrintList(List<int> selectedIDs)
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
				V_Movimento m = null;

				List<MovementMaster> printList = new List<MovementMaster>();

				printList = MasterList;

				printList.ForEach(x => x.Date = x.Date.Value.Date);

				foreach (MovementMaster item in MasterList)
				{
					if ((selectedIDs != null && selectedIDs.Contains(item.ID)) || selectedIDs == null)
					{
						foreach (MovementDetail md in item.Details)
						{
							if (md.MovementID > 0)
							{
								m = new V_Movimento();
								m.Dt_Movimento = item.Date.Value;
								m.Articolo = md.Article;
								m.EsercizioVendita = item.Store;
								m.Id_Articolo = md.ArticleID;
								m.Id_EsercizioVendita = item.StoreID;
								m.Id_Movimento = item.ID;
								m.Id_TipoMovimento = item.MovementTypeID;
								m.Identificativo = item.ID;
								m.ISBN = md.ISBN;
								m.Nota = item.Note;
								m.NotaMovimento = item.MovementNote;
								m.Nr_Pos = item.PosNumber;
								m.Nr_Pezzi = (short)md.Quantity;
								m.PrezzoPubblico = md.PublicPrice;
								m.PrezzoVendita = md.Price;
								m.RicevutaBolla = item.Document;
								m.Sconto = item.StoreDiscount;
								m.TipoArticolo = md.ArticleType;
								m.TipoMovimento = item.MovementType;
								m.TipoPagamento = item.PaymentType;

								ds.Add(m);
							}
						}
					}
				}

				frm = new wndPrivateMovementsList();
				frm.Name = windowName;
				frm.DsWarehouseMovements = ds;
				frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoChangeMovementPaymentTypes()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoChangeMovementPaymentType(((MovementMaster)grdMaster.SelectedItems[i]));
		}

		private void DoChangeMovementPaymentType(MovementMaster obj)
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
				List<int> allowedPaymentTypeIDs = new List<int>() { 3, 4, 5 };

				frm = new wndChangeMovementPaymentType(obj.ID, obj.MovementTypeID, allowedPaymentTypeIDs);
				frm.User = User;
				frm.Name = windowName;
				frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
			//if (SelectedMovementTypeID.Count > 0)
			//    Filter.SetFilter("Id_TipoMovimento", Utilities.ValueType.Int, SelectedMovementTypeID, Utilities.SQLOperator.Between);
			//else
			//    Filter.RemoveFilter("Id_TipoMovimento");

			BindMaster();

		}

		private void DoCancelFilter()
		{
			dtpDate1.SelectedDate = null;
			dtpDate2.SelectedDate = null;
			//SelectedMovementTypeID.Clear();
		}

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
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

				if (ID > 0)
				{
					Movimento_Dal dal = new Movimento_Dal();
					int count = 0;
					List<V_Movimento> list = dal.GetFilteredList_V_Movimento(null, null, null, 0, 0, out count).Where(x => x.Identificativo == ID).ToList(); ;
					BasePrintPage wnd = new wndPrintMovement();
					wnd.PrintGuidesMovementsArgs.MainTotal = list.Sum(x => x.Totale).ToString();
					wnd.DsWarehouseMovements = list;
					wnd.Show();
				}
			}
		}

		private void FillEmptyRows(int pageIndex, int pageSize)
		{
			int populatedRows = MasterList.Count - (pageIndex * pageSize);
			for (int i = populatedRows; i < pageSize; i++)
				((List<MovementMaster>)grdMaster.ItemsSource).Add(new MovementMaster());
		}

		#endregion// Private Methods

	}
}
