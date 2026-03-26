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
using Telerik.Windows;
using System.Data;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgStores.xaml
	/// </summary>
	public partial class pgStores : BaseContentPage
	{
		#region Variables

		private int MasterID;
		EsercizioVendita_Dal dal = new EsercizioVendita_Dal();

		#endregion



		#region Constructors

		public pgStores()
		{
			InitializeComponent();
			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgStores_CommandEvent);

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.IsReadOnly = true;
			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			grdMaster.MouseDoubleClick += new MouseButtonEventHandler(grdMaster_MouseDoubleClick);
			grdMaster.Grouped += new EventHandler<GridViewGroupedEventArgs>(grdMaster_Grouped);
			BindMaster();
		}

		#endregion



		#region Events

		void pgStores_CommandEvent(ContentPageCommandEventArgs e)
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
					if (MessageBox.Show("Eliminare le guide selezionate? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						DoDeleteItems();
						BindMaster();
						NotifySelection();
					}
					break;
			}
		}

		void frm_Closed(object sender, EventArgs e)
		{
			BindMaster();
			NotifySelection();
		}

		void grdMaster_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
			{
				if (grd.SelectedItems.Count > 0)
					MasterID = (int)((EsercizioVendita)grd.SelectedItems[0]).Id_EsercizioVendita;

				NotifySelection();
			}
		}

		void grdMaster_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
				if (grd.SelectedItems.Count > 0)
					DoOpenItem(((EsercizioVendita)grd.SelectedItems[0]), WindowStartupLocation.CenterScreen);
		}

		void grdMaster_Grouped(object sender, GridViewGroupedEventArgs e)
		{
			grdMaster.CollapseAllGroups();
		}

		#endregion// Events



		#region Context Menu

		private void contextMenu_Opening(object sender, RoutedEventArgs e)
		{
			int selectedItems = grdMaster.SelectedItems.Count;
			mnuOpen.IsEnabled = false;
			mnuDelete.IsEnabled = false;
			if (selectedItems > 0)
			{
				mnuOpen.IsEnabled = true;
				mnuDelete.IsEnabled = true;
			}
		}

		private void contextMenu_Click(object sender, RadRoutedEventArgs e)
		{
			switch (((RadMenuItem)(e.Source)).CommandParameter.ToString())
			{
				case "open":
					DoOpenItems();
					break;
				case "delete":
					if (MessageBox.Show("Eliminare le guide selezionate? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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
			BaseFilter filter = new BaseFilter();
			filter.AddSortField("Descrizione");
			filter.SortDirection = SortDirection.ASC;
			int count = 0;
			List<EsercizioVendita> MasterTable = dal.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count);
			grdMaster.DataContext = MasterTable;
		}

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				args.SelectedIDs.Add((int)((EsercizioVendita)grdMaster.SelectedItems[i]).Id_EsercizioVendita);

			OnItemSelectionEvent(args);
		}

		private void DoNewItem()
		{
			BaseDetailPage frm = new wndStoreDetail(0);
			frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			frm.ShowDialog();
			BindMaster();
		}

		private void DoOpenItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoOpenItem(((EsercizioVendita)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
		}

		private void DoOpenItem(EsercizioVendita obj, WindowStartupLocation startupLocation)
		{
			bool open = true;
			string windowName = string.Format("Store_{0}", obj.Id_EsercizioVendita.ToString());
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
				frm = new wndStoreDetail(obj.Id_EsercizioVendita);
				frm.User = User;
				frm.Name = windowName;
				frm.WindowStartupLocation = startupLocation;
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoDeleteItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoDeleteItem(((EsercizioVendita)grdMaster.SelectedItems[i]));
		}

		private void DoDeleteItem(EsercizioVendita obj)
		{
			EsercizioVendita_Dal dal = new EsercizioVendita_Dal();
			try
			{
				EsercizioVendita g = dal.GetItem(obj.Id_EsercizioVendita);
				if (g != null)
					dal.DeleteObject(g);
			}
			catch (UpdateException e)
			{
				MessageBox.Show("Impossibile eliminare: fra i record selezionati, alcuni sono associati a una o più visite.\nEliminare le visite associate alle guide selezionate, quindi riprovare", "Errore eliminazione", MessageBoxButton.OK);
			}
		}

		#endregion// Private Methods
	}
}
