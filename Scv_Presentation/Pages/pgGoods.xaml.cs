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
using Scv_Entities;
using Telerik.Windows.Controls;
using System.Data;
using Telerik.Windows;
using Telerik.Windows.Controls.GridView;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgPersonnel.xaml
	/// </summary>
	public partial class pgGoods : BaseContentPage
	{
		#region Variables

		private int MasterID = 0;
		Articolo_Dal dal = new Articolo_Dal();

		#endregion// Variables



		#region Constructors

		public pgGoods()
		{
			InitializeComponent();
			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgGuides_CommandEvent);

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.IsReadOnly = true;
			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			//grdMaster.MouseDoubleClick += new MouseButtonEventHandler(grdMaster_MouseDoubleClick);
			grdMaster.RowActivated += new EventHandler<RowEventArgs>(grdMaster_RowActivated);
			grdMaster.Grouped += new EventHandler<GridViewGroupedEventArgs>(grdMaster_Grouped);
			BindMaster();
		}

		#endregion// Constructors



		#region Events

		void pgGuides_CommandEvent(ContentPageCommandEventArgs e)
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
					if (MessageBox.Show("Eliminare gli articoli selezionati? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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
					MasterID = (int)((V_Articoli)grd.SelectedItems[0]).Id_Articolo;

				NotifySelection();
			}
		}

		void grdMaster_RowActivated(object sender, RowEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
				if (grd.SelectedItems.Count > 0)
					DoOpenItem(((V_Articoli)grd.SelectedItems[0]), WindowStartupLocation.CenterScreen);
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
					if (MessageBox.Show("Eliminare gli articoli selezionati? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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
			string[] sort = new string[] { "TipoArticolo", "Descrizione" };
			List<V_Articoli> MasterTable = dal.GetFilteredList_V_Articoli(null, sort, "asc", 0, 0, out count);
			grdMaster.DataContext = MasterTable;
		}

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				args.SelectedIDs.Add((int)((V_Articoli)grdMaster.SelectedItems[i]).Id_Articolo);

			OnItemSelectionEvent(args);
		}

		private void DoNewItem()
		{
			BaseDetailPage frm = new wndGoodiesDetail(0);
			frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			frm.ShowDialog();
			BindMaster();
		}

		private void DoOpenItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoOpenItem(((V_Articoli)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
		}

		private void DoOpenItem(V_Articoli obj, WindowStartupLocation startupLocation)
		{
			bool open = true;
			string windowName = string.Format("Guide_{0}", obj.Id_Articolo.ToString());
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
				frm = new wndGoodiesDetail(obj.Id_Articolo);
				frm.User = User;
				frm.Name = windowName;
				frm.WindowStartupLocation = startupLocation;
				//frm.Closed += new EventHandler<Telerik.Windows.Controls.WindowClosedEventArgs>(frm_Closed);
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoDeleteItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoDeleteItem(((V_Articoli)grdMaster.SelectedItems[i]));
		}

		private void DoDeleteItem(V_Articoli obj)
		{
			Articolo_Dal dal = new Articolo_Dal();
			try
			{
				Articolo g = dal.GetItem(obj.Id_Articolo);
				if (g != null)
					dal.DeleteObject(g);
			}
			catch (UpdateException e)
			{
				MessageBox.Show("Impossibile eliminare: fra i record selezionati, alcuni sono associati a uno o più movimenti di magazzino.\nEliminare i movimenti di magazzino associati agli articoli selezionati, quindi riprovare", "Errore eliminazione", MessageBoxButton.OK);
			}
		}

		#endregion// Private Methods

	}
}
